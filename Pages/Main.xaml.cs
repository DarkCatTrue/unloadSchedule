using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using unloadSchedule.Classes;
using static MaterialDesignThemes.Wpf.Theme;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        string filepath = @"Jsons\configuration.json";
        FtpUnload ftpUnload = new FtpUnload();
        public Main()
        {
            InitializeComponent();
        }

        private async void unloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string jsonFile = File.ReadAllText(filepath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string path = json.SchedulePath;
            string ip = json.Ip;
            string login = json.Login;
            string password = json.Password;
            await ftpUnload.DefaultUnload(path, ip, login, password, "*.htm");
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
                unloadList.Visibility = Visibility.Visible;
                FirstList.Visibility = Visibility.Visible;
                SecondList.Visibility = Visibility.Visible;
                ThirdList.Visibility = Visibility.Visible;
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
                unloadList.Visibility = Visibility.Collapsed;
                FirstList.Visibility = Visibility.Collapsed;
                SecondList.Visibility = Visibility.Collapsed;
                ThirdList.Visibility = Visibility.Collapsed;
            }
        }
    }
}
