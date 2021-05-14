using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace csbattleship
{
    public class NetworkClass
    {
        static TcpListener listener;
        public TcpClient client;
        NetworkStream stream = null;


        public NetworkClass(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public void Worker(string socketType)
        {
            try
            {
                ConnectionLoop(socketType);

                Program.f.SetGameStatus(1);

                StartBattleLoop(socketType);

                Program.f.SetGameStatus(2);

                ActionLoop(socketType);
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

                int bytes = 0;
                string response = "";
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    response += Encoding.UTF8.GetString(data, 0, bytes);
                }
                while (stream.DataAvailable);

                if (socketType == "client" && response != "server connect" ||
                    socketType == "server" && response != "client connect")
                    throw new Exception("Соединение не установлено");
            }
            catch (Exception)
            {
                throw new Exception("Соединение не установлено");
            }
        }
        // TODO: корректно завершать сокеты и потоки после закрытия программы
        void StartBattleLoop(string socketType)
        {
            try
            {
                while (true)
                {
                    if (Program.f.readyForBattle)   // TODO: нет времени на расстановку кораблей
                    {
                        break;
                    }
                }

                byte[] data = new byte[64];

                if (socketType == "client")
                    data = Encoding.UTF8.GetBytes("client ready");
                else if (socketType == "server")
                    data = Encoding.UTF8.GetBytes("server ready");
                stream.Write(data, 0, data.Length); // TODO: слишком рано

                int bytes = 0;
                string response = "";
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    response += Encoding.UTF8.GetString(data, 0, bytes);
                }
                while (stream.DataAvailable);

                if (socketType == "client" && response != "server ready")
                    throw new Exception("Что-то не так");
                if (socketType == "server" && response != "client ready")
                    throw new Exception("Что-то не так");
            }
            catch (Exception)
            {
                throw new Exception("Что-то не так");
            }
        }

        void ActionLoop(string socketType)
        {
            while (true)
            {
                byte[] data = new byte[64];
                string message = Program.f.SendNetData();
                if (message != "[\"\",\"\"]")
                {
                    data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }

                int bytes = 0;
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
                        
                    Program.f.RecieveNetData(message);
                }

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
