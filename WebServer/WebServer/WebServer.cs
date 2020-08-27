using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebServer
{
    class WebServer
    {
        public static Socket _serverSocket;
        public static int _port = 0;
        public static string _directory = null;

        static void Main(string[] args)
        {
            for(int i = 0; i<args.Length; i++)
            {
                if (i > 1) break;

                if(i == 0)
                    _port = int.Parse(args[i]);
                else
                    _directory = args[i];
            }

            CheckArguments();

            StartServer();

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                AcceptClient();
            }
        }

        public static void CheckArguments()
        {
            if (_port == 0 || _port < 0)
                _port = Defaults.PORT;

            if (string.IsNullOrEmpty(_directory))
                _directory = Defaults.DIRECTORY;
        }

        public static void StartServer()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Stream,
                                      ProtocolType.Tcp);

            IPAddress ip = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ip, _port);
            _serverSocket.Bind(ipEndPoint);
            _serverSocket.Listen(10);

            Console.WriteLine("Running: " + ip + ":" + _port);
        }

        public static void AcceptClient()
        {
            Socket client = _serverSocket.Accept();
            Thread thread = new Thread(() => ProcessRequest(client));
            thread.Start();
        }
        
        public static void ProcessRequest(Socket client)
        {
            NetworkStream ns = new NetworkStream(client);
            StreamReader reader = new StreamReader(ns);
            StreamWriter writter = new StreamWriter(ns);

            try
            {
                string request = reader.ReadLine();
                Console.WriteLine(request);

                string[] tokens = request.Split(' ');
                string page = tokens[1];

                if (page == "/" && _directory == Defaults.DIRECTORY)
                {
                    page = "/test.htm";
                }

                StreamReader file = new StreamReader(_directory + page);
                writter.WriteLine("HTTP/1.0 200 OK\n");

                string data = file.ReadLine();

                while (data != null)
                {
                    writter.WriteLine(data);
                    writter.Flush();
                    data = file.ReadLine();
                }
            }
            catch (FileNotFoundException)
            {
                writter.WriteLine("HTTP/1.0 404 ERROR\n");
                writter.WriteLine("<H1>Could not find the file.</H1>");
                writter.Flush();
            }
            catch (Exception)
            {
                writter.WriteLine("HTTP/1.0 400 ERROR\n");
                writter.WriteLine("<H1>Bad request.</H1>");
                writter.Flush();
            }

            client.Close();
        }
    }
}
