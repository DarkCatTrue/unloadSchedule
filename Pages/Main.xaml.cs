using System.Windows;
using System.Windows.Controls;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
           Page settings = new settingsPage();
           MainWindow.pageManager.ChangePage(settings);
        }

        private void scheduledUnload_Checked(object sender, RoutedEventArgs e)
        {
            if (scheduledUnload.IsChecked == true)
            {
                timeReserv.IsEnabled = true;
                datePicker.IsEnabled = true;
            }

        }

        private void scheduledUnload_Unchecked(object sender, RoutedEventArgs e)
        {
            if (scheduledUnload.IsChecked == false)
            {
                timeReserv.IsEnabled = false;
                datePicker.IsEnabled = false;
                timeReserv.Text = string.Empty;
                datePicker.Text = string.Empty;
            }
        }
    }
}
