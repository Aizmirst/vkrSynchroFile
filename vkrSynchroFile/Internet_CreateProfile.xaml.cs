using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace vkrSynchroFile
{
    public partial class Internet_CreateProfile : Window
    {
        private string uid;
        public Internet_CreateProfile(string uid)
        {
            InitializeComponent();
            this.uid = uid;
        }

        private void centerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (centerRadioButton.IsChecked == true && calendar != null)
            {
                calendar.IsEnabled = false;
                foreach (var button in daysButtonsPanel.Children.OfType<ToggleButton>())
                {
                    button.IsEnabled = true;
                }
            }
        }

        private void lowerRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (lowerRadioButton.IsChecked == true && calendar != null)
            {
                calendar.IsEnabled = true;
                foreach (var button in daysButtonsPanel.Children.OfType<ToggleButton>())
                {
                    button.IsEnabled = false;
                }
            }
        }


        private string folder1name;
        private string folder1path;
        private void FolderSelectClick(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            folderBrowserDialog.Description = "Выберите папку";

            if (folderBrowserDialog.ShowDialog(this).GetValueOrDefault())
            {
                string selectedFolder = folderBrowserDialog.SelectedPath;

                if (Directory.Exists(selectedFolder))
                {
                    string[] folderInfo = GetFolderInfo(selectedFolder);
                    folder1name = folderInfo[0];
                    folder1path = folderInfo[1];
                    foldere1Info.Text = $"Имя папки: {folderInfo[0]};\nПолный путь: {folderInfo[1]}.";
                }
            }
        }


        string userInput;
        bool userStatus;
        string userIP;

        private void UserSelectClick(object sender, RoutedEventArgs e)
        {
            InputUserIDWindow dialog = new InputUserIDWindow();
            dialog.Owner = this; // Установите основное окно как владельца всплывающего окна
            dialog.ShowDialog();

            // Получите введенный пользователем текст после закрытия окна
            userInput = dialog.UserInput;
            userStatus = dialog.userStatus;
            userIP = dialog.userIP;

            if (userStatus)
            {
                foldere2Info.Text = $"Идентификатор устройства: {userInput};\nСтатус: Доступен.";
            }
            else
            {
                foldere2Info.Text = $"Идентификатор устройства: {userInput};\nСтатус: Недоступен.";
            }
        }

        private void CreateProfileClick(object sender, RoutedEventArgs e)
        {

            if (folder1name != null && userStatus)
            {
                InternetNetwork internetNetwork = new InternetNetwork();
                bool synhroMode = twoSideSynhroButton.IsChecked == true;
                Request result = internetNetwork.SendProfile(userIP, synhroMode);

                if (result != null)
                {
                    MySqlManager myDB = new MySqlManager();
                    string ip = myDB.searchIP_DB(result.uid);

                    SQLiteManager db = new SQLiteManager();
                    DirectoryInfo directoryInfo = new DirectoryInfo(folder1path);
                    db.insertInternetDB(synhroMode, directoryInfo.Name, directoryInfo.FullName, directoryInfo.LastWriteTime, directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length), result.uid, result.profileUID, true);
                    this.Close();
                }
                /*SQLiteManager db = new SQLiteManager();
                DirectoryInfo directoryInfo1 = new DirectoryInfo(folder1path);
                DirectoryInfo directoryInfo2 = new DirectoryInfo(folder2path);
                bool synhroMode = twoSideSynhroButton.IsChecked == true;
                db.insertDB(synhroMode, directoryInfo1.Name, directoryInfo1.FullName, directoryInfo1.LastWriteTime, directoryInfo1.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length),
                    directoryInfo2.Name, directoryInfo2.FullName, directoryInfo2.LastWriteTime, directoryInfo2.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length));
                this.Close();*/
            }
            else
            {
                MessageBox.Show("Ошибка! Не заполнены обязательные настройки профиля!");
            }
        }

        public string[] GetFolderInfo(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            string[] result = new string[2];
            result[0] = directoryInfo.Name;
            result[1] = directoryInfo.FullName;

            return result;
        }
    }
}
