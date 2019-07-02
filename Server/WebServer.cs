using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebServer
{
    public class Server
    {
        private Socket Socket;
        private Thread AcceptingThread;
        public int MaxConnection = 5;
        private List<string> Cookies = new List<string>();

        public DateTime StartTime { get; private set; }
        public bool IsRunning { get; private set; }
        public string ResourcePath { get; set; }
        public int Port { get; set; }
        public string Address
        {
            get
            {
                return Socket.LocalEndPoint.ToString();
            }
        }
        public string ServerName { get; } = "WinstonServer";
        public string ServerVersion { get; } = "0.8";
        public string ServerComment { get; } = ".Net Framework";
        public Server(int port, string resourcePath)
        {
            IsRunning = false;
            Port = port;
            ResourcePath = resourcePath;
        }

        public void Start()
        {
            if (!IsRunning)
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);
                Socket.Bind(endPoint);
                Socket.Listen(MaxConnection);

                IsRunning = true;

                AcceptingThread = new Thread(Accepting);
                AcceptingThread.Name = "Accepting";
                AcceptingThread.Start();

                StartTime = DateTime.Now;

                Console.WriteLine("Сервер сконфигурирован. Порт: " + Port.ToString() + "\n");
            }
        }

        private void Accepting()
        {
            while (true)
            {
                Socket clientSocket = Socket.Accept();

                Thread clientHandlerThread = new Thread(new ParameterizedThreadStart(RequestHandler));
                clientHandlerThread.Name = "Handler_for_" + clientSocket.RemoteEndPoint.ToString();
                clientHandlerThread.Start(clientSocket);
            }
        }

        private void RequestHandler(object clientSocket)
        {
            Socket socket = clientSocket as Socket;

            byte[] receiveBuffer = new byte[30 * 1024]; // 30 kb
            int byteReceived = socket.Receive(receiveBuffer);

            if (byteReceived == 0)
            {
                socket.Close();
                return;
            }

            Request request = new Request(receiveBuffer, byteReceived);

            Log.Write(
                socket.RemoteEndPoint.ToString().PadRight(25) +
                request.Method.PadRight(8) +
                request.Uri.PadRight(30) +
                request.ProtocolName + "/" + request.ProtocolVersion + "  ");

            // ------------------------------------------

            Responce responce = new Responce(request);

            responce.Status = HttpStatusCode.NotFound;

            string fileToSend = string.Empty;
            byte[] content = null;

            if (request.Method == "GET")
            {
                string uri = (request.Uri == "/") ? "/index.html" : request.Uri;

                uri = uri.Substring(1);

                // Список поддерживаемых клиентом MIME-типов
                string[] clientSupportedMIMETypes = request.Headers["accept"].Split(',');

                // Узнаем расширение целевого файла и соответствующий ему MIME-тип
                string[] pathItems = uri.Split('/');
                string fileName = pathItems[pathItems.Length - 1];
                string extension = fileName.Split('.')[1];

                // Тип отправляемого ресурса
                MIMEType typeContent = new MIMEType(extension);
                // TODO проверить поддерживает ли клиент конечный тип (clientSupportedMIMETypes)

                if (typeContent.SubType == "html" || typeContent.SubType == "htm")
                {
                    string value;
                    if (request.Headers.TryGetValue("user-agent", out value))
                    {
                        if (value.Contains("Android"))
                            uri = "mobile/" + uri;
                        else
                            uri = "desktop/" + uri;
                    }
                }

                string[] files = Directory.GetFiles(ResourcePath, uri);

                if (files.Length != 0 && !string.IsNullOrEmpty(files[0]))
                {
                    responce.Status = HttpStatusCode.BadRequest;

                    long contentLength = 0;

                    switch (typeContent.Type)
                    {
                        case "text":
                            {
                                switch (typeContent.SubType)
                                {
                                    case "html":
                                    case "htm":
                                        {
                                            string file = File.ReadAllText(files[0], Encoding.UTF8);

                                            file = file.Replace("@datetime", DateTime.Now.ToString());

                                            content = Encoding.UTF8.GetBytes(file);
                                            contentLength = content.Length;
                                            responce.Status = HttpStatusCode.OK;
                                            break;
                                        }
                                    case "css":
                                    case "javascript":
                                    case "txt":
                                        {
                                            fileToSend = files[0];
                                            contentLength = new FileInfo(files[0]).Length;
                                            responce.Status = HttpStatusCode.OK;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "image":
                            {
                                fileToSend = files[0];
                                contentLength = new FileInfo(files[0]).Length;

                                switch (typeContent.SubType)
                                {
                                    case "x-icon":
                                        {
                                            responce.Status = HttpStatusCode.OK;
                                            break;
                                        }
                                }
                                break;
                            }
                    }

                    responce.Headers.Add("content-type", typeContent.ToString());
                    responce.Headers.Add("content-length", contentLength.ToString());
                }
                else // file not found
                    responce.Status = HttpStatusCode.NotFound;
            }
            else  // GET
            {
                responce.Headers.Add("allow", "GET");
                responce.Status = HttpStatusCode.NotImplemented;
            }

            // Общем заголовки для всех ответов
            responce.Headers.Add("server", $"{ServerName}/{ServerVersion} ({ServerComment})");
            responce.Headers.Add("date", DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");

            string startLine = responce.ToString() + "\r\n" + responce.GetHeadersString() + "\r\n";
            socket.Send(Encoding.ASCII.GetBytes(startLine));
            if (content != null && content.Length > 0)
                socket.Send(content);
            if (!string.IsNullOrEmpty(fileToSend))
                socket.SendFile(fileToSend);

            Log.WriteLineAdd((int)responce.Status + " " + responce.Status.ToString());

            socket.Close();
        }

        public void Stop()
        {
            if (IsRunning)
            {
                AcceptingThread.Abort();
                Socket.Close();
                IsRunning = false;
            }
        }
    }
}
