using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace vkrSynchroFile
{
    /// <summary>
    /// Логика взаимодействия для PC_CreateProfile.xaml
    /// </summary>
    public partial class PC_ChangeProfile : Window
    {

        private int profileID;
        private int folder1ID;
        private int folder2ID;
        public PC_ChangeProfile(ListItem selectedItem)
        {
            InitializeComponent();
            profileID = selectedItem.profile_id;
            if (selectedItem.profMode)
            {
                oneSideSynhroButton.IsChecked = false;
                twoSideSynhroButton.IsChecked = true;
            }
            else
            {
                oneSideSynhroButton.IsChecked = true;
                twoSideSynhroButton.IsChecked = false;
            }
            folder1ID = selectedItem.folder1id;
            folder1name = selectedItem.folder1name;
            folder1path = selectedItem.folder1path;
            folder2ID = selectedItem.folder2id;
            folder2name = selectedItem.folder2name;
            folder2path = selectedItem.folder2path;
            foldere1Info.Text = $"Имя папки: {selectedItem.folder1name};\nПолный путь: {selectedItem.folder1path}.";
            foldere2Info.Text = $"Имя папки: {selectedItem.folder2name};\nПолный путь: {selectedItem.folder2path}.";
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
        private void FolderSelect1Click(object sender, RoutedEventArgs e)
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


        private string folder2name;
        private string folder2path;
        private void FolderSelect2Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            folderBrowserDialog.Description = "Выберите папку";

            if (folderBrowserDialog.ShowDialog(this).GetValueOrDefault())
            {
                string selectedFolder = folderBrowserDialog.SelectedPath;

                if (Directory.Exists(selectedFolder))
                {
                    string[] folderInfo = GetFolderInfo(selectedFolder);
                    folder2name = folderInfo[0];
                    folder2path = folderInfo[1];
                    foldere2Info.Text = $"Имя папки: {folderInfo[0]};\nПолный путь: {folderInfo[1]}.";
                }
            }
        }

        private void CreateProfileClick(object sender, RoutedEventArgs e)
        {

            if (folder1name != null && folder2name != null)
            {
                SQLiteManager db = new SQLiteManager();
                DirectoryInfo directoryInfo1 = new DirectoryInfo(folder1path);
                DirectoryInfo directoryInfo2 = new DirectoryInfo(folder2path);
                bool synhroMode = twoSideSynhroButton.IsChecked == true;
                db.updateDB_PC(profileID, synhroMode,
                    folder1ID, directoryInfo1.Name, directoryInfo1.FullName,
                    folder2ID, directoryInfo2.Name, directoryInfo2.FullName);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка!");
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
