using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static LabelSwitchingRouter.LabelSwitchingRouter;

namespace LabelSwitchingRouter
{
    class ManagementAgent
    {
        public IPAddress MCAdress { get; set; }
        public int MCPort { get; set; }
        public IPEndPoint MCEndPoint { get; set; }
        public Socket CSocket { get; set; }
        public string Name { get; set; }
        
        public void ConnectToManagementCenter()
        {
            ReturnLog("Trying to connect with Management Center.");
            MCAdress = LSRConfig.MCIP;
            MCPort = LSRConfig.MCPORT;
            MCEndPoint = new IPEndPoint(MCAdress, MCPort);
            Name = LSRConfig.LSRNAME;
            while (true)
            {
                try
                {
                    Socket socket = new Socket(MCAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    var asyncResult = socket.BeginConnect(MCEndPoint,null,null);
                    asyncResult.AsyncWaitHandle.WaitOne(5000,true);
                    if (socket.Connected)
                    {
                        ReturnLog("Sending Hello to Management Center.");
                        Send(socket,$"HELLO {Name}");
                        var message = Receive(socket);
                        if(message.Contains("HELLO"))
                        {
                            ReturnLog("Connected with Management Center");
                            CSocket = socket;
                            break;
                        }
                    }
                    else
                    {
                        socket.Close();
                        //ReturnLog("Timeout. Trying to reconnect.");
                    }
                }
                catch (Exception)
                {
                    ReturnLog("Trying to reconnect.");
                }
            }
        }
        public void Start()
        {
            ReturnLog("Managment agent start working.");
            ConnectToManagementCenter();
            while (true)
            {
                try
                {
                    var message = Receive(CSocket);
                    var splittedMessage = message.Split(' ');
                    ReturnLog($"Received message from Management Center: {message}");
                    switch (splittedMessage[0])
                    {
                        case "ADDRECORD":
                            MPLSFibTable.AddElement(new MPLSFibRecord(
                                splittedMessage[1]+" "+splittedMessage[2]+" "+splittedMessage[3]+" "+splittedMessage[4]+" " +splittedMessage[5]));
                            ReturnLog($"Number of MPLSFibTable elements: {MPLSFibTable.MplsFibTable.Count}");
                            break;
                        case "REMOVERECORD":
                            MPLSFibTable.RemoveElement(new MPLSFibRecord(
                                splittedMessage[1] + " " + splittedMessage[2] + " " + splittedMessage[3] + " " + splittedMessage[4] + " " + splittedMessage[5]));
                            ReturnLog($"Number of MPLSFibTable elements: {MPLSFibTable.MplsFibTable.Count}");
                            break;
                        default:
                            ReturnLog("Received unknown command from Management Center");
                            break;
                    }
                }
                catch(Exception e)
                {

                }
            }
        }

        public static void ReturnLog(string log)
        {
            Console.WriteLine($"[{DateTime.Now}]" +" "+log + "\n---------------------");
        }

        private static void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            client.Send(byteData);
        }

   
        private static string Receive(Socket client)
        {
            byte[] buffer = new byte[256];
            var message = Encoding.ASCII.GetString(buffer,0,client.Receive(buffer));
            return message;
        }
    }

}
