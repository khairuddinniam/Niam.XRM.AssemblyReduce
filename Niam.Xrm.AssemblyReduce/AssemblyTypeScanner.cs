using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niam.Xrm.AssemblyReduce
{
    internal class AssemblyTypeScanner
    {
        private readonly HashSet<string> _usedTypeIds;
        private readonly AssemblyDefinition _assemblyDefinition;

        public IEnumerable<string> UsedTypeIds => _usedTypeIds;

        public AssemblyTypeScanner(AssemblyDefinition assemblyDefinition)
        {
            _assemblyDefinition = assemblyDefinition ?? throw new ArgumentNullException(nameof(assemblyDefinition));
            var compilerGeneratedTypes = _assemblyDefinition.MainModule.Types
                .Where(IsCompilerGenerated)
                .Select(t => t.FullName);
            _usedTypeIds = new HashSet<string>(compilerGeneratedTypes) { "<Module>" };
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

        public void ScanType(TypeReference entryTypeRef)
        {
            if (entryTypeRef == null) return;
            if (entryTypeRef.Scope.MetadataScopeType != MetadataScopeType.ModuleDefinition) return;
            
            var typeDef = entryTypeRef.Resolve();
            if (_usedTypeIds.Contains(typeDef.FullName)) return;

            _usedTypeIds.Add(typeDef.FullName);

            var methodDefs = typeDef.Methods
                .Concat(typeDef.Properties.Where(pd => pd.GetMethod != null).Select(pd => pd.GetMethod))
                .Concat(typeDef.Properties.Where(pd => pd.SetMethod != null).Select(pd => pd.SetMethod));
            var bodies = methodDefs.Where(md => md.HasBody).Select(md => md.Body).ToArray();
            var operands = bodies.SelectMany(b => b.Instructions).Select(i => i.Operand).ToArray();

            IEnumerable<TypeReference> usedTypes = new[] { typeDef.BaseType }
                .Concat(typeDef.Interfaces.Select(ii => ii.InterfaceType))
                .Concat(typeDef.Fields.Select(fd => fd.FieldType))
                .Concat(methodDefs.Select(md => md.ReturnType))
                .Concat(methodDefs.SelectMany(md => md.Parameters).Select(pd => pd.ParameterType))
                .Concat(bodies.SelectMany(b => b.Variables).Select(vd => vd.VariableType))
                .Concat(operands.OfType<GenericInstanceMethod>().SelectMany(gim => gim.GenericArguments))
                .Concat(operands.OfType<MemberReference>().Select(mr => mr.DeclaringType));

            if (entryTypeRef is GenericInstanceType git)
                usedTypes = usedTypes.Union(git.GenericArguments);

            foreach (var usedType in usedTypes)
                ScanType(usedType);
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
