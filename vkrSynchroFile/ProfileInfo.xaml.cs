using System;
using System.Collections.Generic;
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
    public partial class ProfileInfo : Window
    {
        public ProfileInfo(ListItem selectedItem)
        {
            InitializeComponent();
            string[] days = selectedItem.auto_day.Trim().Split(" ");
            string day = string.Join(", ", days) + ";";
            string profInfoText = "";
            string mode = "";

            switch (selectedItem.profType)
            {
                case 1:
                    mode = selectedItem.profMode ? "Двусторонняя синхронизация" : "Односторонняя синхронизация";
                    profInfoText = $"Тип синхронизации: Внутри одного устройства\n" +
                        $"Режим синхронизации: {mode}" +
                        $"\n\n" +
                        $"Первая папка:\n" +
                        $"Имя папки: {selectedItem.folder1name}\n" +
                        $"Полный путь к папке: {selectedItem.folder1path}" +
                        $"\n\n" +
                        $"Вторая папка:\n" +
                        $"Имя папки: {selectedItem.folder2name}\n" +
                        $"Полный путь к папке: {selectedItem.folder2path}" +
                        $"\n\n" +
                        $"Параметры автоматизации:\n" +
                        $"Дни синхронизации: {day}\n" +
                        $"Время синхронизации: {selectedItem.auto_time}";
                    profTitle.Text = $"Параметры профиля №{selectedItem.profile_number}";
                    profInfo.Text = profInfoText;
                    break;
                case 3:
                    mode = selectedItem.profMode ? "Двусторонняя синхронизация" : "Односторонняя синхронизация";
                    profInfoText = $"Тип синхронизации: Между устройствами в локальной сети\n" +
                        $"Режим синхронизации: {mode}\n" +
                        $"Сетевой идентификатор профиля: {selectedItem.profileUID}" +
                        $"\n\n" +
                        $"Папка синхронизации:\n" +
                        $"Имя папки: {selectedItem.folder1name}\n" +
                        $"Полный путь к папке: {selectedItem.folder1path}" +
                        $"\n\n" +
                        $"Второе устройство синхронизации:\n" +
                        $"Идентификатор устройства: {selectedItem.userUID}" +
                        $"\n\n" +
                        $"Параметры автоматизации:\n" +
                        $"Дни синхронизации: {day}\n" +
                        $"Время синхронизации: {selectedItem.auto_time}";
                    profTitle.Text = $"Параметры профиля №{selectedItem.profile_number}";
                    profInfo.Text = profInfoText;
                    break;
            }
        }
    }
}
