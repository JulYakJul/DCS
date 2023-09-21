using Microsoft.Extensions.Configuration;

namespace DCS
{
    static class Program
    {
        // Количество принимаемых подключений к серверу
        public static int MAXNUMCLIENTS;
        public static string? ipClient;
        public static string? SITAIPClient;
        public static int portClient;
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

            ipClient = config["ipAddressBSIS"];
            portClient = Convert.ToInt32(config["portBSIS"]);
            MAXNUMCLIENTS = Convert.ToInt32(config["maxCountClients"]);
            SITAIPClient = config["SITAAdress"];
            SITAportClient = Convert.ToInt32(config["SITAPort"]);

            DCSConnection dcsConnection = new();
            dcsConnection.StartServer();
            Console.WriteLine();
        }
    }
}