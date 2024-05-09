using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;

namespace vkrSynchroFile
{
    public partial class Internet_SelectSecondFolder : Window
    {

        public Internet_SelectSecondFolder()
        {
            InitializeComponent();
            acceptProfile = false;
        }


        private string foldername;
        public string folderpath { get; private set; }

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


        public string uniqueId { get; private set; }
        public bool acceptProfile { get; private set; }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            if (foldername != null)
            {
                uniqueId = Guid.NewGuid().ToString();
                acceptProfile = true;

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
    }
}
