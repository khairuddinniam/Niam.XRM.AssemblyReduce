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

        public AssemblyReducerTests(ITestOutputHelper output)
        {
            _output = output ?? throw new System.ArgumentNullException(nameof(output));

            var fileName = "Niam.Xrm.TestAssembly";
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
                Input = fileName
            };

            new AssemblyReducer(settings).Execute();

            var testAssembly = Assembly.LoadFrom(fileName);
            var testTypes = testAssembly.GetTypes();
            foreach (var testType in testTypes.OrderBy(t => t.FullName))
                _output.WriteLine($"- {testType.FullName}");

            Assert.DoesNotContain(testTypes, t => t.FullName == "Niam.Xrm.TestAssembly.UnusedClass");
        }
    }
}
