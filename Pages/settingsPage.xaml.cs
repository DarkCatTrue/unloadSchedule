using System.Windows.Controls;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для settingsPage.xaml
    /// </summary>
    public partial class settingsPage : Page
    {
        public settingsPage()
        {
            InitializeComponent();
        }

        private void mainMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Page main = new Main();
            MainWindow.pageManager.ChangePage(main);
        }
    }
}
