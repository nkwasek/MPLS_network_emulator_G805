using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    class IOPort
    {
        public string RouterName { get; set; }
        public int PortNumber { get; set; }
        public IPAddress CCAdress { get; set; }
        public int CCPort { get; set; }
        public IPEndPoint CCEndPoint { get; set; }
        public Socket CSocket { get; set; }
        public IOPort(string RouterName,int PortNumber)
        {
            this.RouterName = RouterName;
            this.PortNumber = PortNumber;
            Thread thread1 = new Thread(Start);
            thread1.Start();
        }
        public void ConnectToCableClaud()
        {
            CCAdress = LSRConfig.CCIP;
            CCPort = LSRConfig.CCPORT;
            CCEndPoint = new IPEndPoint(CCAdress, CCPort);
            while (true)
            {
                try
                {
                    Socket socket = new Socket(CCAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    var asyncResult = socket.BeginConnect(CCEndPoint, null, null);
                    asyncResult.AsyncWaitHandle.WaitOne(5000, true);
                    if (socket.Connected)
                    {
                        
                        Send($"HELLO {RouterName} {PortNumber}",socket);
                        var message = Receive(socket);

                        if (message.Equals("HELLO"))
                        {
                            CSocket = socket;
                            break;
                        }
                    }
                    else
                    {
                        socket.Close();

                    }
                }
                catch (Exception)
                {
                    
                }
            }

        }
        public void Start()
        {
            ConnectToCableClaud();
            while (true)
            {
                try
                {
                    var message = Receive(CSocket);
                    switch (message)
                    {
                        case "OK":
                            ManagementAgent.ReturnLog("Sent message.");
                            break;
                        case "NOTOK":
                            ManagementAgent.ReturnLog("Unable to send message.");
                            break;
                        default:
                            byte[] msg = Encoding.Default.GetBytes(message);
                            MPLSPacket mpls = new MPLSPacket(msg);
                            ManagementAgent.ReturnLog($"Port: {PortNumber}; Received MPLS Packet: "+mpls.MPLSInfo());
                            SwitchingFabric.SwapLabels(PortNumber, mpls);
                            break;
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        public static void Send(string data,Socket socket)
        {
            byte[] byteData = Encoding.Default.GetBytes(data);
            socket.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), socket);
        }
        public void Send(MPLSPacket mplsp)
        {
            byte[] byteData = mplsp.GetBytes();
            CSocket.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), CSocket);
            ManagementAgent.ReturnLog($"Port {PortNumber} Trying to send MPLS Packet: " + mplsp.MPLSInfo());
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string Receive(Socket client)
        {
            byte[] buffer = new byte[256];
            var message = Encoding.Default.GetString(buffer, 0, client.Receive(buffer));
            return message;
        }
    }
}
