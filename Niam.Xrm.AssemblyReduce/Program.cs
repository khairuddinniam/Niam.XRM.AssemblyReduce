
namespace Niam.Xrm.AssemblyReduce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = AssemblyReducerSettings.ParseArguments(args);
            var reducer = new AssemblyReducer(settings);
            reducer.Execute();
        }
    }
}
