
namespace Niam.Xrm.AssemblyReduce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = AssemblyReducerSettings.ParseArguments(args);
            settings.Validate();
            var reducer = new AssemblyReducer(settings);
            reducer.Execute();
        }
    }
}
