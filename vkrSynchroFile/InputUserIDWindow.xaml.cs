using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
    public partial class InputUserIDWindow : Window
    {
        public string UserInput { get; private set; }
        public bool userStatus { get; private set; }
        public string userIP { get; private set; }

        public InputUserIDWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            UserInput = InputTextBox.Text;
            if (UserInput.Length != 36)
            {
                MessageBox.Show("Ошибка в идентификаторе!");
            }
            else
            {
                MySqlManager myDB = new MySqlManager();
                if (myDB.searchDB(UserInput))
                {
                    userIP = myDB.searchIP_DB(UserInput);
                    InternetNetwork internetNetwork = new InternetNetwork();
                    userStatus = internetNetwork.PingDevice(userIP);
                    Close();
                }
                else
                {
                    MessageBox.Show("Вы ввели идентификатор не верно, либо такого устройсва не существует!");
                }
            }
        }

        private void MaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Получаем текущий текст из TextBox
            var textBox = (TextBox)sender;
            string text = textBox.Text;

            // Допустимые символы для маски
            string allowedChars = "0123456789abcdef-";

            // Проверяем, является ли вводимый символ допустимым
            if (!allowedChars.Contains(e.Text.ToLower()))
            {
                // Отменяем ввод символа, если он недопустимый
                e.Handled = true;
                return;
            }

            // Добавляем разделители в нужных местах (каждый 8, 13, 18 и 23 символ)
            if (text.Length == 8 || text.Length == 13 || text.Length == 18 || text.Length == 23)
            {
                textBox.Text += "-";
                textBox.CaretIndex = textBox.Text.Length; // Перемещаем курсор в конец TextBox
            }
        }

    }
}
