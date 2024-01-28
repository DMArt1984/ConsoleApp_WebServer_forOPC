using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApp_WebServerDOC
{
    class Server
    {
        TcpListener myListener; // Объект, принимающий TCP-клиентов

        // Запуск сервера
        public Server(int Port)
        {
            // Слушатель указанного порта
            myListener = new TcpListener(IPAddress.Any, Port);
            myListener.Start();

            // В бесконечном цикле
            while (true)
            {
                // 1)
                // Принимаем новых клиентов и передаем их на обработку новому экземпляру класса Client
                // new Client(Listener.AcceptTcpClient());

                // 2)
                // Принимаем нового клиента
                // TcpClient Client = Listener.AcceptTcpClient();
                // Создаем поток
                // Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));
                // И запускаем этот поток, передавая ему принятого клиента
                // Thread.Start(Client);

                // 3)
                // Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                // с использованием пула потоков.
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), myListener.AcceptTcpClient());
            }
        }

        // Остановка сервера
        ~Server()
        {
            // Есть кто?
            if (myListener != null)
            {
                // Остановим его!!!
                myListener.Stop();
            }
        }

        // Ура! Новый клиент
        static void ClientThread(Object StateInfo)
        {
            Console.WriteLine("{new client}");
            new Client((TcpClient)StateInfo);
        }
    }

}

