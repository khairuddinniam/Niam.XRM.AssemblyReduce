using System;
using System.Linq;
using Xunit;

namespace Niam.Xrm.AssemblyReduce.Tests
{
    public class AssemblyReducerSettingsTests
    {
        [Theory]
        [InlineData("-i=hello")]
        [InlineData("--i=hello")]
        [InlineData("-input=hello")]
        [InlineData("--input=hello")]
        public void Output_is_same_as_input_when_not_specified(string args)
        {
            var settings = AssemblyReducerSettings.ParseArguments(args.Split(' ').ToArray());
            Assert.Equal("hello", settings.Input);
            Assert.Equal("hello", settings.Output);
        }

        [Theory]
        [InlineData("-i=hello --output=world")]
        [InlineData("--i=hello -output=world")]
        [InlineData("-input=hello --o=world")]
        [InlineData("--input=hello -o=world")]
        public void Output_is_different_from_input_when_is_specified(string args)
        {
            var settings = AssemblyReducerSettings.ParseArguments(args.Split(' ').ToArray());
            Assert.Equal("hello", settings.Input);
            Assert.Equal("world", settings.Output);
        }

        [Theory]
        [InlineData("-kt=first,second")]
        [InlineData("--kt=first,second")]
        [InlineData("-keeptypes=first,second")]
        [InlineData("--keeptypes=first,second")]
        public void Can_parse_namespaces(string keeptypes)
        {
            var settings = AssemblyReducerSettings.ParseArguments(new[] { keeptypes });
            Assert.Equal(new[] { "first", "second" }, settings.KeepTypes);
        }

        [Theory]
        [InlineData("-snk=path/to/snk/file")]
        [InlineData("--snk=path/to/snk/file")]
        [InlineData("-strong-name-key=path/to/snk/file")]
        [InlineData("--strong-name-key=path/to/snk/file")]
        public void Can_parse_strong_name_key(string args)
        {
            var settings = AssemblyReducerSettings.ParseArguments(args.Split(' ').ToArray());
            Assert.Equal("path/to/snk/file", settings.StrongNameKey);
        }

        [Theory]
        [InlineData("not_exist.dll")]
        [InlineData(null)]
        public void Invalid_input_path(string input)
        {
            var settings = new AssemblyReducerSettings
            {
                Input = input
            };
            var ex = Assert.Throws<ArgumentException>(() => settings.Validate());
            Assert.Equal("input file not found.", ex.Message);
        }

        [Fact]
        public void Invalid_strong_name_key_path()
        {
            var settings = new AssemblyReducerSettings
            {
                Input = "Niam.Xrm.AssemblyReduce.Tests.dll",
                StrongNameKey = "not_exist.dll"
            };
            var ex = Assert.Throws<ArgumentException>(() => settings.Validate());
            Assert.Equal("strong name key file not found.", ex.Message);
        }
    }
}
