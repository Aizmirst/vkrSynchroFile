using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vkrSynchroFile
{
    /// <summary>
    /// Логика взаимодействия для Internet_SelectSecondFolder.xaml
    /// </summary>
    public partial class Internet_SelectSecondFolder : Window
    {

        private string uid;
        public Internet_SelectSecondFolder(string uid)
        {
            InitializeComponent();
            this.uid = uid;
            TitleTextBlock.Text = $"Получен запрос на создание профиля от пользователя: \n {uid}.";
        }


        private string foldername;
        private string folderpath;
        private void FolderSelectClick(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            folderBrowserDialog.Description = "Выберите папку";

            if (folderBrowserDialog.ShowDialog(this).GetValueOrDefault())
            {
                string selectedFolder = folderBrowserDialog.SelectedPath;

                if (Directory.Exists(selectedFolder))
                {
                    string[] folderInfo = InternetProfileMethods.GetFolderInfo(selectedFolder);
                    foldername = folderInfo[0];
                    folderpath = folderInfo[1];
                    foldereInfo.Text = $"Имя папки: {folderInfo[0]};\nПолный путь: {folderInfo[1]}.";
                }
            }
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            if (foldername != null)
            {
                MySqlManager myDB = new MySqlManager();
                if (myDB.searchDB(uid))
                {
                    InternetNetwork internetNetwork = new InternetNetwork();
                    string ip = myDB.searchIP_DB(uid);
                    string uniqueId = Guid.NewGuid().ToString();
                    internetNetwork.AcceptProfile(ip, uid, uniqueId);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Проблемы с подключением к БД. Повторите попытку!");
                }

                /*SQLiteManager db = new SQLiteManager();
                DirectoryInfo directoryInfo1 = new DirectoryInfo(folder1path);
                DirectoryInfo directoryInfo2 = new DirectoryInfo(folder2path);
                bool synhroMode = twoSideSynhroButton.IsChecked == true;
                db.insertDB(synhroMode, directoryInfo1.Name, directoryInfo1.FullName, directoryInfo1.LastWriteTime, directoryInfo1.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length),
                    directoryInfo2.Name, directoryInfo2.FullName, directoryInfo2.LastWriteTime, directoryInfo2.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length));*/
            }
            else
            {
                MessageBox.Show("Ошибка!");
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
