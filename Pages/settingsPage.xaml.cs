using Newtonsoft.Json;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для settingsPage.xaml
    /// </summary>
    public partial class settingsPage : Page
    {
        ImageSourceConverter imgs = new ImageSourceConverter();

        public settingsPage()
        {
            InitializeComponent();
        }

        private void mainMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Page main = new Main();
            MainWindow.pageManager.ChangePage(main);
        }

        private void PassIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (textBox.Visibility == System.Windows.Visibility.Visible)
            {
                textBox.Visibility = System.Windows.Visibility.Collapsed;
                passwordBox.Visibility = System.Windows.Visibility.Visible;
                PassIcon.SetValue(Image.SourceProperty, imgs.ConvertFromString("pack://application:,,,/Icons/HiddenPassBtn.png"));
            }
            else
            {
                passwordBox.Visibility = System.Windows.Visibility.Collapsed;
                textBox.Visibility = System.Windows.Visibility.Visible;
                PassIcon.SetValue(Image.SourceProperty, imgs.ConvertFromString("pack://application:,,,/Icons/showPassBtn.png"));
            }
        }

        private void passwordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            textBox.Text = passwordBox.Password;
        }

        private void saveBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filepath = @"\configuration.json";
            string path = pathBox.Text;
            string ip = ipBox.Text;
            string login = loginBox.Text;
            string password = passwordBox.Password;
            JsonConfiguration configuration = new JsonConfiguration(path, ip, login, password);
            string json = JsonConvert.SerializeObject(configuration);
            File.WriteAllText(filepath, json);
        }
    }
}
