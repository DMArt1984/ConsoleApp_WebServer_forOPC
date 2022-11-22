using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ConsoleApp_WebServer1
{
    class Client
    {
        // Класс для принятого клиента от TcpListener
        public Client(TcpClient Client)
        {

            string clientIPAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
            Console.WriteLine("> " + Client.ToString()+" "+ clientIPAddress + " data=" + Client.Available);

            // запрос клиента
            string Request = "";

            // Буфер хранения принятых от клиента данных
            byte[] clientBuffer = new byte[1024];

            // Количество байт, принятых от клиента
            int bytesCount;

            // Читаем из потока клиента все данные
            while ((bytesCount = Client.GetStream().Read(clientBuffer, 0, clientBuffer.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                Request += Encoding.ASCII.GetString(clientBuffer, 0, bytesCount);

                // Кто-то заметил, что:
                // Запрос должен обрываться последовательностью \r\n\r\n
                // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                // Нам не нужно получать данные из POST-запроса (и т. п.), а обычный запрос
                // по идее не должен быть больше 4 килобайт
                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                {
                    break;
                }
            }

            // Анализ запроса с использованием регулярных выражений
            // также отсекаем все переменные GET-запроса
            Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            // Если запрос не удался
            if (ReqMatch == Match.Empty)
            {
                // Передаем клиенту ошибку 400 - неверный запрос
                SendError(Client, 400);
                return;
            }
            // Получаем строку запроса
            string RequestUri = ReqMatch.Groups[1].Value;

            // Приводим ее к изначальному виду, преобразуя экранированные символы
            // Например, "%20" -> " "
            RequestUri = Uri.UnescapeDataString(RequestUri);

            // Если в строке содержится двоеточие, передадим ошибку 400
            // Это нужно для защиты от URL типа http://example.com/../../file.txt
            if (RequestUri.IndexOf("..") >= 0)
            {
                SendError(Client, 400);
                return;
            }
            if (RequestUri.Length > 1)
            {
                RequestUri = RequestUri.Remove(0, 1);
            }


            // Переменные для заголовка и другие
            string Document = "";
            String Param = "";

            // Если задан тип var
            if (RequestUri.Length > 4 && RequestUri.Substring(0, 4) == "var=")
            {
                Param = RequestUri.Substring(4); // Получаем значение параметра var
            }

            // Если задан тип info
            if (RequestUri.Length > 5 && RequestUri.Substring(0, 5) == "info=")
            {
                Param = RequestUri.Substring(5); // Получаем значение параметра info

                // Получаем OPC Server
                OPC info = new OPC("Kepware.KEPServerEX.V6", true, Param);

                // Код одной HTML-странички
                string Html = "<html><body><h1>I'm C#. It works!</h1><h2> ip " + clientIPAddress + "</h2><p>" + info.Out + "</p>Req: " + Request + "<h4>" + RequestUri + "</h4></body></html>";
                
                // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                Document = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;

            }
            else
            {
                // Получаем OPC Server
                OPC info = new OPC("Kepware.KEPServerEX.V6", Param);

                // Код другой HTML-странички
                string Html = "<html><body><h1>I'm C#. It works!</h1><h2> ip " + clientIPAddress + "</h2><h3>" + info.Out + "</h3>Req: " + Request + "<h4>" + RequestUri + "</h4></body></html>";
                
                // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                Document = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;

            }

            // Приведем строку к виду массива байт
            byte[] outBuffer = Encoding.ASCII.GetBytes(Document);
            
            // Отправим его клиенту
            Client.GetStream().Write(outBuffer, 0, outBuffer.Length);
            
            // Закроем соединение
            Client.Close();
        }

        // Отправка страницы с ошибкой
        private void SendError(TcpClient Client, int Code)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            
            // Закроем соединение
            Client.Close();
        }
    }
}
