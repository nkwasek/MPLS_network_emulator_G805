using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace CableCloud
{
    public class StateObject
    {
        // Rozmiar bufora 
        public const int BufferSize = 1024;

        // Bufor  
        public byte[] buffer = new byte[BufferSize];

        // Odebrana wiadomość
        public StringBuilder sb = new StringBuilder();

        // Socket klienta
        public Socket workSocket = null;


    }

    class CableCloud
    {
        private Socket socket;
        public List<ConnectionsTableRow> ConnectionTable = new List<ConnectionsTableRow>();
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private IPAddress cableCloudAddress = IPAddress.Parse("127.0.0.1");
        private const int cableCloudPort = 1230;
        private static Dictionary<Socket, string> PortNamesKeySocket = new Dictionary<Socket, string>();
        private static Dictionary<string, Socket> PortNamesKeyName = new Dictionary<string, Socket>();
        public string consoleCommand;

        public void StartServer()
        {
            // Utworzenie socketa serwerowego
            socket = new Socket(cableCloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(new IPEndPoint(cableCloudAddress, cableCloudPort));

                // Oczekiwanie na klienta
                socket.Listen(100);
                ReturnLog("Cable Cloud is now working.");

                while (true)
                { 
                    allDone.Reset();
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                    allDone.WaitOne();
                }
            }
            catch (Exception) { ReturnLog("Exception: server unable to establish connection."); }
        }

        public void AcceptCallback(IAsyncResult ar)
        { 
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Utworzenie obiektu stanu  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Odczyt danych od klienta
            int bytesRead;
            try
            {
                bytesRead = handler.EndReceive(ar);

                var message = Encoding.Default.GetString(state.buffer, 0, bytesRead);
                var splitedMessage = message.Split(' ');
                if (splitedMessage[0] == "HELLO" && splitedMessage.Length == 3)
                {
                    Send(handler, "HELLO");
                    string port = splitedMessage[1] + "." + splitedMessage[2];
                    PortNamesKeySocket.Add(handler, port);
                    PortNamesKeyName.Add(port, handler);
                }
                else
                {
                    string firstPort = PortNamesKeySocket[handler];
                    string secondPort = string.Empty;
                    foreach (ConnectionsTableRow ctr in ConnectionTable)
                    {
                        if (ctr.SecondEnd(firstPort) && ctr.IsWorking())
                        {
                            secondPort = ctr.GetSecondEnd();
                        }
                    }
                    if (!secondPort.Equals(string.Empty))
                    {
                        try
                        {
                            Socket endSocket = PortNamesKeyName[secondPort];
                            Send(handler, "OK");
                            Thread.Sleep(1000);
                            Send(endSocket, message);
                            ReturnLog($"Received packet from {firstPort}, sent to {secondPort}");
                        }
                        catch(Exception)
                        {
                            ReturnLog($"Link between {firstPort} and {secondPort} is broken.");
                            Send(handler, "NOTOK");
                        }
                    }
                    else
                    {
                        ReturnLog($"Received packet from {firstPort}. Unable to send.");
                        Send(handler, "NOTOK");
                    }
                }
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception)
            {
                // if the client has been shutdown, then close the connection
                var Name = PortNamesKeySocket[handler];
                PortNamesKeySocket.Remove(handler);
                PortNamesKeyName.Remove(Name);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                ReturnLog($"Port {Name} stopped working.");
                return;
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Konwersja 
            byte[] byteData = Encoding.Default.GetBytes(data);

            // Rozpoczęcie przesyłania danych do klienta
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }



        private static void SendCallback(IAsyncResult ar)
        {
            try
            { 
                Socket handler = (Socket)ar.AsyncState;

                // Zakończenie wysyłania danych do klienta
                int bytesSent = handler.EndSend(ar);
                
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        private static void ReturnLog(string log)
        {
            Console.WriteLine($"[{DateTime.Now}]" + " " + log + "\n---------------------");
        }
        public void LoadConfig(string path)
        {
            string[] message = File.ReadAllLines(path);
            foreach (string line in message)
            {
                string[] splitedLine = line.Split(' ');
                ConnectionTable.Add(new ConnectionsTableRow(splitedLine[0], splitedLine[1], splitedLine[2], splitedLine[3], splitedLine[4]));
                ConnectionTable.Add(new ConnectionsTableRow(splitedLine[2], splitedLine[3], splitedLine[0], splitedLine[1], splitedLine[4]));
            }
        }
        private void ChangeState(string firstPort)
        {
            string secondPort = string.Empty;
            foreach (ConnectionsTableRow ctr in ConnectionTable)
            {
                if (ctr.SecondEnd(firstPort))
                {
                    secondPort = ctr.GetSecondEnd();
                    ctr.ChangeWorking();
                    if(ctr.IsWorking())
                    {
                        ReturnLog($"Connection between {firstPort} and {secondPort} is working now.");
                    }
                    else
                    {
                        ReturnLog($"Connection between {firstPort} and {secondPort} is broken now.");
                    }
                }
            }
            if(secondPort!=null)
            {
                foreach (ConnectionsTableRow ctr in ConnectionTable)
                {
                    if (ctr.SecondEnd(secondPort))
                    {
                        ctr.ChangeWorking();
                    }
                }
            }
            else
            {
                ReturnLog("Connection doesn't exist.");
            }

        }

        private void ReadCommand(string command)
        {
            // Odczyt komendy wczytanej z konsoli
            string[] splitedCommand = command.Split(' ');
            try
            {
                if (splitedCommand[0] == "CHANGESTATE")
                {
                    ChangeState(splitedCommand[1]);
                }
                else if (splitedCommand[0] == "SHOWCONNECTIONS")
                {
                    foreach (ConnectionsTableRow ctr in ConnectionTable)
                    {
                        ctr.ShowRecord();
                    }

                }
                else { ReturnLog("Unknown command"); }
            }
            catch (Exception)
            {
                ReturnLog("Unknown command");
            }
        }

        public void ReadCommandLines()
        {
            // Wczytywanie komendy z konsoli

            while (true)
            {
                consoleCommand = Console.ReadLine();
                ReadCommand(consoleCommand);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine();
            //  Invoke this sample with an arbitrary set of command line arguments.
            string[] arguments = Environment.GetCommandLineArgs();
            CableCloud cableCloud = new CableCloud();
            cableCloud.LoadConfig(arguments[1]);
            Thread reading = new Thread(cableCloud.ReadCommandLines);
            Thread thread = new Thread(cableCloud.StartServer);
            thread.Start();
            reading.Start();
        }
    }
}
