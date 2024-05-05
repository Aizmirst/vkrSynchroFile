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
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.Ocsp;

namespace vkrSynchroFile
{
    internal class InternetNetwork
    {
        private MainWindow mainWindowInstance; // Поле для хранения ссылки на экземпляр MainWindow

        // Конструктор, который принимает ссылку на экземпляр MainWindow
        public InternetNetwork(MainWindow mainWindow)
        {
            mainWindowInstance = mainWindow;
        }
        
        public InternetNetwork()
        {
        }

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
        }

        // Отправка запроса на второе устройство для создания профиля
        public Request SendProfile(string ip, bool synhroMode)
        {
            try
            {
                if (PingDevice(ip))
                {
                    // IP-адрес и порт сервера, к которому мы хотим подключиться
                    string serverIP = ip; // Замените на IP-адрес вашего сервера
                    int serverPort = 12345; // Замените на порт вашего сервера

                    // Создание экземпляра TcpClient для подключения к серверу
                    TcpClient client = new TcpClient(serverIP, serverPort);

                    // Получаем поток для передачи данных
                    NetworkStream stream = client.GetStream();
                    string myUID = InternetProfileMethods.myUserUID();
                    // Создание объекта запроса для отправки сообщения
                    Request request = new Request
                    {
                        Type = 1,
                        uid = myUID,
                        synhroMode = synhroMode
                    };

                    // Преобразование объекта запроса в JSON
                    string requestData = JsonSerializer.Serialize(request);

                    // Получение длины сообщения в байтах
                    byte[] messageLengthBytes = BitConverter.GetBytes(requestData.Length);
                    stream.Write(messageLengthBytes, 0, messageLengthBytes.Length);

                    // Отправка JSON на сервер
                    byte[] requestDataBytes = Encoding.UTF8.GetBytes(requestData);
                    stream.Write(requestDataBytes, 0, requestDataBytes.Length);

                    // Получение и обработка ответа от сервера
                    Request streamResult =  ProcessServerResponse(stream);

                    /*// Ждем подтверждение от сервера
                    byte[] buffer = new byte[256];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string confirmationMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageBox.Show("Подтверждение от сервера: " + confirmationMessage, "Уведомление");*/

                    // Закрываем соединение
                    stream.Close();
                    client.Close();


                    if (streamResult == null)
                    {
                        MessageBox.Show("Создание профиля не одобрено вторым устройством.");
                    }

                    return streamResult;
                }
                else
                {
                    MessageBox.Show("Устройство недоступно.", "Ошибка");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
                return null;
            }
        }

        public bool deleteProfile(string userUID, string profileUID)
        {
            try
            {
                MySqlManager myDB = new MySqlManager();
                if (myDB.searchDB(userUID)){
                    string ip = myDB.searchIP_DB(userUID);

                    if (PingDevice(ip))
                    {
                        // IP-адрес и порт сервера, к которому мы хотим подключиться
                        string serverIP = ip; // Замените на IP-адрес вашего сервера
                        int serverPort = 12345; // Замените на порт вашего сервера

                        // Создание экземпляра TcpClient для подключения к серверу
                        TcpClient client = new TcpClient(serverIP, serverPort);

                        // Получаем поток для передачи данных
                        NetworkStream stream = client.GetStream();
                        string myUID = InternetProfileMethods.myUserUID();
                        // Создание объекта запроса для отправки сообщения
                        Request request = new Request
                        {
                            Type = 2,
                            uid = myUID,
                            profileUID = profileUID
                        };

                        // Преобразование объекта запроса в JSON
                        string requestData = JsonSerializer.Serialize(request);

                        // Получение длины сообщения в байтах
                        byte[] messageLengthBytes = BitConverter.GetBytes(requestData.Length);
                        stream.Write(messageLengthBytes, 0, messageLengthBytes.Length);

                        // Отправка JSON на сервер
                        byte[] requestDataBytes = Encoding.UTF8.GetBytes(requestData);
                        stream.Write(requestDataBytes, 0, requestDataBytes.Length);

                        // Получение и обработка ответа от сервера
                        Request streamResult = ProcessServerResponse(stream);

                        // Закрываем соединение
                        stream.Close();
                        client.Close();


                        if (streamResult == null)
                        {
                            MessageBox.Show("Создание профиля не одобрено вторым устройством.");
                        }

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Устройство недоступно.", "Ошибка");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
                return false;
            }
        }

        private Request ProcessServerResponse(NetworkStream stream)
        {
            try
            {
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
                    // Преобразование JSON в объект ответа
                    Request response = JsonSerializer.Deserialize<Request>(messageData);
                    return response;

                }
                return null;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обработке ответа от сервера: " + ex.Message, "Ошибка");
                return null;
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
                //ProfileRequest
                if (request.Type == 1)
                {
                    //MainWindow.openInternetProfileAccept(request.uid);
                    /*Internet_SelectSecondFolder internet_SelectSecondFolder = new Internet_SelectSecondFolder();
                    internet_SelectSecondFolder.Owner = Application.Current.MainWindow;
                    internet_SelectSecondFolder.senderUID = request.uid;
                    //internet_SelectSecondFolder.senderUID = request.uid;
                    internet_SelectSecondFolder.ShowDialog();*/
                    // В другом классе или потоке
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Создаем экземпляр диалогового окна и передаем значение в конструкторе
                        Internet_SelectSecondFolder internet_SelectSecondFolder = new Internet_SelectSecondFolder();
                        // Устанавливаем владельца диалогового окна
                        /*internet_SelectSecondFolder.Owner = Application.Current.MainWindow;
                        internet_SelectSecondFolder.senderUID = request.uid;*/

                        // Показываем диалоговое окно
                        internet_SelectSecondFolder.ShowDialog();
                        //MessageBox.Show(internet_SelectSecondFolder.uniqueId);
                        //MessageBox.Show(internet_SelectSecondFolder.folderpath);
                        if(internet_SelectSecondFolder.acceptProfile)
                        {
                            MySqlManager myDB = new MySqlManager();
                            string ip = myDB.searchIP_DB(request.uid);

                            SQLiteManager db = new SQLiteManager();
                            DirectoryInfo directoryInfo = new DirectoryInfo(internet_SelectSecondFolder.folderpath);
                            db.insertInternetDB(request.synhroMode, directoryInfo.Name, directoryInfo.FullName, directoryInfo.LastWriteTime, directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length), request.uid, internet_SelectSecondFolder.uniqueId);
                            //AcceptProfile(ip, internet_SelectSecondFolder.uniqueId);
                            string myUID = InternetProfileMethods.myUserUID();
                            Request newRequest = new Request
                            {
                                uid = myUID,
                                profileUID = internet_SelectSecondFolder.uniqueId
                            };
                            SendConfirmation(newRequest, client);
                            mainWindowInstance.TableUpdate();
                        }
                        else
                        {
                            Request newRequest = null;
                            SendConfirmation(newRequest, client);
                        }
                    });
                    // Вывод сообщения в MessageBox
                    //MessageBox.Show(request.Message, $"Сообщение от клиента {request.uid}", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                //ProfileReply
                else if (request.Type == 2)
                {
                    //MessageBox.Show(request.uid);
                    //MessageBox.Show(request.profileUID);
                    // Обработка файла
                    //byte[] fileContent = request.FileData;
                    // Далее ваша обработка файла...
                }
                //File
                else if (request.Type == 3)
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
            //SendConfirmation(client);

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

        private void SendConfirmation(Request request, TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                /*string confirmationMessage = "Сообщение получено!";
                byte[] confirmationData = Encoding.UTF8.GetBytes(confirmationMessage);
                stream.Write(confirmationData, 0, confirmationData.Length);*/

                // Преобразование объекта запроса в JSON
                string requestData = JsonSerializer.Serialize(request);

                // Получение длины сообщения в байтах
                byte[] messageLengthBytes = BitConverter.GetBytes(requestData.Length);
                stream.Write(messageLengthBytes, 0, messageLengthBytes.Length);

                // Отправка JSON на клиентский узел
                byte[] requestDataBytes = Encoding.UTF8.GetBytes(requestData);
                stream.Write(requestDataBytes, 0, requestDataBytes.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }
    }
}
