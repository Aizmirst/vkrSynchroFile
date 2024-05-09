using System.Windows;

namespace vkrSynchroFile
{
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
