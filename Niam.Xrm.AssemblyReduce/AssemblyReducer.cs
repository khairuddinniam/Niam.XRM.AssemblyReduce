using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
            using (var assemblyResolver = CreateAssemblyResolver())
            {
                var readerParams = CreateReaderParameters(assemblyResolver);
                using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(_settings.Input, readerParams))
                {
                    var scanner = new AssemblyTypeScanner(assemblyDefinition, _settings.KeepTypes);
                    scanner.ScanFromPluginTypes();
                    RemoveUnusedTypes(assemblyDefinition, scanner.UsedTypeIds);

                    var writerParams = CreateWriterParameters(readerParams);
                    var sameFile = _settings.Input == _settings.Output;
                    if (sameFile)
                        assemblyDefinition.Write(writerParams);
                    else
                        assemblyDefinition.Write(_settings.Output, writerParams);
                }
            }
        }

        private IAssemblyResolver CreateAssemblyResolver()
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(_settings.Input));
            return assemblyResolver;
        }

        private ReaderParameters CreateReaderParameters(IAssemblyResolver assemblyResolver)
        {
            var sameFile = _settings.Input == _settings.Output;
            var haveSymbols = File.Exists(Path.ChangeExtension(_settings.Input, "pdb")) ||
                File.Exists(Path.ChangeExtension(_settings.Input, "mdb"));
            var readingParams = new ReaderParameters
            {
                ReadSymbols = haveSymbols,
                ReadWrite = sameFile,
                AssemblyResolver = assemblyResolver
            };

            return readingParams;
        }

        private WriterParameters CreateWriterParameters(ReaderParameters readerParameters)
        {
            var writerParams = new WriterParameters { WriteSymbols = readerParameters.ReadSymbols };
            if (_settings.StrongNameKey != null)
                writerParams.StrongNameKeyPair = new StrongNameKeyPair(File.ReadAllBytes(_settings.StrongNameKey));

            return writerParams;
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
