using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Niam.Xrm.AssemblyReduce
{
    public class AssemblyReducer
    {
        private readonly AssemblyReducerSettings _settings;

        public AssemblyReducer(AssemblyReducerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Execute()
        {
            var readingParams = new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                InMemory = true
            };

            using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(_settings.Input, readingParams))
            {
                var scanner = new AssemblyTypeScanner(assemblyDefinition);
                scanner.ScanFromPluginTypes();
                RemoveUnusedTypes(assemblyDefinition, scanner.UsedTypeIds);

                assemblyDefinition.Write(_settings.Output);
            }
        }

        private void RemoveUnusedTypes(AssemblyDefinition assemblyDefinition, IEnumerable<string> usedTypeIds)
        {
            var types = assemblyDefinition.MainModule.Types;
            var unusedTypes = types.Where(t => !usedTypeIds.Contains(t.FullName)).ToArray();
            foreach (var unusedType in unusedTypes)
                types.Remove(unusedType);
        }
    }
}
