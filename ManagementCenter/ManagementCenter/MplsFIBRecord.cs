using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementCenter
{
    class MplsFIBRecord
    {
        public string RouterName { get; set; }
        public int InputPort { get; set; }
        public int InputLabel { get; set; }
        public int OutputPort { get; set; }
        public List<int> OutputLabel { get; set; } = new List<int>();
        public int Index { get; set; }

        public MplsFIBRecord(string line)
        {
            string[] splitedline = line.Split(' ');

            RouterName = splitedline[0];
            InputPort = Convert.ToInt32(splitedline[1]);
            InputLabel = Convert.ToInt32(splitedline[2]);
            OutputPort = Convert.ToInt32(splitedline[3]);
            //int[] OutputLabel = Convert.ToInt32(outLabelsTable);
            Index = Convert.ToInt32(splitedline[5]);
            //Console.WriteLine(splitedline[0]);
            string[] outLabelsTable = splitedline[4].Split(',');

            foreach(string label in outLabelsTable)
            {
                OutputLabel.Add(Convert.ToInt32(label));
            }
            
        }
        public string send()
        {
            string toSend = InputPort.ToString() + " " + InputLabel.ToString() + " " + OutputPort.ToString() + " ";

            foreach (int label in OutputLabel)
            {
                toSend +=label + ",";
            }
            toSend = toSend.Remove(toSend.Length-1,1);

            toSend += " " + Index;

            return toSend;
        }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                MplsFIBRecord record = (MplsFIBRecord)obj;
                return (RouterName == record.RouterName) && (InputPort == record.InputPort) 
                    && (InputLabel == record.InputLabel) && (OutputPort == record.OutputPort)
                    && (Enumerable.SequenceEqual(OutputLabel, record.OutputLabel)) && (Index == record.Index);
            }
        }
    }
}
