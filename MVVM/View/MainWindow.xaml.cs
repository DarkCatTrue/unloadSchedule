using System.IO;
using System.Windows;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string pathConfig = "Jsons";

        static string configJson = @"Jsons\configuration.json";

        static string currentTaskJson = @"Jsons\currentTask.json";

        static string ScheduleFolders = "ScheduleFolders";

        string firstScheduleFolder = $"{ScheduleFolders}\\First";

        string secondScheduleFolder = $"{ScheduleFolders}\\Second";

        string thirdScheduleFolder = $"{ScheduleFolders}\\Third";

        static public PageManager pageManager;
        public MainWindow()
        {
            InitializeComponent();
            InitializeFolders();
            pageManager = new PageManager(mainFrame);
        }

        public void InitializeFolders()
        {
            if (!Directory.Exists(pathConfig))
                Directory.CreateDirectory(pathConfig);

            if (!File.Exists(configJson))
                File.Create(configJson);

            if (!File.Exists(currentTaskJson))
                File.Create(currentTaskJson);

            if (!Directory.Exists(ScheduleFolders))
                Directory.CreateDirectory(ScheduleFolders);

            if (!Directory.Exists(firstScheduleFolder))
                Directory.CreateDirectory(firstScheduleFolder);

            if (!Directory.Exists(secondScheduleFolder))
                Directory.CreateDirectory(secondScheduleFolder);

            if (!Directory.Exists(thirdScheduleFolder))
                Directory.CreateDirectory(thirdScheduleFolder);
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
