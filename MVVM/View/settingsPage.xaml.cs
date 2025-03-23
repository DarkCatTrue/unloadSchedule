using Newtonsoft.Json;
using System.IO;
using System.Windows;
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
        string filepath = @"Jsons\configuration.json";
        public settingsPage()
        {
            InitializeComponent();
            ReaderConfiguration();
            DataContext = new ViewModel();
        }

        public void ReaderConfiguration()
        {
            string jsonFile = File.ReadAllText(filepath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            try
            {
                pathBox.Text = json.SchedulePath;
                ipBox.Text = json.Ip;
                loginBox.Text = json.Login;
                passwordBox.Password = json.Password;
            }
            catch { }
        }


        private void PassIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (textBox.Visibility == Visibility.Visible)
            {
                textBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                PassIcon.SetValue(Image.SourceProperty, imgs.ConvertFromString("pack://application:,,,/Icons/HiddenPassBtn.png"));
            }
            else
            {
                passwordBox.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
                PassIcon.SetValue(Image.SourceProperty, imgs.ConvertFromString("pack://application:,,,/Icons/showPassBtn.png"));
            }
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            textBox.Text = passwordBox.Password;
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = pathBox.Text;
            path = path.Replace(@"""", "");
            string ip = ipBox.Text;
            string login = loginBox.Text;
            string password = passwordBox.Password;
            JsonConfiguration jsonConfiguration = new JsonConfiguration(path, ip, login, password);
            try
            {
                jsonConfiguration.SaveConfiguration(path, ip, login, password);
            }
            catch { MessageBox.Show("Все поля должны быть заполнены.", "Сохранение конфигурации", MessageBoxButton.OK, MessageBoxImage.Error); }

            MessageBox.Show("Вы успешно сохранили конфигурацию.", "Сохранение конфигурации", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}