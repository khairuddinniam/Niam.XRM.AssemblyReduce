using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Niam.Xrm.AssemblyReduce.Tests
{
    public class AssemblyReducerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly byte[] _expectedPublicKey;
        private readonly byte[] _expectedPublicKeyToken;

        public AssemblyReducerTests(ITestOutputHelper output)
        {
            _output = output ?? throw new System.ArgumentNullException(nameof(output));

            var fileName = "Niam.Xrm.TestAssembly";
            var assemblyName = Assembly.LoadFrom($"{fileName}.dll").GetName();
            _expectedPublicKey = assemblyName.GetPublicKey();
            _expectedPublicKeyToken = assemblyName.GetPublicKeyToken();

            File.Copy($"{fileName}.dll", $"{fileName}.test.dll", true);
            var pdbFileName = $"{fileName}.pdb";
            if (File.Exists(pdbFileName))
                File.Copy(pdbFileName, $"{fileName}.test.pdb", true);
        }

        [Fact]
        public void Can_remove_unused_types()
        {
            var fileName = "Niam.Xrm.TestAssembly.test.dll";
            var settings = new AssemblyReducerSettings
            {
                Input = fileName,
                StrongNameKey = "open-source.snk"
            };

            new AssemblyReducer(settings).Execute();

            var testAssembly = Assembly.LoadFrom(fileName);
            var testTypes = testAssembly.GetTypes();
            foreach (var testType in testTypes.OrderBy(t => t.FullName))
                _output.WriteLine($"- {testType.FullName}");

            var testAssemblyName = testAssembly.GetName();
            Assert.Equal(_expectedPublicKey, testAssemblyName.GetPublicKey());
            Assert.Equal(_expectedPublicKeyToken, testAssemblyName.GetPublicKeyToken());

            Assert.DoesNotContain(testTypes, t => t.FullName == "Niam.Xrm.TestAssembly.UnusedClass");

            var existingTypes = new[]
            {
                "Niam.Xrm.TestAssembly.UsedAsGenericParamConstraintClass",
                "Niam.Xrm.TestAssembly.UsedAsGenericParamConstraintClass2",

                "Niam.Xrm.TestAssembly.DirectImplementPlugin",
                "Niam.Xrm.TestAssembly.PluginBase",
                "Niam.Xrm.TestAssembly.UsingCustomBasePlugin",
                "Niam.Xrm.TestAssembly.PluginBaseGeneric`1",
                "Niam.Xrm.TestAssembly.GenericPlugin`1"
            };

            foreach (var fullname in existingTypes)
                Assert.Contains(testTypes, t => t.FullName == fullname);
        }
    }
}
