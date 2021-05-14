using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace csbattleship
{
    public class NetworkClass
    {
        const int port = 8888;
        static TcpListener listener;
        public TcpClient client;
        NetworkStream stream = null;


        public NetworkClass(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public void Worker(string mode)
        {
            try
            {
                if (ConnectionSuccessful(mode))
                {
                    Program.f.SetGameStatus(1);
                }
                else
                {
                    throw new Exception("Соединение не установлено");
                }

                byte[] data = new byte[64];

                while (true)
                {
                    string message = Program.f.transportData;
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        message += Encoding.UTF8.GetString(data, 0, bytes);
                    }
                    while (stream.DataAvailable);

                    if (message.Length > 0)
                    {
                        data = Encoding.UTF8.GetBytes("[получено] " + message);
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.f.SetGameStatus(0);
                Program.f.SendMessage(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }

        bool ConnectionSuccessful(string mode)
        {
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64];

                if (mode == "client")
                    data = Encoding.UTF8.GetBytes("client connect");
                else if (mode == "server")
                    data = Encoding.UTF8.GetBytes("server connect");
                stream.Write(data, 0, data.Length);

                int bytes = 0;
                string message = "";
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    message += Encoding.UTF8.GetString(data, 0, bytes);
                }
                while (stream.DataAvailable);

                if (mode == "client" && message == "server connect" || 
                    mode == "server" && message == "client connect")
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Server()
        {
            try
            {
                new Thread(() =>
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
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Program.f.SendMessage(ex.Message);
                Program.f.SetGameStatus(0);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }

        public static void Client()
        {
            try
            {
                new Thread(() =>
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
            catch (Exception ex)
            {
                Program.f.SendMessage(ex.Message);
                Program.f.SetGameStatus(0);
            }
        }

    }
}
