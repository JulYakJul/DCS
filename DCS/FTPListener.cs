using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace DCS
{
    public class FTPListener : Listener
    {
        public List<FtpListItem>? oldList;
        private FtpClient? ftpClient;

        private TcpClient tcpClient;

        private readonly FTPConfiguration _ftpConfiguration;

        public string _localPath;

        public FTPListener(FTPConfiguration ftpConfiguration)
        {
            _ftpConfiguration = ftpConfiguration;
            _localPath = ftpConfiguration.LocalPath;
        }

        ~FTPListener()
        {
            ConnctionClose();
        }

        public void Dispose()
        {
            // Пустая реализация, так как нет ресурсов для освобождения
        }

        public void Connect()
        {
            ftpClient = new FtpClient(_ftpConfiguration.Address, _ftpConfiguration.FTPEmail, _ftpConfiguration.FTPPassword);
        }
        public List<String>? AskForNewFile()
        {
            if (ftpClient == null)
            {
                return null;
            }

            var readList = ftpClient.GetListing().Where(l => l.Type == FtpObjectType.File).ToList();

            if (oldList == null)
            {
                oldList = readList.ToList();
                return readList.Select(r => r.FullName).ToList();
            }

            var newFileList = new List<FtpListItem>();

            readList.ToList().ForEach(l =>
            {
                if (!oldList.Any(o => o.FullName == l.FullName))
                {
                    newFileList.Add(l);
                }
            });

            oldList.AddRange(newFileList);

            return newFileList.Select(r => r.FullName).ToList();
        }

        public string DownloadNewFile(string localPath, string path)
        {
            if (ftpClient == null)
            {
                return String.Empty;
            }

            ftpClient.DownloadFile(localPath, path);

            return File.ReadAllText(localPath);
        }

        public async Task<bool> SendMessageToBrokerAsync(string address, List<string> message, TcpClient tcpClient)
        {
            NetworkStream stream = tcpClient.GetStream();

            try
            {
                foreach (var m in message)
                {
                    var requestData = Encoding.UTF8.GetBytes(m);

                    await stream.WriteAsync(requestData, 0, requestData.Length);

                    byte[] newline = Encoding.UTF8.GetBytes(Environment.NewLine);
                    await stream.WriteAsync(newline, 0, newline.Length);
                }
            }
            catch (SocketException ex)
            {
                tcpClient.Close();
                Console.WriteLine(ex.ToString());
                stream.Close();
            }
            return true;
        }

        public void ConnctionClose()
        {
            ftpClient?.Disconnect();
        }
    }
}
