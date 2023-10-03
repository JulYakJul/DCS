using Microsoft.Extensions.Configuration;

namespace DCS
{
    static class Program
    {
        // Количество принимаемых подключений к серверу
        public static int MAXNUMCLIENTS;
        public static string? ServeripClient;
        public static int ServerportClient;

        public static string? SITAIPClient;
        public static int SITAportClient;

        public static string? TCPLIPClient;
        public static int TCPLportClient;
        public const int tcpPort = 8080;

        static void Main(string[] args)
        {
            var directory = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile($"appsettings.json", true, true).Build();

            ServeripClient = "192.168.1.129";
            ServerportClient = 8080;
            MAXNUMCLIENTS = 200000000;
            SITAIPClient = "192.168.1.95";
            SITAportClient = 8080;

            DCSConnection dcsConnection = new();
            dcsConnection.StartServer();
            Console.WriteLine();
        }
    }
}