using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Timers;
using System.Windows;
using Path = System.IO.Path;
using Timer = System.Timers.Timer;


namespace vkrSynchroFile
{
    public partial class MainWindow : Window
    {
        SQLiteManager dbLite;
        MySqlManager dbMySQL;
        private const string FirstRunFlagFileName = "firstrun.marker";
        private string uniqueId;
        InternetNetwork internetNetwork;
        private Timer timer;

        public MainWindow()
        {
            InitializeComponent();
            dbLite = new SQLiteManager();
            dbMySQL = new MySqlManager();
            internetNetwork = new InternetNetwork(this);
            internetNetwork.StartServer();
            TableUpdate();

            uniqueId = "";
            if (IsFirstRun())
            {
                if (GetLocalIPAddress == null)
                {
                    MessageBox.Show("ip для создание иденификатора не обнаружен.");
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
                string ip = dbMySQL.searchIP_DB(uniqueId);

                if (ip != null)
                {
                    if (GetLocalIPAddress() != ip && GetLocalIPAddress() != null)
                    {
                        //обновить ip в таблцие
                        dbMySQL.updateDB(GetLocalIPAddress(), uniqueId);
                    }
                }
                else
                {
                    if (GetLocalIPAddress != null)
                    {
                        // Генерация уникального идентификатора
                        uniqueId = Guid.NewGuid().ToString();
                        dbMySQL.insertDB(GetLocalIPAddress(), uniqueId);
                        // Сохранение уникального идентификатора в файле
                        File.WriteAllText(FirstRunFlagFileName, uniqueId);
                    }
                }

                // Изменить текст кнопки
                copiedTextButton.Content = $"Ваш идентефикатор: {uniqueId}";
            }


            // Создаем таймер
            timer = new Timer();

            // Устанавливаем интервал проверки в миллисекундах
            timer.Interval = 1000; // Проверяем каждую секунду

            // Устанавливаем обработчик события, который будет вызываться каждый раз при срабатывании таймера
            timer.Elapsed += Timer_Elapsed;

            // Запускаем таймер
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Получаем текущее время
            DateTime currentTime = DateTime.Now;

            // Проверяем, если текущее время равно 00:00 или 12:00, то вызываем метод Synchro для всех элементов в itemListBox
            if ((currentTime.Hour == 0 && currentTime.Minute == 0) || (currentTime.Hour == 12 && currentTime.Minute == 0))
            {
                // Вызываем метод Synchro для всех элементов в itemListBox
                foreach (ListItem item in itemListBox.Items)
                {
                    Synchro(item);
                }
            }
        }

        public static string GetLocalIPAddress()
        {
            // Получаем первый доступный IP-адрес устройства
            string ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                .ToString();
            return ipAddress;
        }

        private static bool IsFirstRun()
        {
            // Проверяем наличие файла-маркера
            return !File.Exists(FirstRunFlagFileName);
        }

        public void TableUpdate()
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
                    case 3:
                        Internet_ChangeProfile internet_ChangeProfile = new Internet_ChangeProfile(selectedItem);
                        internet_ChangeProfile.ShowDialog();
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
                        dbLite.deleteDB_PC(selectedItem.profile_id, selectedItem.folder1id, selectedItem.folder2id);
                        //((ObservableCollection<ListItem>)itemListBox.ItemsSource).Remove(selectedItem);
                        TableUpdate();
                        break;
                    case 3:
                        InternetNetwork internetNetwork = new InternetNetwork();
                        bool deleteCheck = internetNetwork.DeleteProfile(selectedItem.userUID, selectedItem.profileUID);
                        if (deleteCheck)
                        {
                            dbLite.deleteDB_Internet(selectedItem.userUID, selectedItem.profileUID);
                            TableUpdate();
                        }
                        break;
                }
            }
        }

        private void SynchroFileButton(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли выбранный элемент
            if (itemListBox.SelectedItem != null)
            {
                ListItem selectedItem = (ListItem)itemListBox.SelectedItem;
                Synchro(selectedItem);
            }
        }

        private void Synchro(ListItem selectedItem)
        {
            switch (selectedItem.profType)
            {
                case 1:
                    SynchroPC(selectedItem.folder1path, selectedItem.folder2path, selectedItem.profMode);
                    break;
                case 3:
                    SynchroInternet(selectedItem.folder1path, selectedItem.userUID, selectedItem.profileUID, selectedItem.profMode, selectedItem.mainUser);
                    break;
            }
        }

        public void SynchroInternet(string nameFolder1, string userUID, string profileUID, bool mode, bool mainUser)
        {
            if (mode)
            {
                List<FileInformation> result = AnalisFolderForInternet(nameFolder1);
                string userIP = dbMySQL.searchIP_DB(userUID);
                string folderPath = dbLite.getFolderPathInternetProfile(profileUID);
                internetNetwork.TwoSideSynchroSend(userIP, profileUID, folderPath, result);
            }
            else
            {
                if (mainUser)
                {
                    if (dbMySQL.searchDB(userUID))
                    {
                        List<FileInformation> result = AnalisFolderForInternet(nameFolder1);
                        string userIP = dbMySQL.searchIP_DB(userUID);
                        string folderPath = dbLite.getFolderPathInternetProfile(profileUID);
                        internetNetwork.OneSideSynchroSend(userIP, profileUID, folderPath, result);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка в идентификаторе второго устройства!");
                    }
                }
                else
                {
                    string userIP = dbMySQL.searchIP_DB(userUID);
                    internetNetwork.TriggerOneSideSynchroSend(userIP, profileUID);
                }
            }
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

        public static FileInformation GetFileInformation(string path)
        {
            return new FileInformation()
            {
                Name = Path.GetFileName(path),
                Path = path,
                LastModified = File.GetLastWriteTime(path),
                HashCode = CalculateFileHash(path),
                IsDirectory = Directory.Exists(path)
            };
        }

        public void SynchroPC(string nameFolder1, string nameFolder2, bool mode)
        {
            if (mode)
            {
                TwoSideSynhroFoldersPC(nameFolder1, nameFolder2);
            }
            else
            {
                OneSideSynhroFoldersPC(nameFolder1, nameFolder2);
            }
        }


        private void OneSideSynhroFoldersPC(string folder1, string folder2)
        {
            // Получаем информацию о файлах в каждой из папок
            string[] folder1Files = Directory.GetFiles(folder1);
            string[] folder2Files = Directory.GetFiles(folder2);

            // Сравниваем файлы в первой папке с файлами во второй папке
            foreach (string filePath1 in folder1Files)
            {
                string fileName = Path.GetFileName(filePath1);
                string filePath2 = Path.Combine(folder2, fileName.Replace('/', '\\'));

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
                string filePath1 = Path.Combine(folder1, fileName.Replace('/', '\\'));

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
                string subdirectory1 = Path.Combine(folder1, subdirectoryName.Replace('/', '\\'));

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
                string subdirectory2 = Path.Combine(folder2, subdirectoryName.Replace('/', '\\'));

                if (Array.Exists(subdirectories2, d => Path.GetFileName(d) == subdirectoryName))
                {
                    // Если подпапка с таким же именем есть во второй папке,
                    // вызываем метод анализа для этой подпапки
                    OneSideSynhroFoldersPC(subdirectory1, subdirectory2);
                }
                else
                {
                    // Если подпапки нет во второй папке, создаем ее
                    Directory.CreateDirectory(subdirectory2);
                    // и вызываем метод анализа для этой подпапки
                    OneSideSynhroFoldersPC(subdirectory1, subdirectory2);
                }
            }
        }


        private void TwoSideSynhroFoldersPC(string folder1, string folder2)
        {
            // Получаем информацию о файлах в каждой из папок
            string[] folder1Files = Directory.GetFiles(folder1);
            string[] folder2Files = Directory.GetFiles(folder2);

            // Сравниваем файлы в первой папке с файлами во второй папке
            foreach (string filePath1 in folder1Files)
            {
                string fileName = Path.GetFileName(filePath1);
                string filePath2 = Path.Combine(folder2, fileName.Replace('/', '\\'));

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
                string filePath1 = Path.Combine(folder1, fileName.Replace('/', '\\'));

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
                string subdirectory2 = Path.Combine(folder2, subdirectoryName.Replace('/', '\\'));

                if (Array.Exists(subdirectories2, d => Path.GetFileName(d) == subdirectoryName))
                {
                    // Если подпапка с таким же именем есть во второй папке,
                    // вызываем метод анализа для этой подпапки
                    TwoSideSynhroFoldersPC(subdirectory1, subdirectory2);
                }
                else
                {
                    // Если подпапки нет во второй папке, создаем ее
                    Directory.CreateDirectory(subdirectory2);
                    // и вызываем метод анализа для этой подпапки
                    TwoSideSynhroFoldersPC(subdirectory1, subdirectory2);
                }
            }
            // Проверяем наличие подпапок из folder2 в folder1
            foreach (string subdirectory2 in subdirectories2)
            {
                string subdirectoryName = Path.GetFileName(subdirectory2);
                string subdirectory1 = Path.Combine(folder1, subdirectoryName.Replace('/', '\\'));

                if (!Array.Exists(subdirectories1, d => Path.GetFileName(d) == subdirectoryName))
                {
                    // Если подпапки нет в первой папке, создаем ее
                    Directory.CreateDirectory(subdirectory1);
                    // и вызываем метод анализа для этой подпапки
                    TwoSideSynhroFoldersPC(subdirectory1, subdirectory2);
                }
            }
        }

        private void SyncFile(string filePath, string sourceFolder, string targetFolder)
        {
            string sourcePath = Path.Combine(sourceFolder, Path.GetFileName(filePath).Replace('/', '\\'));
            string targetPath = Path.Combine(targetFolder, Path.GetFileName(filePath).Replace('/', '\\'));
            File.Copy(sourcePath, targetPath, true); // Перезаписываем файл в целевой папке
        }

        private void CopyFile(string filePath, string sourceFolder, string targetFolder)
        {
            string sourcePath = Path.Combine(sourceFolder, Path.GetFileName(filePath).Replace('/', '\\'));
            string targetPath = Path.Combine(targetFolder, Path.GetFileName(filePath).Replace('/', '\\'));
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

        private void CopyRowButton_Click(object sender, RoutedEventArgs e)
        {
            // Получение текста из строки
            string textToCopy = uniqueId;

            // Копирование текста в буфер обмена
            Clipboard.SetText(textToCopy);
        }
    }
}