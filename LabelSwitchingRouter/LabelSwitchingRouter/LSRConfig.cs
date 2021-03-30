using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    
    class LSRConfig
    {
        public static string LSRNAME { get; set; }
        public static IPAddress MCIP { get; set; }
        public static int MCPORT { get; set; }
        public static IPAddress CCIP { get; set; }
        public static int CCPORT { get; set; }
        public static List<int> PORTS { get; set; } = new List<int>();


        public static void LoadConfig(string path)
        {
            string[] message = File.ReadAllLines(path);
            foreach (string line in message)
            {
                string[] splitedLine = line.Split(' ');
                switch (splitedLine[0])
                {
                    case "NAME":
                        LSRNAME = splitedLine[1];
                        break;
                    case "MCIP":
                        MCIP = IPAddress.Parse(splitedLine[1]);
                        break;
                    case "MCPORT":
                        MCPORT = Convert.ToInt32(splitedLine[1]);
                        break;
                    case "CCIP":
                        CCIP = IPAddress.Parse(splitedLine[1]);
                        break;
                    case "CCPORT":
                        CCPORT = Convert.ToInt32(splitedLine[1]);
                        break;
                    case "PORT":
                        PORTS.Add(Convert.ToInt32(splitedLine[1]));
                        break;
                    default:
                        throw new ArgumentException("Cannot load LSR config.");
                }
            }
        }
 
    }
}
