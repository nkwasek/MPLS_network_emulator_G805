using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwitchingRouter
{
    class SwitchingFabric
    {
        public static void SwapLabels ( int portNumber, MPLSPacket mplsp)
        {
            if (mplsp.TTL > 0)
            {
                mplsp.TTL -= 1;
                int LabelIndex = 1;
                while (true)
                {
                    byte LastLabel = mplsp.Labels[mplsp.Labels.Count() - 1];
                    int mplsListIndex = MPLSFibTable.ReturnIndex(portNumber, LastLabel, LabelIndex);
                    if (mplsListIndex == -1)
                    {
                        ManagementAgent.ReturnLog("Unable to forward packet.");
                        break;
                    }
                    else
                    {
                        List<int> outputLabels = MPLSFibTable.MplsFibTable[mplsListIndex].OutputLabel;
                        if (outputLabels[0] == 0)
                        {
                            LabelIndex++;
                            mplsp.Labels.RemoveAt(mplsp.Labels.Count() - 1);
                            continue;
                        }
                        else
                        {
                            mplsp.Labels.RemoveAt(mplsp.Labels.Count() - 1);
                            foreach (int labels in outputLabels)
                            {
                                mplsp.Labels.Add(Convert.ToByte(labels));
                            }
                            IOPort exitPort = LabelSwitchingRouter.ReturnPort(MPLSFibTable.MplsFibTable[mplsListIndex].OutputPort);
                            if (exitPort != null)
                            {
                                exitPort.Send(mplsp);
                            }
                            else { ManagementAgent.ReturnLog("Unable to forward packet."); }
                            break;

                        }
                    }
                }
            }
            else
            {
                ManagementAgent.ReturnLog("TTL value equals 0. Packet disappeared.");
            }
        }
    }
}
