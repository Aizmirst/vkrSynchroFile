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
using Mysqlx.Crud;
using Org.BouncyCastle.Asn1.X509;
using static vkrSynchroFile.MainWindow;
using System.Security.Cryptography;
using System.Reflection;
using static vkrSynchroFile.InternetNetwork;
using Microsoft.VisualBasic.ApplicationServices;

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

        public bool DeleteProfile(string userUID, string profileUID)
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


                        if (streamResult != null)
                        {
                            MessageBox.Show("Профиль удалён.");
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Удаление профиля отклонено.");
                            return false;
                        }
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

        public void TriggerOneSideSynchroSend(string ip, string profileUID)
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
                        Type = 5,
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

                    // Закрываем соединение
                    stream.Close();
                    client.Close();
                }
                else
                {
                    MessageBox.Show("Устройство недоступно.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }

        public void OneSideSynchroSend(string ip, string profileUID, string folderPath, List<FileInformation> filesInfo) 
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
                        Type = 3,
                        uid = myUID,
                        folderPath = folderPath,
                        profileUID = profileUID,
                        fileInformation = filesInfo
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

                    // Обработка ответа от сервера
                    if (streamResult == null)
                    {
                        MessageBox.Show("Ошибка синхронизации.");
                    }
                    else
                    {
                        // Обработка ответа прошла успешно, здесь можно отправить еще один запрос, если нужно

                        // Например:
                        List<FileInformation> listForSynchro = OneSideReadyFilesForSend(streamResult.fileInformation);
                        OneSideSynchroSendFile(ip, profileUID, folderPath, listForSynchro);

                    }
                    /*// Ждем подтверждение от сервера
                    byte[] buffer = new byte[256];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string confirmationMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageBox.Show("Подтверждение от сервера: " + confirmationMessage, "Уведомление");*/

                    // Закрываем соединение
                    stream.Close();
                    client.Close();
                }
                else
                {
                    MessageBox.Show("Устройство недоступно.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }

        private List<FileInformation> OneSideReadyFilesForSend(List<FileInformation> fileInformation)
        {
            List<FileInformation> newList = new List<FileInformation>();
            foreach (var fileInfo in fileInformation)
            {
                // Загрузка файла
                byte[] fileData = File.ReadAllBytes(fileInfo.Path);

                fileInfo.FileData = fileData;
                newList.Add(fileInfo);
            }
            return newList;
        }

        private void OneSideSynchroSendFile(string ip, string profileUID, string folderPath, List<FileInformation> list)
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
                        Type = 4,
                        uid = myUID,
                        profileUID = profileUID,
                        folderPath = folderPath,
                        fileInformation = list
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

                    // Обработка ответа от сервера
                    if (streamResult == null)
                    {
                        MessageBox.Show("Процесс синхронизации завершён.");
                    }

                    // Закрываем соединение
                    stream.Close();
                    client.Close();
                }
                else
                {
                    MessageBox.Show("Устройство недоступно.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }

        public void TwoSideSynchroSend(string ip, string profileUID, string folderPath, List<FileInformation> filesInfo)
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
                        Type = 6,
                        uid = myUID,
                        folderPath = folderPath,
                        profileUID = profileUID,
                        fileInformation = filesInfo
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

                    // Обработка ответа от сервера
                    if (streamResult == null)
                    {
                        MessageBox.Show("Ошибка синхронизации.");
                    }
                    else
                    {
                        // Обработка ответа прошла успешно, здесь можно отправить еще один запрос, если нужно
                        List<FileInformation> listForSynchro = TwoSideReadyFilesForSend(streamResult.fileInformation, folderPath);
                        TwoSideSynchroSendFile(ip, profileUID, folderPath, listForSynchro);

                    }
                    /*// Ждем подтверждение от сервера
                    byte[] buffer = new byte[256];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string confirmationMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageBox.Show("Подтверждение от сервера: " + confirmationMessage, "Уведомление");*/

                    // Закрываем соединение
                    stream.Close();
                    client.Close();
                }
                else
                {
                    MessageBox.Show("Устройство недоступно.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }
        private List<FileInformation> TwoSideReadyFilesForSend(List<FileInformation> fileInformation, string folder)
        {
            List<FileInformation> newList = new List<FileInformation>();
            foreach (var fileInfo in fileInformation)
            {
                if (!fileInfo.IsDirectory)
                {
                    if (fileInfo.FileData == null)
                    {
                        // Загрузка файла
                        byte[] fileData = File.ReadAllBytes(fileInfo.Path);

                        fileInfo.FileData = fileData;
                        newList.Add(fileInfo);
                    }
                    else
                    {
                        File.WriteAllBytes(fileInfo.Path, fileInfo.FileData);
                    }
                }
                else
                {
                    /*string directoryName = GetRelativePath(fileInfo.Path, folder1path);
                    string directoryPath2 = Path.Combine(folder2, directoryName);*/
                    if (!Directory.Exists(fileInfo.Path))
                    {
                        // Если подпапки нет во второй папке, создаем ее
                        Directory.CreateDirectory(fileInfo.Path);
                    }
                }
                
            }
            return newList;
        }

        private void TwoSideSynchroSendFile(string ip, string profileUID, string folderPath, List<FileInformation> list)
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
                        Type = 7,
                        uid = myUID,
                        profileUID = profileUID,
                        folderPath = folderPath,
                        fileInformation = list
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

                    // Обработка ответа от сервера
                    if (streamResult == null)
                    {
                        MessageBox.Show("Процесс синхронизации завершён.");
                    }

                    // Закрываем соединение
                    stream.Close();
                    client.Close();
                }
                else
                {
                    MessageBox.Show("Устройство недоступно.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
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
                switch (request.Type)
                {
                    case 1:
                        //Обработка запроса но создание профиля
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
                            if (internet_SelectSecondFolder.acceptProfile)
                            {
                                MySqlManager myDB = new MySqlManager();
                                string ip = myDB.searchIP_DB(request.uid);

                                SQLiteManager db = new SQLiteManager();
                                DirectoryInfo directoryInfo = new DirectoryInfo(internet_SelectSecondFolder.folderpath);
                                db.insertInternetDB(request.synhroMode, directoryInfo.Name, directoryInfo.FullName, directoryInfo.LastWriteTime, directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length), request.uid, internet_SelectSecondFolder.uniqueId, false);
                                //AcceptProfile(ip, internet_SelectSecondFolder.uniqueId);
                                string myUID = InternetProfileMethods.myUserUID();
                                Request newRequest = new Request
                                {
                                    uid = myUID,
                                    profileUID = internet_SelectSecondFolder.uniqueId
                                };
                                SendConfirmation(newRequest, client);
                                //mainWindowInstance.TableUpdate();
                            }
                            else
                            {
                                Request newRequest = null;
                                SendConfirmation(newRequest, client);
                            }
                        });
                        break;
                    case 2:
                        // Обработка запроса на удаление профиля
                        MessageBoxResult result = MessageBox.Show($"Получен запрос на удаления профиля {request.profileUID}. \n\n\n Вы подтверждаете удаление профиля?", "Удалить профиль?", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            SQLiteManager dbLite = new SQLiteManager();
                            dbLite.deleteDB_Internet(request.uid, request.profileUID);
                            Request newRequest = new Request();
                            SendConfirmation(newRequest, client);
                            //mainWindowInstance.TableUpdate();
                        }
                        else
                        {
                            Request newRequest = null;
                            SendConfirmation(newRequest, client);
                        }
                        break;
                    case 3:
                        //Обработка односторонней синхронизации
                        List<FileInformation> fileInformation = request.fileInformation;

                        // формирование списка файлов, которые нужно получить
                        SQLiteManager db = new SQLiteManager();
                        List<FileInformation> newfileInformation = OneSideAnalisFileInformation(fileInformation, request.folderPath, db.getFolderPathInternetProfile(request.profileUID));
                        string myUID = InternetProfileMethods.myUserUID();
                        Request nRequest = new Request
                        {
                            Type = 3,
                            uid = myUID,
                            fileInformation = newfileInformation
                        };
                        SendConfirmation(nRequest, client);
                        break;
                    case 4:
                        // Получение и синхронизация файлов
                        List<FileInformation> files = request.fileInformation;
                        SQLiteManager db4 = new SQLiteManager();
                        WriteNewFiles(files, request.folderPath, db4.getFolderPathInternetProfile(request.profileUID));
                        Request newReq = null;
                        SendConfirmation(newReq, client);
                        break; 
                    case 5:
                        MySqlManager dbMySQL = new MySqlManager();
                        if (dbMySQL.searchDB(request.uid))
                        {
                            SQLiteManager db5 = new SQLiteManager();
                            List<FileInformation> result5 = AnalisFolderForInternet(db5.getFolderPathInternetProfile(request.profileUID));
                            string userIP = dbMySQL.searchIP_DB(request.uid);
                            string folderPath = db5.getFolderPathInternetProfile(request.profileUID);
                            OneSideSynchroSend(userIP, request.profileUID, folderPath, result5);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка с идентификаторе второго устройства!");
                        }
                        break;
                    case 6:
                        //Обработка односторонней синхронизации
                        List<FileInformation> fileInformation6 = request.fileInformation;

                        // формирование списка файлов, которые нужно получить
                        SQLiteManager db6 = new SQLiteManager();
                        List<FileInformation> newfileInformation6 = TwoSideAnalisFileInformation(fileInformation6, request.folderPath, db6.getFolderPathInternetProfile(request.profileUID));
                        string myUID6 = InternetProfileMethods.myUserUID();
                        Request nRequest6 = new Request
                        {
                            Type = 6,
                            uid = myUID6,
                            fileInformation = newfileInformation6
                        };
                        SendConfirmation(nRequest6, client);
                        break;
                    case 7:
                        // Получение и синхронизация файлов
                        List<FileInformation> files7 = request.fileInformation;
                        SQLiteManager db7 = new SQLiteManager();
                        WriteNewFiles(files7, request.folderPath, db7.getFolderPathInternetProfile(request.profileUID));
                        Request newReq7 = null;
                        SendConfirmation(newReq7, client);
                        break;
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

        private List<FileInformation> OneSideAnalisFileInformation(List<FileInformation> fileList, string folder1path, string folder2)
        {
            List <FileInformation> newList = new List<FileInformation>();
            List<FileInformation> folder2List = AnalisFolderForInternet(folder2);

            foreach (var fileInfo in fileList)
            {
                if (!fileInfo.IsDirectory)
                {
                    string fileName = GetRelativePath(fileInfo.Path, folder1path);
                    string filePath2 = Path.Combine(folder2, fileName);

                    // Получаем информацию о файле из списка folder2List
                    var fileData2 = folder2List.FirstOrDefault(f => f.Name == fileInfo.Name && !f.IsDirectory);

                    if (fileData2 != null)
                    {
                        if (fileInfo.HashCode != fileData2.HashCode)
                        {
                            // Если хеш-коды файлов не совпадают, обновляем файл в папке 2
                            fileInfo.ForSynchro = true;
                            newList.Add(fileInfo);
                        }
                    }
                    else
                    {
                        // Если файл отсутствует во второй папке, копируем его из первой
                        fileInfo.ForCopy = true;
                        newList.Add(fileInfo);
                    }
                }
                else
                {
                    string directoryName = GetRelativePath(fileInfo.Path, folder1path);
                    string directoryPath2 = Path.Combine(folder2, directoryName);
                    if (!Directory.Exists(directoryPath2))
                    {
                        // Если подпапки нет во второй папке, создаем ее
                        Directory.CreateDirectory(directoryPath2);
                    }
                }
            }

            // Проверяем файлы во втором списке, которые отсутствуют в первом списке
            foreach (var fileInfo2 in folder2List)
            {
                string fileName = GetRelativePath(fileInfo2.Path, folder2);
                string filePath1 = Path.Combine(folder1path, fileName);

                // Если файл отсутствует в первом списке, удаляем его
                if (!fileList.Any(f => f.Path == filePath1))
                {
                    if (!fileInfo2.IsDirectory)
                    {
                        File.Delete(fileInfo2.Path);
                    }
                    else
                    {
                        Directory.Delete(fileInfo2.Path, true);
                    }
                }
            }

            return newList;
        }

        private List<FileInformation> TwoSideAnalisFileInformation(List<FileInformation> fileList, string folder1path, string folder2)
        {
            List<FileInformation> newList = new List<FileInformation>();
            // Анализ папки на этом устройстве
            List<FileInformation> folder2List = AnalisFolderForInternet(folder2);

            foreach (var fileInfo in fileList)
            {
                if (!fileInfo.IsDirectory)
                {
                    string fileName = GetRelativePath(fileInfo.Path, folder1path);
                    string filePath2 = Path.Combine(folder2, fileName);

                    // Получаем информацию о файле из списка folder2List
                    var fileData2 = folder2List.FirstOrDefault(f => f.Name == fileInfo.Name && !f.IsDirectory);

                    if (fileData2 != null)
                    {
                        if (fileInfo.HashCode != fileData2.HashCode)
                        {
                            if(fileInfo.LastModified > fileData2.LastModified)
                            {
                                // Если file1 более новый, значит запрашиваем его на синхронизацию
                                fileInfo.ForSynchro = true;
                                newList.Add(fileInfo);
                            }
                            else
                            {
                                byte[] fileData = File.ReadAllBytes(fileData2.Path);
                                fileInfo.ForSynchro = true;
                                fileInfo.FileData = fileData;
                                newList.Add(fileInfo);
                            }
                            
                        }
                    }
                    else
                    {
                        // Если файл отсутствует во второй папке, копируем его из первой
                        fileInfo.ForCopy = true;
                        newList.Add(fileInfo);
                    }
                }
                else
                {
                    string directoryName = GetRelativePath(fileInfo.Path, folder1path);
                    string directoryPath2 = Path.Combine(folder2, directoryName);
                    if (!Directory.Exists(directoryPath2))
                    {
                        // Если подпапки нет во второй папке, создаем ее
                        Directory.CreateDirectory(directoryPath2);
                    }
                }
            }

            // Проверяем файлы во втором списке, которые отсутствуют в первом списке
            foreach (var fileInfo2 in folder2List)
            {
                string fileName = GetRelativePath(fileInfo2.Path, folder2);
                string filePath1 = Path.Combine(folder1path, fileName);

                // Если файл отсутствует в первом списке, удаляем его
                if (!fileList.Any(f => f.Path == filePath1))
                {
                    if (!fileInfo2.IsDirectory)
                    {
                        FileInformation fileInfo = GetFileInformation(fileInfo2.Path);
                        byte[] fileData = File.ReadAllBytes(fileInfo2.Path);
                        fileInfo.ForSynchro = true;
                        fileInfo.FileData = fileData;
                        newList.Add(fileInfo);
                    }
                    else
                    {
                        FileInformation fileInfo = GetFileInformation(fileInfo2.Path);
                        fileInfo.Path = filePath1;
                        fileInfo.needCreate = true;
                        newList.Add(fileInfo);
                    }
                }
            }

            return newList;
        }

        private void WriteNewFiles(List<FileInformation> list, string folder1path, string folder2)
        {
            foreach(var fileInfo in list)
            {
                string fileName = GetRelativePath(fileInfo.Path, folder1path);
                string filePath2 = Path.Combine(folder2, fileName);
                // Сохраняем файл по пути filePath2
                File.WriteAllBytes(filePath2, fileInfo.FileData);
            }
        }

        // Функция для получения относительного пути относительно folder1path
        private string GetRelativePath(string fullPath, string basePath)
        {
            /*Uri fullUri = new Uri(fullPath);
            Uri baseUri = new Uri(basePath);
            return baseUri.MakeRelativeUri(fullUri).ToString();*/
            string relativePath = Path.GetRelativePath(basePath, fullPath).Replace('\\', '/');
            return relativePath;
        }

        private List<FileInformation> AnalisFolderForInternet(string folderPath)
        {
            List<FileInformation> filesInfo = new List<FileInformation>();

            // Получаем информацию о файлах и подпапках в текущей папке
            string[] files = Directory.GetFiles(folderPath);
            foreach (string filePath in files)
            {
                // Получаем информацию о файле и добавляем ее в список
                FileInformation fileInfo = GetFileInformation(filePath);
                filesInfo.Add(fileInfo);
            }

            // Получаем информацию о подпапках и их содержимом
            string[] subdirectories = Directory.GetDirectories(folderPath);
            foreach (string subdirectory in subdirectories)
            {
                // Получаем информацию о подпапке и добавляем ее в список
                FileInformation subdirectoryInfo = GetFileInformation(subdirectory);
                filesInfo.Add(subdirectoryInfo);

                // Рекурсивно получаем информацию о файлах в подпапке
                List<FileInformation> subdirectoryFilesInfo = AnalisFolderForInternet(subdirectory);
                filesInfo.AddRange(subdirectoryFilesInfo);
            }

            return filesInfo;
        }

        public FileData GetFileData(string filePath)
        {
            return new FileData()
            {
                LastModified = File.GetLastWriteTime(filePath),
                HashCode = CalculateFileHash(filePath)
            };
        }

        public class FileData
        {
            public DateTime LastModified { get; set; }
            public string HashCode { get; set; }
        }

        public static string CalculateFileHash(string path)
        {
            if (!File.Exists(path)) return null;

            using (var algorithm = SHA256.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    byte[] hashBytes = algorithm.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
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
