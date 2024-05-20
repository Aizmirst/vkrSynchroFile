using Ookii.Dialogs.Wpf;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace vkrSynchroFile
{
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
            
            // загрузка дней
            lowerRadioButton.IsChecked = selectedItem.auto_type;
            string[] days = selectedItem.auto_time.Trim().Split(" ");
            if (!selectedItem.auto_type)
            {
                foreach (var button in daysButtonsPanel.Children.OfType<ToggleButton>())
                {
                    if (days.Contains(button.Content.ToString()))
                    {
                        button.IsChecked = true;
                    }
                    else
                    {
                        button.IsChecked = false;
                    }
                }
            }
            else
            {
                calendar.SelectedDates.Clear();
                foreach (var date in days)
                {
                    if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        calendar.SelectedDates.Add(parsedDate);
                    }
                }
            }

            // Загрузка времени
            string[] time = selectedItem.auto_time.Trim().Split(":");
            string selectedHour = time[0];
            string selectedMinute = time[1];
            foreach (ComboBoxItem item in hoursComboBox.Items)
            {
                if (item.Content.ToString() == selectedHour)
                {
                    hoursComboBox.SelectedItem = item;
                }
            }
            foreach (ComboBoxItem item in minutesComboBox.Items)
            {
                if (item.Content.ToString() == selectedMinute)
                {
                    minutesComboBox.SelectedItem = item;
                }
            }
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

        private void ChangeProfileClick(object sender, RoutedEventArgs e)
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
                db.updateDB_PC(profileID, synhroMode,
                    folder1ID, directoryInfo1.Name, directoryInfo1.FullName,
                    folder2ID, directoryInfo2.Name, directoryInfo2.FullName, autoType, autoDay, autoTime);
                this.Close();
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
