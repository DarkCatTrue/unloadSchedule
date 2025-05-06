using System.IO;
using System.Windows;
using unloadSchedule.Classes;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public PageManager pageManager;
        public MainWindow()
        {
            InitializeComponent();
            AppSettings appSettings = new AppSettings();
            appSettings.InitializeFolders();
            pageManager = new PageManager(mainFrame);
        }

        private void colapseBtn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void closeBtn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }

        private void ToolBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
