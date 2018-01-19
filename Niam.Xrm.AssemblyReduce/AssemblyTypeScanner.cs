using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Niam.Xrm.AssemblyReduce
{
    internal class AssemblyTypeScanner
    {
        private readonly HashSet<string> _usedTypeIds;
        private readonly AssemblyDefinition _assemblyDefinition;

        public IEnumerable<string> UsedTypeIds => _usedTypeIds;

        public AssemblyTypeScanner(AssemblyDefinition assemblyDefinition, string[] keepTypePatterns)
        {
            _assemblyDefinition = assemblyDefinition ?? throw new ArgumentNullException(nameof(assemblyDefinition));
            _usedTypeIds = GetInitialUsedTypeIds(assemblyDefinition, keepTypePatterns);
        }

        private static HashSet<string> GetInitialUsedTypeIds(AssemblyDefinition assemblyDefinition, string[] keepTypeIdPatterns)
        {
            var ids = new HashSet<string> { "<Module>" };

            if (assemblyDefinition.EntryPoint != null)
                ScanType(assemblyDefinition.EntryPoint.DeclaringType, ids);

            var selfCustomAttributeTypes = assemblyDefinition.CustomAttributes
                .Select(ca => ca.AttributeType)
                .Where(t => t.Scope.MetadataScopeType == MetadataScopeType.ModuleDefinition);

            var compilerGeneratedTypes = assemblyDefinition.MainModule.Types
                .Where(IsCompilerGenerated);

            var keepRegexs = keepTypeIdPatterns
                .Select(s =>
                {
                    var pattern = s.Contains("*") ? s.Replace("*", ".*") : $"^{s}$";
                    pattern = pattern.Replace(".", "\\.");
                    return new Regex(pattern);
                }).ToArray();
            var keepTypes = assemblyDefinition.MainModule.Types.Where(t => keepRegexs.Any(r => r.IsMatch(t.FullName)));

            var initTypes = selfCustomAttributeTypes
                .Concat(compilerGeneratedTypes)
                .Concat(keepTypes);
            foreach (var type in initTypes)
                ScanType(type, ids);

            return ids;
        }

        public void ScanFromPluginTypes()
        {
            foreach (var pluginType in GetPluginTypes())
                ScanType(pluginType);
        }

        private IEnumerable<TypeDefinition> GetPluginTypes()
        {
            return _assemblyDefinition.MainModule.Types
                .Where(t => !t.IsAbstract)
                .Where(t => t.IsPublic)
                .Where(t => IsImplementInterface(t, "Microsoft.Xrm.Sdk.IPlugin"));
        }

        public void ScanType(TypeReference entryTypeRef) => ScanType(entryTypeRef, _usedTypeIds);

        private static void ScanType(TypeReference entryTypeRef, HashSet<string> usedTypeIds)
        {
            if (entryTypeRef == null) return;
            if (entryTypeRef.Scope.MetadataScopeType != MetadataScopeType.ModuleDefinition)
            {
                if (entryTypeRef is GenericInstanceType g)
                {
                    foreach (var typeRef in g.GenericArguments.Where(t => t.Scope.MetadataScopeType == MetadataScopeType.ModuleDefinition))
                        ScanType(typeRef, usedTypeIds);
                }

                return;
            }

            var typeDef = entryTypeRef.Resolve();
            var fullName = typeDef?.FullName ?? entryTypeRef.FullName;
            if (usedTypeIds.Contains(fullName)) return;

            usedTypeIds.Add(fullName);
            if (typeDef == null) return;

            var methodDefs = typeDef.Methods
                .Concat(typeDef.Events.Where(ed => ed.AddMethod != null).Select(ed => ed.AddMethod))
                .Concat(typeDef.Events.Where(ed => ed.RemoveMethod != null).Select(ed => ed.RemoveMethod))
                .Concat(typeDef.Properties.Where(pd => pd.GetMethod != null).Select(pd => pd.GetMethod))
                .Concat(typeDef.Properties.Where(pd => pd.SetMethod != null).Select(pd => pd.SetMethod));
            var bodies = methodDefs.Where(md => md.HasBody).Select(md => md.Body).ToArray();
            var operands = bodies.SelectMany(b => b.Instructions).Select(i => i.Operand).ToArray();
            var customAttributeTypes = typeDef.CustomAttributes
                .Concat(typeDef.Fields.SelectMany(fd => fd.CustomAttributes))
                .Concat(typeDef.Properties.SelectMany(pd => pd.CustomAttributes))
                .Concat(typeDef.Events.SelectMany(ed => ed.CustomAttributes))
                .Concat(methodDefs.SelectMany(md => md.CustomAttributes))
                .Concat(methodDefs.SelectMany(md => md.Parameters).SelectMany(md => md.CustomAttributes));

            IEnumerable<TypeReference> usedTypes = new[] { typeDef.BaseType }
                .Concat(customAttributeTypes.Select(ca => ca.AttributeType))
                .Concat(typeDef.GenericParameters.SelectMany(gp => gp.Constraints))
                .Concat(typeDef.Interfaces.Select(ii => ii.InterfaceType))
                .Concat(typeDef.Fields.Select(fd => fd.FieldType))
                .Concat(methodDefs.Select(md => md.ReturnType))
                .Concat(methodDefs.SelectMany(md => md.Parameters).Select(pd => pd.ParameterType))
                .Concat(methodDefs.SelectMany(md => md.GenericParameters).SelectMany(gp => gp.Constraints))
                .Concat(bodies.SelectMany(b => b.Variables).Select(vd => vd.VariableType))
                .Concat(operands.OfType<TypeReference>())
                .Concat(operands.OfType<MemberReference>().Select(mr => mr.DeclaringType))
                .Concat(operands.OfType<GenericInstanceMethod>().SelectMany(gim => gim.GenericArguments));

            if (entryTypeRef is GenericInstanceType git)
                usedTypes = usedTypes.Union(git.GenericArguments);

            foreach (var usedType in usedTypes)
                ScanType(usedType, usedTypeIds);
        }

        private static bool IsImplementInterface(TypeDefinition typeDef, string fullName)
        {
            do
            {
                if (typeDef.Interfaces.Any(i => i.InterfaceType.FullName == fullName))
                    return true;

                typeDef = typeDef.BaseType?.Resolve();
            }
            while (typeDef != null);

            return false;
        }

        private static bool IsCompilerGenerated(TypeDefinition typeDef)
        {
            var isCompilerGenerated = typeDef.HasCustomAttributes &&
                typeDef.CustomAttributes
                    .Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            return isCompilerGenerated;
        }
    }
}
