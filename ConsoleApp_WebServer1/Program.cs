using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace ConsoleApp_WebServer1
{
    class Program
    {

        static void Main(string[] args)
        {

            // Какое максимальное количество потоков?
            // Ну, пусть будет по 4 на каждый процессор
            int MaxThreadsCount = Environment.ProcessorCount * 4;

            // Установим максимальное количество рабочих потоков
            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);

            // Установим минимальное количество рабочих потоков
            ThreadPool.SetMinThreads(2, 2);

            // Вывод
            Console.WriteLine(" ===== Server =====");
            Console.Write(" port (90) = ");

            String myStringPort = Console.ReadLine();
            int nPort = 90;

            if (myStringPort.Length > 0)
            {
                nPort = Convert.ToInt16(myStringPort);
                if (nPort <= 0)
                {
                    nPort = 90;
                }
            } 

            Console.WriteLine("<start>");

            // Создадим новый сервер на порту 90 или nPort
            new Server(nPort);

            Console.WriteLine("<end>");
        }

    }
}
