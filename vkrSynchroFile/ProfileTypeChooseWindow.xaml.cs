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
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class ProfileTypeChooseWindow : Window
    {
        // Свойство для передачи значения обратно в первое окно
        public int SelectedAction { get; private set; }

        public ProfileTypeChooseWindow()
        {
            InitializeComponent();
            SelectedAction = 0;
        }

        private void Action1_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = 1;
            this.Close();
        }

        private void Action2_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = 2;
            this.Close();
        }
        private void Action3_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = 3;
            this.Close();
        }

        private void Сancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = 0;
            this.Close();
        }
    }
}
