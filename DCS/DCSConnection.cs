using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FluentFTP;
using FluentFTP.Helpers;

//using System.Net.WebClient;

namespace DCS
{
    internal class DCSConnection
    {
        // Высокоуровневая надстройка для прослушивающего сокета
        TcpListener server;

        TcpClient[] clients = new TcpClient[Program.MAXNUMCLIENTS];
        TcpClient ServerClient = new();
        TcpClient SITAClient = new();

        bool stopNetwork;

        // Счетчик подключенных клиентов
        int countClient = 0;

        public void StartServer()
        {
            // Предотвратим повторный запуск сервера
            if (server == null)
            {
                // Блок перехвата исключений на случай запуска одновременно
                // двух серверных приложений с одинаковым портом.
                try
                {
                    stopNetwork = false;
                    countClient = 0;

                    server = new TcpListener(IPAddress.Any, Program.tcpPort);
                    server.Start();

                    Thread acceptThread = new(AcceptClients);
                    acceptThread.Start();

                    Console.WriteLine("Сервер запущен");

                    string filePath = "C:\\Users\\DemidOffice\\Desktop\\Test\\TestDCS.txt";
                    string fileName = "TestDCS.txt";

                    FtpClient ftpClient = new FtpClient("i9.files.com")
                    {
                        Credentials = new NetworkCredential("alenforgot@gmail.com", "gavri1lA123)"),
                    };

                    // Отправка файла на FTP-сервер в папку /files
                    SendFileToFTP(ftpClient, filePath, fileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось запустить сервер");
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        // Принимаем запросы клиентов на подключение и
        // привязываем к каждому подключившемуся клиенту 
        // сокет (в данном случае объект класса TcpClient)
        // для обменом сообщений.
        public void AcceptClients()
        {
            while (true)
            {
                try
                {
                    this.clients[countClient] = server.AcceptTcpClient();
                    Thread readThread = new(ReceiveRun);
                    readThread.Start(countClient);
                    Console.WriteLine("Подключился клиент");
                    countClient++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось подключить клиента");
                    Console.WriteLine(ex.ToString());
                }

                if (countClient == Program.MAXNUMCLIENTS || stopNetwork == true)
                {
                    break;
                }
            }
        }

        public void ReceiveRun(object num)
        {
            while (true)
            {
                try
                {
                    string stream = null;
                    NetworkStream networkStream = clients[(int)num].GetStream();

                    while (networkStream.DataAvailable == true)
                    {
                        byte[] buffer = new byte[clients[(int)num].Available];
                        networkStream.Read(buffer, 0, buffer.Length);
                        stream = Encoding.Default.GetString(buffer);
                        string[] messages = stream.Split(new[] { "\r\n\r\n", "\r\n\n" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in messages)
                        {
                            SITAClient.Connect(IPAddress.Parse(Program.SITAIPClient), Program.SITAportClient);
                            ServerClient.Connect(IPAddress.Parse(Program.SITAIPClient), Program.SITAportClient);
                            SendToClients(line, (int)num, ServerClient);
                            SendToClients(line, (int)num, SITAClient);

                            // Отправьте файл на FTP-сервер
                            //string filePath = "C:\\Users\\DemidOffice\\Desktop\\Test";
                            //string fileName = "TestDCS.txt";
                            //SendFileToFTP(ftpClient, filePath, fileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                if (stopNetwork == true)
                {
                    break;
                }
            }
        }


        private bool SendFileToFTP(FtpClient ftpClient, string localFilePath, string remoteFileName)
        {
            try
            {
                ftpClient.Connect();

                string remoteDirectory = "test"; //путь к целевой папке
                string remotePath = remoteDirectory + "/" + remoteFileName;

                if (!File.Exists(localFilePath))
                {
                    Console.WriteLine($"Файл {localFilePath} не существует на локальной машине.");
                    return false; 
                }

                // существует ли целевая папка на FTP-сервере
                if (!ftpClient.DirectoryExists(remoteDirectory))
                {
                    Console.WriteLine($"Папка {remoteDirectory} не существует на FTP-сервере.");
                    return false; 
                }

                ftpClient.UploadFile(localFilePath, remotePath);

                Console.WriteLine($"Файл {remoteFileName} успешно отправлен на FTP-сервер");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке файла на FTP-сервер: " + ex.ToString());
                return false;
            }
            finally
            {
                ftpClient.Disconnect();
            }
        }

        public void SendToClients(string text, int skipIndex, TcpClient tcpClient)
        {
            // Подготовка и запуск асинхронной отправки сообщения.
            NetworkStream ns = tcpClient.GetStream();
            byte[] myReadBuffer = Encoding.Default.GetBytes(text);
            ns.BeginWrite(myReadBuffer, 0, myReadBuffer.Length, new AsyncCallback(AsyncSendCompleted), ns);
        }

        // Асинхронная отправка сообщения клиенту.
        public void AsyncSendCompleted(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            ns.EndWrite(ar);
        }
    }
}
