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
    /// Логика взаимодействия для Internet_ChangeProfile.xaml
    /// </summary>
    public partial class Internet_ChangeProfile : Window
    {
        private bool changeProfile;
        ListItem item;

        public Internet_ChangeProfile(ListItem selectedItem)
        {
            InitializeComponent();
            item = selectedItem;
            changeProfile = false;
            foldereInfo.Text = $"Имя папки: {item.folder1name};\nПолный путь: {item.folder1path}.";
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
                    changeProfile = true;
                }
            }
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            if (changeProfile)
            {
                // Был изменение - обновить таблицу
                SQLiteManager db = new SQLiteManager();
                db.updateDB_Internet(item.folder1id, foldername, folderpath);
                this.Close();
            }
            else
            {
                // Изменений нет, просто закрыть окно
                this.Close();
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
