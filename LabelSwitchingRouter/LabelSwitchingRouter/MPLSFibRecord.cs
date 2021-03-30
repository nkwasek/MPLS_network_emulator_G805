using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    class MPLSFibRecord
    {
        public int InputPort { get; set; }
        public int InputLabel { get; set; }
        public int OutputPort { get; set; }
        public List<int> OutputLabel { get; set; } = new List<int>();
        public int Index { get; set; }

        public MPLSFibRecord(string record)
        {
            string[] splitedRecord = record.Split(' ');
            InputPort = Convert.ToInt32(splitedRecord[0]);
            InputLabel = Convert.ToInt32(splitedRecord[1]);
            OutputPort = Convert.ToInt32(splitedRecord[2]);
            string[] splitedLabel = splitedRecord[3].Split(',');
            foreach(string label in splitedLabel)
            {
                OutputLabel.Add(Convert.ToInt32(label));
            }
            Index = Convert.ToInt32(splitedRecord[4]);
        }
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                MPLSFibRecord p = (MPLSFibRecord)obj;
                return (InputPort == p.InputPort) && (InputLabel == p.InputLabel) && (OutputPort == p.OutputPort) && (Enumerable.SequenceEqual(OutputLabel, p.OutputLabel)) && (Index == p.Index);
            }
        }
        public void ShowRecord()
        {
            int i = 0;
            string ol = string.Empty;
            foreach(int label in OutputLabel)
            {
                if (i == 0) { ol = $"{label}"; i++; }
                else { ol = ol + "," + label.ToString(); }
            }
            Console.WriteLine($"{InputPort} {InputLabel} {OutputPort} {ol} {Index}");
        }
    }
}
