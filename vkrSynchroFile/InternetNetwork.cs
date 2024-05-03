using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace vkrSynchroFile
{
    internal class InternetNetwork
    {

        public bool PingDevice(string ipAddress)
        {
            int port = 12345;
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(ipAddress, port);
                    return true;
                }
            }
            catch (SocketException)
            {
                return false;
            }
            /*try
            {
                // Создание объекта для отправки ICMP-запроса (ping)
                Ping pingSender = new Ping();

                // Отправка ICMP-запроса на указанный IP-адрес
                PingReply reply = pingSender.Send(ipAddress);

                // Проверка статуса ответа
                // Если статус равен IPStatus.Success, то удаленное устройство доступно
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                // В случае возникновения ошибки выводим сообщение об ошибке
                Console.WriteLine("Ошибка при выполнении ping: " + ex.Message);
                return false; // Возвращаем false, так как не удалось выполнить ping
            }*/
        }

        public void SendProfile(string ip, string uid)
        {
            try
            {
                // IP-адрес и порт сервера, к которому мы хотим подключиться
                string serverIP = ip; // Замените на IP-адрес вашего сервера
                int serverPort = 12345; // Замените на порт вашего сервера

                // Создание экземпляра TcpClient для подключения к серверу
                TcpClient client = new TcpClient(serverIP, serverPort);

                // Получаем поток для передачи данных
                NetworkStream stream = client.GetStream();

                // Создание объекта запроса для отправки сообщения
                Request request = new Request
                {
                    Type = "ProfileRequest",
                    uid = uid,
                    Message = "Привет, сервер!"
                };

                // Преобразование объекта запроса в JSON
                string requestData = JsonSerializer.Serialize(request);

                // Получение длины сообщения в байтах
                byte[] messageLengthBytes = BitConverter.GetBytes(requestData.Length);
                stream.Write(messageLengthBytes, 0, messageLengthBytes.Length);

                // Отправка JSON на сервер
                byte[] requestDataBytes = Encoding.UTF8.GetBytes(requestData);
                stream.Write(requestDataBytes, 0, requestDataBytes.Length);


                // Ждем подтверждение от сервера
                byte[] buffer = new byte[256];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string confirmationMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                MessageBox.Show("Подтверждение от сервера: " + confirmationMessage, "Уведомление");

                // Закрываем соединение
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }

        private const int port = 12345;
        private TcpListener server;

        public void StartServer()
        {
            server = new TcpListener(IPAddress.Any, 12345); // Создание TcpListener
            server.Start(); // Запуск прослушивания
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            // Начать асинхронное принятие входящих соединений
            server.BeginAcceptTcpClient(new AsyncCallback(HandleClient), null);
        }

        private void HandleClient(IAsyncResult ar)
        {
            TcpClient client = server.EndAcceptTcpClient(ar);
            Console.WriteLine("Клиент подключен.");

            NetworkStream stream = client.GetStream();

            // Чтение длины сообщения
            byte[] messageLengthBytes = new byte[sizeof(int)];
            stream.Read(messageLengthBytes, 0, messageLengthBytes.Length);
            int messageLength = BitConverter.ToInt32(messageLengthBytes, 0);

            // Чтение сообщения
            byte[] messageData = new byte[messageLength];
            int totalBytesRead = 0;
            while (totalBytesRead < messageLength)
            {
                int bytesRead = stream.Read(messageData, totalBytesRead, messageLength - totalBytesRead);
                if (bytesRead == 0)
                {
                    // Если чтение вернуло 0 байт, значит соединение закрыто
                    break;
                }
                totalBytesRead += bytesRead;
            }


            // Проверка, является ли полученный запрос корректным JSON
            bool isJson = IsValidJson(messageData);

            if (isJson)
            {
                // Преобразование JSON в объект запроса
                Request request = JsonSerializer.Deserialize<Request>(messageData);

                // Обработка запроса в зависимости от его типа
                if (request.Type == "ProfileRequest")
                {
                    // Вывод сообщения в MessageBox
                    MessageBox.Show(request.Message, $"Сообщение от клиента {request.uid}", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (request.Type == "ProfileReply")
                {
                    // Обработка файла
                    //byte[] fileContent = request.FileData;
                    // Далее ваша обработка файла...
                }
                else if (request.Type == "File")
                {
                    // Обработка файла
                    //byte[] fileContent = request.FileData;
                    // Далее ваша обработка файла...
                }
            }
            else
            {
                // Вывод сообщения о пинге или другом некорректном запросе
                Console.WriteLine("Получен запрос, который не является JSON.");
            }

            // Отправка подтверждения клиенту
            SendConfirmation(client);

            stream.Close();
            client.Close();

            server.BeginAcceptTcpClient(new AsyncCallback(HandleClient), null);
        }


        private bool IsValidJson(byte[] messageData)
        {
            try
            {
                // Попытка десериализации JSON
                JsonSerializer.Deserialize<Request>(messageData);
                return true;
            }
            catch (Exception)
            {
                // Если десериализация не удалась, запрос не является JSON
                return false;
            }
        }

        private void SendConfirmation(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            string confirmationMessage = "Сообщение получено!";
            byte[] confirmationData = Encoding.UTF8.GetBytes(confirmationMessage);
            stream.Write(confirmationData, 0, confirmationData.Length);
            stream.Close();
            client.Close();
        }
    }
}
