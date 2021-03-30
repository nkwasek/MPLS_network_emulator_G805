using System;

namespace CableCloud
{
    class ConnectionsTableRow
    {
        private string senderName;
        private int senderPort;
        private string receiverName;
        private int receiverPort;
        private int isWorking; // czy łącze działa: 1 - działa, 0 - nie działa
        public ConnectionsTableRow(string senderName,string senderPort,string receiverName,string receiverPort,string isWorking )
        {
            this.senderName = senderName;
            this.senderPort = Convert.ToInt32(senderPort);
            this.receiverName = receiverName;
            this.receiverPort = Convert.ToInt32(receiverPort);
            this.isWorking = Convert.ToInt32(isWorking);

        }
        public bool SecondEnd(string firstEnd)
        {
            string[] msg = firstEnd.Split('.');

            return (msg[0].Equals(senderName) && Convert.ToInt32(msg[1]) == senderPort);
        }
        public string GetSecondEnd()
        {
            return ($"{receiverName}.{receiverPort}");
        }
        public bool IsWorking()
        {
            if(isWorking == 1) { return true; }
            else { return false; }
        }
        public void ChangeWorking()
        {
            if(IsWorking())
            {
                isWorking = 0;
            }
            else
            {
                isWorking = 1;
            }
        }
        public void ShowRecord()
        {
            Console.WriteLine($"{senderName} {senderPort} {receiverName} {receiverPort} {isWorking}");
        }

    }
}
