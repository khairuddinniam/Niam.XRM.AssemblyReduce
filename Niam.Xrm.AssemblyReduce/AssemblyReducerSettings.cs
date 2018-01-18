using NDesk.Options;

namespace Niam.Xrm.AssemblyReduce
{
    public class AssemblyReducerSettings
    {
        private string _output;

        public string Input { get; set; }
        public string Output
        {
            get => _output ?? Input;
            set => _output = value;
        }
        public string[] KeepTypes { get; set; } = new string[0];
        public string StrongNameKey { get; set; }

        public static AssemblyReducerSettings ParseArguments(string[] args)
        {
            var settings = new AssemblyReducerSettings();
            var p = new OptionSet() {
                { "i|input=", v => settings.Input = v },
                { "o|output=", v => settings.Output = v },
                { "kt|keeptypes=", v => settings.KeepTypes = v.Split(',') },
                { "snk|strong-name-key=", v => settings.StrongNameKey = v }
            };
            var extra = p.Parse(args);
                        
            return settings;
        }
    }
}
