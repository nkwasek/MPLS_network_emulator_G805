using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LabelSwitchingRouter
{
    class MPLSPacket
    {
        public string Message { get; } 
        public byte TTL { get; set; } //1byte
        public List<byte> Labels { get; set; } = new List<byte>(); //3bytes
        public string Source { get; } //3bytes

        public byte[] GetBytes()
        {
            byte[] msg = Encoding.Default.GetBytes(Message);
            byte[] labels = new byte[3];
            int i = 0;
            foreach(byte b in Labels)
            {
                labels[i] = b;
                i++;
            }
            i = 0;
            byte[] src = Encoding.Default.GetBytes(Source);
            int msgleng = msg.Length;
            byte[] all = new byte[msgleng + 7];
            foreach(byte b in msg)
            {
                all[i] = b;
                i++;
            }
            all[i] = TTL;
            i++;
            foreach (byte b in src)
            {
                all[i] = b;
                i++;
            }
            foreach (byte b in labels)
            {
                all[i] = b;
                i++;
            }
            return all;
        }
        public MPLSPacket(byte[] b)
        {
            int len = b.Length;
            for(int j=0;j<3;j++)
            {
                if(b[len - (3 - j)]!=0)
                {
                    Labels.Add(b[len - (3 - j)]);
                }
            }
            len-=3;
            byte[] src = new byte[3];
            for(int j=0; j<3; j++)
            {
                src[j] = b[len - (3 - j)];
            }
            len -= 3;
            Source = Encoding.Default.GetString(src);

            TTL = b[len-1];
            len -= 1;
            byte[] msg = new byte[len];
            for(int j=0;j<len;j++)
            {
                msg[j] = b[j];
            }
            Message = Encoding.Default.GetString(msg);

        }
        public MPLSPacket(string msg,byte ttl,List<byte>labels,string src)
        {
            Message = msg;
            TTL = ttl;
            Labels = labels;
            Source = src;
        }
        public string MPLSInfo()
        {
            string labels = string.Empty;
            for(int i =0;i<Labels.Count;i++)
            {
                if (i == 0) { labels = Labels[i].ToString(); }
                else { labels = labels + ", " + Labels[i]; }

            }

            return ($"Message: {Message}; TTL: {TTL}; Source: {Source}; Labels: [{labels}]");
        }
    }

}
