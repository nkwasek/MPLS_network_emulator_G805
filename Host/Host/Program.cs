using System;
using System.Threading;

namespace HostApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            //  Invoke this sample with an arbitrary set of command line arguments.
            string[] arguments = Environment.GetCommandLineArgs();
            //string arguments = "H01Config.xml";
            Host host = new Host();
            host.ReadConfig(arguments[1]);
             //host.ReadConfig(arguments);
            host.StartHost();
            
        }
    }
}
