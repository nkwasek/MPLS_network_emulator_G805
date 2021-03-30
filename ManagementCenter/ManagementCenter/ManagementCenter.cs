using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace ManagementCenter
{
    public class StateObject
    {
        // Rozmiar bufora 
        public const int BufferSize = 1024;

        // Bufor
        public byte[] buffer = new byte[BufferSize];

        // Otrzymane dane
        public StringBuilder sb = new StringBuilder();

        // Socket klienta
        public Socket workSocket = null;

    }

    class ManagementCenter
    {
        private Socket serverSocket;
        private IPAddress managementSystemAddress = IPAddress.Parse("127.0.0.1");
        private const int ManagementSystemPort = 1225;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        public string consoleCommand;

        // Słowniki z socketami klienckimi i ich nazwami
        private static Dictionary<Socket, string> ClientNamesKeySocket = new Dictionary<Socket, string>();
        private static Dictionary<string, Socket> ClientNamesKeyName = new Dictionary<string, Socket>();

        // Tablica MPLS FIB
        private static List<MplsFIBRecord> MplsFibTable = new List<MplsFIBRecord>();

        // Konstruktor
        public ManagementCenter()
        {
            
        }

        public void StartServerSocket()
        {
            // Utworzenie socketu serwera
            serverSocket = new Socket(managementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ReturnLog("Created server socket");

            try
            {
                serverSocket.Bind(new IPEndPoint(managementSystemAddress, ManagementSystemPort));
                // Oczekiwanie na klienta
                serverSocket.Listen(100);
                ReturnLog("Waiting for connection");
                
                while (true)
                {
                    allDone.Reset();
                    serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
                    allDone.WaitOne();
                }
            }
            catch (Exception) { ReturnLog("Exception: server unable to establish connection"); }
        }

        private void AcceptCallback(IAsyncResult ar)
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

        private void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {


                // Odczyt danych od klienta
                int bytesRead = handler.EndReceive(ar);
                var message = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                var splitedMessage = message.Split(' ');

                ClientNamesKeySocket.Add(handler, splitedMessage[1]);
                ClientNamesKeyName.Add(splitedMessage[1], handler);

                ReturnLog("Received message: " + splitedMessage[0] + "\tFrom: " + ClientNamesKeySocket[handler]);

                if (splitedMessage[0] == "HELLO")
                {
                    Send(handler, "HELLO");
                    SendConfig(handler);
                }
            }
            catch(Exception)
            {
                // if the client has been shutdown, then close the connection
                var Name = ClientNamesKeySocket[handler];
                ClientNamesKeySocket.Remove(handler);
                ClientNamesKeyName.Remove(Name);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }
        }

        private static void Send(Socket handler, string data)
        {
            // Konwersja danych typu String na wartości w kodzie binarnym
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Rozpoczęcie przesyłania danych
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

            ReturnLog("Sent message: " + data + "\t To: " + ClientNamesKeySocket[handler]);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Odzyskanie socketu z obiektu stanu.
                Socket handler = (Socket)ar.AsyncState;

                // Zakończenie wysyłania danych do urządzenia.
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public static void ReadConfig(string path)
        {
            try
            {
                // Wczytujemy liniami cały plik konfiguracyjny
                string[] config = File.ReadAllLines(path);
                foreach(string line in config)
                {
                    // Dodajemy rekordy do tablicy MPLS FIB
                    MplsFibTable.Add(new MplsFIBRecord(line));
                }
                ReturnLog("Configuration file loaded successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "\n");
                ReturnLog("Cannot load the configuration file");
            }
        }

        private void SendConfig(Socket handler)
        {
            // Wysyłamy odpowiednie rekordy tablicy MPLS FIB dla danego węzła sieciowego
            foreach(MplsFIBRecord record in MplsFibTable)
            {
                if(record.RouterName == ClientNamesKeySocket[handler])
                {
                    Thread.Sleep(1);
                    Send(handler,"ADDRECORD " + record.send());
                }
            }
        }

        private void ReadCommand(string command)
        {
            // Odczyt komendy wczytanej z konsoli
            string[] splitedCommand = command.Split(' ');
            try
            {
                string record = splitedCommand[7] + " " + splitedCommand[1] + " " + splitedCommand[2]
                    + " " + splitedCommand[3] + " " + splitedCommand[4] + " " + splitedCommand[5];

                if (splitedCommand[0] == "ADDRECORD") { AddRecord(record, splitedCommand[7]); }

                else if (splitedCommand[0] == "REMOVERECORD") { RemoveRecord(record, splitedCommand[7]); }

                else { ReturnLog("Unknown command"); }
            }
            catch(Exception)
            {
                ReturnLog("Unknown command");
            }
        }

        private void AddRecord(string command, string clientName)
        {
            // Dodawanie rekordu do tablicy MPLS FIB
            // i wysyłanie komunikatu do odpowiedniego węzła sieciowego

            MplsFIBRecord record = new MplsFIBRecord(command);
            bool added = false;

            foreach (MplsFIBRecord row in MplsFibTable)
            {
                if (row.Equals(record))
                {
                    added = true;
                    ReturnLog("Record already exists");
                    break;
                }
            }
            
            if (added == false)
            {
                try
                {
                    MplsFibTable.Add(record);
                    Send(ClientNamesKeyName[clientName], "ADDRECORD " + record.send());
                    ReturnLog("Record added to MPLS FIB Table");
                }
                catch(Exception e) { ReturnLog("Unable to send message to " + clientName); }
            }  
        }

        private void RemoveRecord(string command, string clientName)
        {
            // Usuwanie rekordu z tablicy MPLS FIB 
            // i wysyłanie komunikatu do odpowiedniego klienta komunikatu, aby usunąć rekord

            MplsFIBRecord record = new MplsFIBRecord(command);
            bool removed = false;
            
            foreach(MplsFIBRecord row in MplsFibTable)
            {
                if(row.Equals(record))
                {
                    MplsFibTable.Remove(row);

                    try
                    {
                        Send(ClientNamesKeyName[clientName], "REMOVERECORD " + record.send());
                        ReturnLog("Record removed from MPLS FIB Table");
                        removed = true;
                        break;
                    }
                    catch(Exception e) { ReturnLog(e + "\nUnable to send message to " + clientName); } 
                }
            }

            if(removed == false) { ReturnLog("Record doesn't exists"); }
        }

        private static void ReturnLog(string log)
        {
            Console.WriteLine($"[{DateTime.Now}]" + " " + log + "\n---------------------");
        }

        public void ReadCommandLines()
        {
            // Wczytywanie komendy z konsoli

            while(true)
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
            ManagementCenter managementCenter = new ManagementCenter();
            ReadConfig(arguments[1]);

            Thread serverThread = new Thread(managementCenter.StartServerSocket);
            Thread consoleCommandsThread = new Thread(managementCenter.ReadCommandLines);

            serverThread.Start();
            consoleCommandsThread.Start();
        }
    }
}
