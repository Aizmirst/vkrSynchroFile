using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using Path = System.IO.Path;


namespace vkrSynchroFile
{
    public partial class MainWindow : Window
    {
        SQLiteManager dbLite;
        MySqlManager dbMySQL;
        private const string FirstRunFlagFileName = "firstrun.marker";
        private string uniqueId;
        InternetNetwork internetNetwork;

        public MainWindow()
        {
            InitializeComponent();
            dbLite = new SQLiteManager();
            dbMySQL = new MySqlManager();
            internetNetwork = new InternetNetwork();
            internetNetwork.StartServer();
            TableUpdate();

            uniqueId = "";
            if (IsFirstRun())
            {
                if(GetLocalIPAddress == null)
                {
                    MessageBox.Show("ip не обнаружен.");
                }
                else
                {
                    // Генерация уникального идентификатора
                    uniqueId = Guid.NewGuid().ToString();
                    dbMySQL.insertDB(GetLocalIPAddress(), uniqueId);
                    // Сохранение уникального идентификатора в файле
                    File.WriteAllText(FirstRunFlagFileName, uniqueId);
                    copiedTextButton.Content = $"Ваш идентефикатор: {uniqueId}";
                }
            }
            else
            {
                // Загрузка уникального идентификатора из файла
                uniqueId = File.ReadAllText(FirstRunFlagFileName);

                // Изменить текст кнопки
                copiedTextButton.Content = $"Ваш идентефикатор: {uniqueId}";
            }
        }

        public static string GetLocalIPAddress()
        {
            // Получаем первый доступный IP-адрес устройства
            string ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                .ToString();
            return ipAddress;

            /*// Получаем список сетевых интерфейсов
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Фильтруем интерфейсы, относящиеся к IPv4 и не являющиеся петлевыми (Loopback)
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                    networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    networkInterface.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet)
                {
                    // Получаем список IP-адресов для текущего интерфейса
                    UnicastIPAddressInformationCollection ipAddresses = networkInterface.GetIPProperties().UnicastAddresses;

                    foreach (UnicastIPAddressInformation ipAddress in ipAddresses)
                    {
                        // Исключаем петлевые адреса и адреса IPv6
                        if (ipAddress.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ipAddress.Address))
                        {
                            return ipAddress.Address.ToString();
                        }
                    }
                }
            }
            // Если не найдено подходящего IP-адреса, возвращаем null или пустую строку, в зависимости от ваших потребностей
            return null;*/
        }

        private static bool IsFirstRun()
        {
            // Проверяем наличие файла-маркера
            return !File.Exists(FirstRunFlagFileName);
        }

        private void TableUpdate()
        {
            ObservableCollection<ListItem> items = new ObservableCollection<ListItem>();
            items = dbLite.readDBforTable();
            itemListBox.ItemsSource = items;
        }

        private void TableUpdateButton(object sender, RoutedEventArgs e)
        {
            TableUpdate();
        }

        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileTypeChooseWindow profileWindow = new ProfileTypeChooseWindow();
            profileWindow.ShowDialog();

            // Получение значения из второго окна
            int selectedAction = profileWindow.SelectedAction;
            switch (selectedAction)
            {
                // Отмена
                case 0:
                    break;
                // Внутри 1 пк
                case 1:
                    PC_CreateProfile pc_CreateProfile = new PC_CreateProfile();
                    pc_CreateProfile.ShowDialog();
                    TableUpdate();
                    break;
                // По локалке
                case 2:
                    TableUpdate();
                    break;
                // По инету
                case 3:
                    Internet_CreateProfile internet_CreateProfile = new Internet_CreateProfile(uniqueId);
                    internet_CreateProfile.ShowDialog();
                    TableUpdate();
                    break;
                default:
                    MessageBox.Show("Произошла ошибка!");
                    break;
            }
        }

        private void ChangeProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли выбранный элемент
            if (itemListBox.SelectedItem != null)
            {
                // Приводим выбранный элемент к типу вашего класса Item
                ListItem selectedItem = (ListItem)itemListBox.SelectedItem;

                switch (selectedItem.profType)
                {
                    case 1:
                        PC_ChangeProfile pc_ChangeProfile = new PC_ChangeProfile(selectedItem);
                        pc_ChangeProfile.ShowDialog();
                        TableUpdate();
                        break;
                    case 2:
                        TableUpdate();
                        break;
                    case 3:
                        TableUpdate();
                        break;
                }
            }
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли выбранный элемент
            if (itemListBox.SelectedItem != null)
            {
                // Приводим выбранный элемент к типу вашего класса Item
                ListItem selectedItem = (ListItem)itemListBox.SelectedItem;

                switch (selectedItem.profType)
                {
                    case 1:
                        dbLite.deleteDB(selectedItem.profile_id, selectedItem.folder1id, selectedItem.folder2id);
                        ((ObservableCollection<ListItem>)itemListBox.ItemsSource).Remove(selectedItem);
                        //TableUpdate();
                        break;
                    case 2:
                        TableUpdate();
                        break;
                    case 3:
                        TableUpdate();
                        break;
                }
            }
        }

        private void ItemListBox_Selection(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли выбранный элемент
            if (itemListBox.SelectedItem != null)
            {
                // Приводим выбранный элемент к типу вашего класса Item
                ListItem selectedItem = (ListItem)itemListBox.SelectedItem;

                // Формируем уведомление с параметрами элемента
                string notification = $"Выбран элемент: {selectedItem.text}\n" +
                                      $"Profile ID: {selectedItem.profile_id}\n" +
                                      $"Folder 1: {selectedItem.folder1path}\n" +
                                      $"Folder 2: {selectedItem.folder2path}";

                // Показываем уведомление
                MessageBox.Show(notification, "Выбор элемента");
            }
        }
        
        private void SynchroFileButton(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли выбранный элемент
            if (itemListBox.SelectedItem != null)
            {
                ListItem selectedItem = (ListItem)itemListBox.SelectedItem;
                Synchro(selectedItem.folder1path, selectedItem.folder2path, selectedItem.profMode);
            }
        }

        public void Synchro(string nameFolder1, string nameFolder2, bool mode)
        {
            if (mode)
            {
                TwoSideSynhroFolders(nameFolder1, nameFolder2);
            }
            else
            {
                OneSideSynhroFolders(nameFolder1, nameFolder2);
            }
        }


        private void OneSideSynhroFolders(string folder1, string folder2)
        {
            // Получаем информацию о файлах в каждой из папок
            string[] folder1Files = Directory.GetFiles(folder1);
            string[] folder2Files = Directory.GetFiles(folder2);

            // Сравниваем файлы в первой папке с файлами во второй папке
            foreach (string filePath1 in folder1Files)
            {
                string fileName = Path.GetFileName(filePath1);
                string filePath2 = Path.Combine(folder2, fileName);

                // Если файл с таким же именем есть во второй папке
                if (Array.Exists(folder2Files, f => Path.GetFileName(f) == fileName))
                {
                    // Получаем информацию о файлах
                    FileData fileData1 = GetFileData(filePath1);
                    FileData fileData2 = GetFileData(filePath2);

                    // Сравниваем хеш-коды файлов
                    if (fileData1.HashCode != fileData2.HashCode)
                    {
                        // Обновляем файл в папке 2
                        SyncFile(filePath1, folder1, folder2);
                    }
                    // Если хеш-коды совпадают, файлы одинаковые, ничего не делаем
                }
                else
                {
                    // Если файл отсутствует во второй папке
                    // Копируем его из первой папки во вторую
                    CopyFile(filePath1, folder1, folder2);
                }
            }

            // Проверяем файлы во второй папке, которые отсутствуют в первой папке
            foreach (string filePath2 in folder2Files)
            {
                string fileName = Path.GetFileName(filePath2);
                string filePath1 = Path.Combine(folder1, fileName);

                // Если файл отсутствует в первой папке, удаляем его из второй
                if (!File.Exists(filePath1))
                {
                    File.Delete(filePath2);
                }
            }

            // Теперь рекурсивно обрабатываем подпапки
            string[] subdirectories1 = Directory.GetDirectories(folder1);
            string[] subdirectories2 = Directory.GetDirectories(folder2);

            // Проверяем подпапки во второй папке, которых нет в первой папке и удаляем их
            foreach (string subdirectory2 in subdirectories2)
            {
                string subdirectoryName = Path.GetFileName(subdirectory2);
                string subdirectory1 = Path.Combine(folder1, subdirectoryName);

                // Если подпапки нет в первой папке, удаляем ее
                if (!Array.Exists(subdirectories1, d => Path.GetFileName(d) == subdirectoryName))
                {
                    Directory.Delete(subdirectory2, true); // Удаляем подпапку и ее содержимое
                }
            }

            // Проверяем наличие одинаковых подпапок
            foreach (string subdirectory1 in subdirectories1)
            {
                string subdirectoryName = Path.GetFileName(subdirectory1);
                string subdirectory2 = Path.Combine(folder2, subdirectoryName);

                if (Array.Exists(subdirectories2, d => Path.GetFileName(d) == subdirectoryName))
                {
                    // Если подпапка с таким же именем есть во второй папке,
                    // вызываем метод анализа для этой подпапки
                    OneSideSynhroFolders(subdirectory1, subdirectory2);
                }
                else
                {
                    // Если подпапки нет во второй папке, создаем ее
                    Directory.CreateDirectory(subdirectory2);
                    // и вызываем метод анализа для этой подпапки
                    OneSideSynhroFolders(subdirectory1, subdirectory2);
                }
            }
        }


        private void TwoSideSynhroFolders(string folder1, string folder2)
        {
            // Получаем информацию о файлах в каждой из папок
            string[] folder1Files = Directory.GetFiles(folder1);
            string[] folder2Files = Directory.GetFiles(folder2);

            // Сравниваем файлы в первой папке с файлами во второй папке
            foreach (string filePath1 in folder1Files)
            {
                string fileName = Path.GetFileName(filePath1);
                string filePath2 = Path.Combine(folder2, fileName);

                // Если файл с таким же именем есть во второй папке
                if (Array.Exists(folder2Files, f => Path.GetFileName(f) == fileName))
                {
                    // Получаем информацию о файлах
                    FileData fileData1 = GetFileData(filePath1);
                    FileData fileData2 = GetFileData(filePath2);

                    // Сравниваем хеш-коды файлов
                    if (fileData1.HashCode != fileData2.HashCode)
                    {
                        // Обновляем файл с более поздней датой изменения
                        if (fileData1.LastModified > fileData2.LastModified)
                        {
                            // Обновляем файл в папке 2
                            SyncFile(filePath1, folder1, folder2);
                        }
                        else
                        {
                            // Обновляем файл в папке 1
                            SyncFile(filePath2, folder2, folder1);
                        }
                    }
                    // Если хеш-коды совпадают, файлы одинаковые, ничего не делаем
                }
                else
                {
                    // Если файл отсутствует во второй папке
                    // Копируем его из первой папки во вторую
                    CopyFile(filePath1, folder1, folder2);
                }
            }

            // Проверяем файлы во второй папке, которые отсутствуют в первой папке
            foreach (string filePath2 in folder2Files)
            {
                string fileName = Path.GetFileName(filePath2);
                string filePath1 = Path.Combine(folder1, fileName);

                // Если файл отсутствует в первой папке
                if (!File.Exists(filePath1))
                {
                    // Копируем его из второй папки в первую
                    CopyFile(filePath2, folder2, folder1);
                }
            }

            // Теперь рекурсивно обрабатываем подпапки
            string[] subdirectories1 = Directory.GetDirectories(folder1);
            string[] subdirectories2 = Directory.GetDirectories(folder2);

            // Проверяем наличие одинаковых подпапок
            foreach (string subdirectory1 in subdirectories1)
            {
                string subdirectoryName = Path.GetFileName(subdirectory1);
                string subdirectory2 = Path.Combine(folder2, subdirectoryName);

                if (Array.Exists(subdirectories2, d => Path.GetFileName(d) == subdirectoryName))
                {
                    // Если подпапка с таким же именем есть во второй папке,
                    // вызываем метод анализа для этой подпапки
                    TwoSideSynhroFolders(subdirectory1, subdirectory2);
                }
                else
                {
                    // Если подпапки нет во второй папке, создаем ее
                    Directory.CreateDirectory(subdirectory2);
                    // и вызываем метод анализа для этой подпапки
                    TwoSideSynhroFolders(subdirectory1, subdirectory2);
                }
            }
            // Проверяем наличие подпапок из folder2 в folder1
            foreach (string subdirectory2 in subdirectories2)
            {
                string subdirectoryName = Path.GetFileName(subdirectory2);
                string subdirectory1 = Path.Combine(folder1, subdirectoryName);

                if (!Array.Exists(subdirectories1, d => Path.GetFileName(d) == subdirectoryName))
                {
                    // Если подпапки нет в первой папке, создаем ее
                    Directory.CreateDirectory(subdirectory1);
                    // и вызываем метод анализа для этой подпапки
                    TwoSideSynhroFolders(subdirectory1, subdirectory2);
                }
            }
        }
        private void SyncFile(string filePath, string sourceFolder, string targetFolder)
        {
            string sourcePath = Path.Combine(sourceFolder, Path.GetFileName(filePath));
            string targetPath = Path.Combine(targetFolder, Path.GetFileName(filePath));
            File.Copy(sourcePath, targetPath, true); // Перезаписываем файл в целевой папке
        }

        private void CopyFile(string filePath, string sourceFolder, string targetFolder)
        {
            string sourcePath = Path.Combine(sourceFolder, Path.GetFileName(filePath));
            string targetPath = Path.Combine(targetFolder, Path.GetFileName(filePath));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)); // Создаем необходимые подпапки
            File.Copy(sourcePath, targetPath, true); // Копируем файл в целевую папку
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

        public string CalculateFileHash(string filePath)
        {
            HashAlgorithm algorithm = new SHA256Managed();
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = algorithm.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        } 

        private void CopyRowButton_Click(object sender, RoutedEventArgs e)
        {
            // Получение текста из строки
            string textToCopy = uniqueId;

            // Копирование текста в буфер обмена
            Clipboard.SetText(textToCopy);
        }

        /*public static void openInternetProfileAccept(string senderUid)
        {
            Internet_SelectSecondFolder internet_SelectSecondFolder = new Internet_SelectSecondFolder(senderUid);
            internet_SelectSecondFolder.ShowDialog();
        }*/
    }
}