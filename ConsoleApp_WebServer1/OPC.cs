using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TitaniumAS.Opc.Client.Da;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da.Browsing;

using System.Xml;
using System.Xml.Linq;

namespace ConsoleApp_WebServer1
{
    // OPC – это набор повсеместно принятых спецификаций, 
    // предоставляющих универсальный механизм обмена данными в системах контроля и управления.
    // Аббревиатура OPC традиционно расшифровывается как OLE for Process Control.
    // OLE – Object Linking and Embedding (связывание и встраивание объектов).  
    class OPC
    {
        public bool IsConnected = false; // подключен
        public String Out = "";

        // Запрос списка тегов
        public OPC(String OPCName, Boolean Info = true, String Group="")
        {
            Uri url = UrlBuilder.Build(OPCName); // например: "Kepware.KEPServerEX.V6"
            using (var server = new OpcDaServer(url))
            {
                // Подключение к серверу
                server.Connect();
                Console.WriteLine("Server connected = " + server.IsConnected);
                IsConnected = server.IsConnected;

                var browser = new OpcDaBrowserAuto(server); // Браузер для просмотра всех тегов
                Out = SimpleBrowseChildren(browser, Group); // только выбранной группы
            }
        }

        // Запрос значений тегов
        public OPC(String OPCName, String VarAddress = "")
        {

            Uri url = UrlBuilder.Build(OPCName); // например: "Kepware.KEPServerEX.V6"
            using (var server = new OpcDaServer(url))
            {
                // Подключение к серверу
                server.Connect();
                Console.WriteLine("Server connected = " + server.IsConnected);
                IsConnected = server.IsConnected;

                // Создать группу
                OpcDaGroup group = server.AddGroup("MG");
                group.IsActive = true;

                var definition1 = new OpcDaItemDefinition // переменная 1
                {
                    ItemId = "Simulation Examples.Functions.Ramp1",
                    IsActive = true
                };

                // заменяем адрес тега для переменной 1
                if (!String.IsNullOrEmpty(VarAddress))
                {
                    definition1.ItemId = VarAddress;
                }

                var definition2 = new OpcDaItemDefinition // переменная 2
                {
                    ItemId = "Simulation Examples.Functions.Sine1",
                    IsActive = true
                };
                var definition3 = new OpcDaItemDefinition // переменная 3
                {
                    ItemId = "Simulation Examples.Functions.Random1",
                    IsActive = true
                };
                
                OpcDaItemDefinition[] definitions = { definition1, definition2, definition3 };
                OpcDaItemResult[] results = group.AddItems(definitions);

                Console.WriteLine();

                // Результат
                foreach (OpcDaItemResult result in results)
                {
                    if (result.Error.Failed)
                    {
                        Console.WriteLine("Error adding items: {0}", result.Error);
                    }
                    else
                    {
                        // не выводим, а могли бы:
                        //Console.WriteLine("> " + result.Item.ItemId.ToString());
                    }
                }

                // А данные нам нужны в формате
                // XML
                XDocument xdoc = new XDocument();

                // создаем корневой элемент
                XElement Tags = new XElement("Tags");

                Console.WriteLine();

                // Чтение всех элементов группы
                OpcDaItemValue[] values = group.Read(group.Items, OpcDaDataSource.Device);

                foreach (OpcDaItemValue element in values)
                {
                    Console.WriteLine(element.Item.ItemId.ToString() + " = " + element.Value.ToString());
                    Out += element.Item.ItemId.ToString() + " = " + element.Value.ToString() + "; ";
                    // xml
                    // создаем элемент
                    XElement Tag = new XElement("Tag");

                    // создаем атрибут
                    XAttribute TagName = new XAttribute("name", element.Item.ItemId.ToString());
                    XElement TagValue = new XElement("value", element.Value.ToString());

                    // добавляем атрибут и значение в элемент
                    Tag.Add(TagName);
                    Tag.Add(TagValue);

                    // добавляем в корневой элемент
                    Tags.Add(Tag);
                }

                Console.WriteLine(Tags.ToString());
            }
        }


        // Список элементов 
        public void BrowseChildren(IOpcDaBrowser browser, string itemId = null, int indent = 0)
        {
            // Когда itemId имеет значение null, будут просматриваться корневые элементы.
            OpcDaBrowseElement[] elements = browser.GetElements(itemId);

            // Выходные элементы
            foreach (OpcDaBrowseElement element in elements)
            {
                Console.Write(new String('-', indent));
                Console.WriteLine(element.ItemId);
                Out += "<p>"+new String('-', indent) + element.ItemId+"</p>";

                // Игнорируем дочерние элементы
                if (!element.HasChildren)
                    continue;

                // Выводим элементы
                BrowseChildren(browser, element.ItemId, indent + 2);
            }
        }

        // Одноуровневый Список элементов 
        public String SimpleBrowseChildren(IOpcDaBrowser browser, string itemId = null, int indent = 0)
        {
            String Out = "";
            // Когда itemId имеет значение null, будут просматриваться корневые элементы.
            OpcDaBrowseElement[] elements = browser.GetElements(itemId);

            // Выходные элементы
            foreach (OpcDaBrowseElement element in elements)
            {
                Console.WriteLine(element.ItemId);
                Out += "<p>" +  element.ItemId + "</p>";
            }
            return Out;
        }
    }
}
