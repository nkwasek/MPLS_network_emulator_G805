using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    class MPLSFibTable
    {
        public static List<MPLSFibRecord> MplsFibTable = new List<MPLSFibRecord>();

        public static void AddElement(MPLSFibRecord record)
        {
            MplsFibTable.Add(record);
        }
        public static void RemoveElement(MPLSFibRecord record)
        {
            MplsFibTable.Remove(record);
        }
        public static int ReturnIndex(int portIn,int label,int index)
        {
            int i = 0;
            foreach(MPLSFibRecord mplsr in MplsFibTable)
            {
                if(mplsr.InputPort == portIn && mplsr.InputLabel == label && mplsr.Index ==index)
                {
                    return i;
                }

                i++;
            }
            return (-1);
        }
        public static void showTable()
        {
            foreach(MPLSFibRecord mplsr in MplsFibTable)
            {
                mplsr.ShowRecord();
            }
        }
    }
}
