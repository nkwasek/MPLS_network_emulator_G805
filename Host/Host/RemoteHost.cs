using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HostApp
{
    class RemoteHost
    {
        public RemoteHost(string x)
        {
            string[] tmp = x.Split(' ');
            ipAddress = IPAddress.Parse(tmp[0]);
            name = tmp[1];
            label = tmp[2][0];
        }
        private IPAddress ipAddress;
        private string name;
        private char label;
        public string getName() { return name; }
        public char getLabel() { return label; }

    }
}
