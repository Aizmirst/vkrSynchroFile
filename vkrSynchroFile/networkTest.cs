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
using Newtonsoft.Json;

namespace vkrSynchroFile
{
    internal class networkTest
    {

        private void SendProfileButton()
        {
            try
            {
                // IP-адрес и порт сервера, к которому мы хотим подключиться
                string serverIP = "192.168.0.210"; // Замените на IP-адрес вашего сервера
                int serverPort = 12345; // Замените на порт вашего сервера

                // Создание экземпляра TcpClient для подключения к серверу
                TcpClient client = new TcpClient(serverIP, serverPort);

                // Получаем поток для передачи данных
                NetworkStream stream = client.GetStream();

                // Создание объекта запроса для отправки сообщения
                Request request = new Request
                {
                    Type = "Message",
                    Message = "Привет, сервер!"
                };

                // Преобразование объекта запроса в JSON
                string requestData = JsonConvert.SerializeObject(request);

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

        private void SendFileButton(object sender, RoutedEventArgs e)
        {
            try
            {
                string serverIP = "192.168.0.210";
                int serverPort = 12345;

                TcpClient client = new TcpClient(serverIP, serverPort);
                NetworkStream stream = client.GetStream();

                string filePath = @"C:\Users\Maksim\Desktop\ВКР\папка1\test.PNG"; // Путь к вашему файлу

                // Чтение данных файла
                byte[] fileData = File.ReadAllBytes(filePath);

                // Архивация данных файла
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        // Создание новой записи в архиве
                        ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(filePath));

                        // Запись содержимого файла в архив
                        using (Stream entryStream = entry.Open())
                        using (MemoryStream fileStream = new MemoryStream(fileData))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                    // Получение сжатых данных из MemoryStream
                    fileData = ms.ToArray();
                }

                // Создание объекта запроса для отправки файла
                Request request = new Request
                {
                    Type = "File",
                    FileData = fileData
                };

                // Преобразование объекта запроса в JSON
                string requestData = JsonConvert.SerializeObject(request);

                // Отправка JSON на сервер
                byte[] requestDataBytes = Encoding.UTF8.GetBytes(requestData);
                stream.Write(requestDataBytes, 0, requestDataBytes.Length);

                // Чтение ответа от сервера
                byte[] buffer = new byte[256];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string confirmationMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                MessageBox.Show("Подтверждение от сервера: " + confirmationMessage, "Уведомление");

                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }



        // Порт, на котором сервер будет ожидать входящие соединения
        private const int port = 12345; // Используйте тот же порт, что и на клиенте
        private TcpListener server;

        /*public MainWindow()
        {
            InitializeComponent();
            StartServer(); // Начать прослушивание соединений при запуске приложения
        }*/

        private void StartServer()
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

            // Чтение длины файла
            byte[] fileLengthBytes = new byte[sizeof(int)];
            stream.Read(fileLengthBytes, 0, fileLengthBytes.Length);
            int fileLength = BitConverter.ToInt32(fileLengthBytes, 0);

            // Чтение файла
            byte[] fileData = new byte[fileLength];
            int totalBytesRead = 0;
            while (totalBytesRead < fileLength)
            {
                int bytesRead = stream.Read(fileData, totalBytesRead, fileLength - totalBytesRead);
                totalBytesRead += bytesRead;
            }

            /*// Сохранение файла
            string filePath = @"C:\Users\Дети\Desktop\Универ\test\test.PNG"; // Путь для сохранения файла на сервере
            File.WriteAllBytes(filePath, fileData);*/

            // Разархивация архива
            using (MemoryStream ms = new MemoryStream(fileData))
            using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Read))
            {
                // Извлечение и сохранение файлов из архива
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string filePath = Path.Combine(@"C:\Users\Дети\Desktop\Универ\test", entry.FullName);

                    // Создаем каталоги, если они не существуют
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // Открываем поток для записи данных
                    using (Stream entryStream = entry.Open())
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        // Копируем содержимое из потока записи архива в поток записи файла
                        entryStream.CopyTo(fs);
                    }
                }
            }

            // Отправка подтверждения клиенту
            SendConfirmation(client);

            stream.Close();
            client.Close();

            server.BeginAcceptTcpClient(new AsyncCallback(HandleClient), null);
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
