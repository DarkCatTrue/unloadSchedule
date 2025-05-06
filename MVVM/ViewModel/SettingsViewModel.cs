using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using unloadSchedule.Classes;

namespace unloadSchedule.MVVM.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        string configPath = AppSettings.ConfigPath;

        public bool _isPasswordVisible;
        public bool _isTextVisible;

        private string pathBox_;
        private string ipBox_;
        private string loginBox_;
        private string passwordBox_;
        private string textBox_;

        public ImageSource _eyeIcon;

        private CommandHandler _commandHandler;
        public ICommand GotoMainCommand => _commandHandler.GotoMainCommand;
        public ICommand SaveConfig { get; set; }
        public ICommand TogglePasswordCommand { get; }
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible; set { _isPasswordVisible = value; OnPropertyChanged(); UpdateEyeIcon(); }
        }
        public bool IsTextVisible
        {
            get => _isTextVisible; set { _isTextVisible = value; OnPropertyChanged(); UpdateEyeIcon(); }
        }

        public string PathBox
        {
            get => pathBox_; set { pathBox_ = value; OnPropertyChanged(); }
        }

        public string TextBox
        {
            get => textBox_; set { textBox_ = value; OnPropertyChanged(); }
        }

        public ImageSource EyeIcon
        {
            get => _eyeIcon; set { _eyeIcon = value; OnPropertyChanged(); }
        }
        public string IpBox
        {
            get => ipBox_; set { ipBox_ = value; OnPropertyChanged(); }
        }

        public string LoginBox
        {
            get => loginBox_; set { loginBox_ = value; OnPropertyChanged(); }
        }
        public string PasswordBox
        {
            get => passwordBox_; set { passwordBox_ = value; OnPropertyChanged(); }
        }

        public void ReaderConfiguration()
        {
            string jsonFile = File.ReadAllText(configPath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            try
            {
                PathBox = json.SchedulePath;
                IpBox = json.Ip;
                LoginBox = json.Login;
                PasswordBox = json.Password;
            }
            catch { }
        }

        private void SaveConfiguration()
        {
            string path = PathBox;
            path = path.Replace(@"""", "");
            string ip = IpBox;
            string login = LoginBox;
            string password = PasswordBox;
            JsonConfiguration jsonConfiguration = new JsonConfiguration(path, ip, login, password);
            try
            {
                jsonConfiguration.SaveConfiguration(path, ip, login, password);
            }
            catch { MessageBox.Show("Все поля должны быть заполнены.", "Сохранение конфигурации", MessageBoxButton.OK, MessageBoxImage.Error); }

            MessageBox.Show("Вы успешно сохранили конфигурацию.", "Сохранение конфигурации", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
            if (IsPasswordVisible) 
            {
                IsTextVisible = false;
                TextBox = PasswordBox;
            }
            else
            {
                IsTextVisible = true;
                TextBox = PasswordBox;
            }
        }

        private void UpdateEyeIcon()
        {
            var iconPath = IsPasswordVisible ?
                "pack://application:,,,/Icons/HiddenPassBtn.png" :
                "pack://application:,,,/Icons/showPassBtn.png";
            EyeIcon = new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute));
        }
        public SettingsViewModel()
        {
            _commandHandler = new CommandHandler();
            ReaderConfiguration();
            UpdateEyeIcon();
            IsPasswordVisible = true;
            SaveConfig = new RelayCommand(SaveConfiguration);
            TogglePasswordCommand = new RelayCommand(TogglePasswordVisibility);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}