using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabelSwitchingRouter;

namespace LabelSwitchingRouter
{
    class LabelSwitchingRouter
    {
        public string Name { get; set; }
        public ManagementAgent MA { get; set; } = new ManagementAgent();
        public static List<IOPort> IOPorts { get; set; } = new List<IOPort>();
        public void StartManagementAgent()
        {
            MA.Start();
        }
        public void LoadData()
        {
            Name = LSRConfig.LSRNAME;
            foreach(int port in LSRConfig.PORTS)
            {
                IOPorts.Add(new IOPort(Name, port));
            }
        }
        public static IOPort ReturnPort(int port)
        {
            foreach (IOPort iop in IOPorts)
            {
                if (iop.PortNumber == port) { return iop; }

            }
            return null;
        }

        static void Main(string[] args)
        {
            Console.WriteLine();
            //  Invoke this sample with an arbitrary set of command line arguments.
            string[] arguments = Environment.GetCommandLineArgs();
            try
            {
                LSRConfig.LoadConfig(arguments[1]);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                Environment.ExitCode = 0;
            }
            ManagementAgent.ReturnLog($"Router {LSRConfig.LSRNAME} is working.");
            LabelSwitchingRouter LSR = new LabelSwitchingRouter();
            LSR.LoadData();
            LSR.StartManagementAgent();
            Console.ReadLine();
        }
    }
}
