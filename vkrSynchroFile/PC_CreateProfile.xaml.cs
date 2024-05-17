using Ookii.Dialogs.Wpf;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace vkrSynchroFile
{
    public partial class PC_CreateProfile : Window
    {
        public PC_CreateProfile()
        {
            InitializeComponent();
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
                bool autoType = false;
                string autoDay = "";
                if (centerRadioButton.IsChecked == true)
                {
                    autoType = false;
                    foreach (ToggleButton button in daysButtonsPanel.Children)
                    {
                        if (button.IsChecked == true)
                        {
                            autoDay += button.Content.ToString() + " ";
                        }
                    }
                }
                else
                {
                    autoType = true;
                    foreach (var date in calendar.SelectedDates)
                    {
                        autoDay += ((DateTime)date).ToString("dd/MM/yyyy") + " ";
                    }
                }
                string selectedHour = (hoursComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string selectedMinute = (minutesComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string autoTime = selectedHour + ":" + selectedMinute;
                db.insertDB(synhroMode, directoryInfo1.Name, directoryInfo1.FullName,
                directoryInfo2.Name, directoryInfo2.FullName, autoType, autoDay, autoTime);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка! Не выбраны папки для синхронизации!");
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
