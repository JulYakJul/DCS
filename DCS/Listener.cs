using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS
{
    public interface Listener
    {
        public List<string>? AskForNewFile();

        public String DownloadNewFile(string localPath, string path);

        // public Task<bool> sendMessageToBrokerAsync(string address, List<string> message);
    }
}
