using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace csbattleship
{
    public class NetworkClass
    {
        static TcpListener listener;
        public TcpClient client;
        NetworkStream stream = null;

        CancellationTokenSource cancelTokenSource;
        CancellationToken token;

        public NetworkClass(TcpClient tcpClient)
        {
            client = tcpClient;

            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
        }

        void Worker(string socketType)
        {
            try
            {
                ConnectionLoop(socketType);
            }
            catch (Exception ex)
            {
                Program.f.SetGameStatus(0);
                Program.f.SendMessage(ex.Message);

                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();

                cancelTokenSource.Cancel();
            }
        }

        void ConnectionLoop(string socketType)
        {
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64];

                if (socketType == "client")
                    data = Encoding.UTF8.GetBytes("client connect");
                else if (socketType == "server")
                    data = Encoding.UTF8.GetBytes("server connect");
                stream.Write(data, 0, data.Length);

                int bytes;
                string response = "";
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    response += Encoding.UTF8.GetString(data, 0, bytes);
                }
                while (stream.DataAvailable);

                if (socketType == "client" && response != "server connect" ||
                    socketType == "server" && response != "client connect")
                    throw new Exception("");

                Task.Run(ActionLoop);

                Program.f.SetGameStatus(1);
            }
            catch (Exception)
            {
                cancelTokenSource.Cancel();
                throw new Exception("Соединение не установлено");
            }
        }
        
        void ActionLoop()
        {
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        throw new Exception("");

                    byte[] data;
                    string message = Program.f.GetNetDataToSend();
                    bool flag = false;
                    if (message != "[\"\",\"\",\"\"]")
                    {
                        data = Encoding.UTF8.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        flag = true;
                    }

                    int bytes;
                    data = new byte[64];
                    message = "";
                    if (stream.DataAvailable)
                    {
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            message += Encoding.UTF8.GetString(data, 0, bytes);
                        }
                        while (stream.DataAvailable);

                        Program.f.ViewReceivedNetData(message);
                    }

                    if (flag && Program.f.messageToSend != "")
                    {
                        Program.f.SendMessage(Program.f.messageToSend, "me");
                        Program.f.messageToSend = "";
                    }
                }
            }
            catch (Exception)
            {
                Program.f.SendMessage("Соединение разорвано");
                Program.f.SetGameStatus(0);
                cancelTokenSource.Cancel();
                if (listener != null)
                    listener.Stop();

                throw new Exception("Соединение разорвано");
            }
        }

        public static void Server()
        {
            new Task(() =>
            {
                try
                {
                    listener = new TcpListener(IPAddress.Parse(Program.f.host), Program.f.port);
                    listener.Start();
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkClass clientObject = new(client);
                    clientObject.Worker("server");
                }
                catch (Exception ex)
                {
                    Program.f.SendMessage(ex.Message);
                    Program.f.SetGameStatus(0);

                    if (listener != null)
                        listener.Stop();
                }
            }).Start();
        }

        public static void Client()
        {
            new Task(() =>
            {
                try
                {
                    TcpClient client = new(Program.f.host, Program.f.port);
                    NetworkClass clientObject = new(client);
                    clientObject.Worker("client");
                }
                catch (Exception ex)
                {
                    Program.f.SendMessage(ex.Message);
                    Program.f.SetGameStatus(0);
                }
            }).Start();
        }

    }
}
