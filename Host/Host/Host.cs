using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace HostApp
{

    class Host
    {
        private string name;
        private IPAddress ipAddress;
        private int port;
        private int cloudPort;
        private IPAddress cloudIP;
        private Socket clientSocket;
        Thread commandThread;
        Thread receiveThread;
        String response = String.Empty;

        public void StartHost()
        {


            ReturnLog($"Starting Host {name}.");
            IPEndPoint hostEP = new IPEndPoint(cloudIP, cloudPort);
            while (true)
            {
                try
                {
                    clientSocket = new Socket(cloudIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    var asyncResult = clientSocket.BeginConnect(hostEP, null, null);
                    asyncResult.AsyncWaitHandle.WaitOne(5000, true);

                    if (clientSocket.Connected)
                    {
                        ReturnLog("Trying to connect to Cable Cloud");
                        clientSocket.Send(Encoding.Default.GetBytes($"HELLO {name} {port}"));
                        var message = Receive(clientSocket);

                        if (message.Contains("HELLO"))
                        {
                            ReturnLog("Connected with Cable Cloud");
                            
                            commandThread = new Thread(MakePacket);
                            receiveThread = new Thread(ReceivePacket);
                            commandThread.Start();
                            receiveThread.Start();
                            break;
                        }
                        Console.WriteLine("co jest");
                    }
                    else
                    {
                        clientSocket.Close();
                        ReturnLog("Timeout. Trying to connect again...");
                    }
                }
                catch (Exception)
                {
                    ReturnLog("Trying to connect again...");
                }
            }


        }

        public void MakePacket()
        {
           
            while (true)
            {

                string rl = Console.ReadLine();
                string[] tmp = rl.Split(' ');
                int len = tmp.Length;
                string msg = String.Empty;
                if (len>2)
                {
                   msg = tmp[1];
                }
                for(int i=2;i<len-1;i++)
                {
                    msg += " "+tmp[i];
                }
                if (tmp[0] == "msg")
                {
                    SendPacket(msg, 255, byte.Parse(tmp[len-1]));
                }
                else
                if (tmp[0] != "msg")
                {
                    ReturnLog("Unknown command");

                }
                else
                    ReturnLog("Can't send the message");

            }

        }
        public void SendPacket(string msg, byte ttl, byte label)
        {

            List<byte> labels = new List<byte>();
            labels.Add(label);
            if (clientSocket.Connected)
            {
               
                MPLSPacket packet = new MPLSPacket(msg, ttl, labels, name);
                Console.WriteLine(packet.MPLSInfo());
                try
                {
                    
                    ReturnLog("Trying to send the message...");
                    Send(clientSocket,packet.GetBytes());
                    Thread.Sleep(1000);
                    if (response.Equals("OK"))
                    {
                        ReturnLog("Message was sent");
                    }
                    else
                    {
                        ReturnLog("Failed to send the message");
                    }
                    response = String.Empty;
                    
                }
                catch (Exception)
                {
                    ReturnLog("Failed to send the message");
                }

            }
        }
       
        public void ReceivePacket()
        {
            //sprawdzac czy sa dane do odebrania
            while(true)
            {
                if (clientSocket.Connected)
                {
                    byte[] buffer = new byte[256];
                    var message = Encoding.Default.GetString(buffer, 0, clientSocket.Receive(buffer));
                    if (message != null && message.Length > 5)
                    {
                        byte[] buffer2 = Encoding.Default.GetBytes(message);

                        MPLSPacket packet = new MPLSPacket(buffer2);
                        ReturnLog("Message received: " + packet.Message + " from " + packet.Source);
                    }
                    else
                        if (message != null)
                    {
                        response = message;
                    }
                        
                    
                }
            }
            
        }
        
        
        private static void ReturnLog(string log)
        {
            Console.WriteLine($"[{DateTime.Now}]" + " " + log + "\n---------------------");
        }
        public void ReadConfig(string path)
        {
            XmlReader reader = new XmlTextReader(path);

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name.ToString())
                    {
                        case "NAME":
                            name = reader.ReadString();
                            break;
                        case "IP":
                            ipAddress = IPAddress.Parse(reader.ReadString());
                            break;
                        case "PORT":
                            port = int.Parse(reader.ReadString());
                            break;
                        case "CLOUDIP":
                            cloudIP = IPAddress.Parse(reader.ReadString());
                            break;
                        case "CLOUDPORT":
                            cloudPort = int.Parse(reader.ReadString());
                            break;
                        

                    }
                }
            }
        }
        private String Receive(Socket client)
        {
            byte[] buffer = new byte[256];
            var msg = Encoding.Default.GetString(buffer, 0, client.Receive(buffer));
            return msg;

        }
       
        private static void Send(Socket client, byte[] byteData)
        {
            client.Send(byteData);
        }

    }
}
