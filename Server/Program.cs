
namespace WebServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServer.Server server = new WebServer.Server(63000, "../../../pages");

            server.Start();
        }
    }
}
