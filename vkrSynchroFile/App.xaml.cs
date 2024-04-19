using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Forms = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace vkrSynchroFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private readonly Forms.NotifyIcon _notifyIcon;
        public App()
        {
            _notifyIcon = new Forms.NotifyIcon();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            MainWindow = new MainWindow();
            MainWindow.Show();

            _notifyIcon.Icon = new System.Drawing.Icon("Resources/icon.ico");
            _notifyIcon.Text = "SynhroFile";
            _notifyIcon.DoubleClick += NotifyIcon_Click;

            // Создание меню
            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Скрыть", Image.FromFile("Resources/icon.ico"), OnHideClicked);
            _notifyIcon.ContextMenuStrip.Items.Add("Закрыть", Image.FromFile("Resources/icon.ico"), OnCloseClicked);

            _notifyIcon.Visible = true;

            base.OnStartup(e);
        }

        private void OnHideClicked(object sender, EventArgs e)
        {
            MainWindow.Hide();
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();

            base.OnExit(e);
        }
    }

}
