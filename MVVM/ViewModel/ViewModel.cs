using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using unloadSchedule.Classes;
using unloadSchedule.MVVM.Model;
using unloadSchedule.MVVM.ViewModel;

public class ViewModel : INotifyPropertyChanged
{
    FtpUnload ftpUnload = new FtpUnload();
    JsonHandler jsonHandler = new JsonHandler();


    public string ConfigPath = AppSettings.ConfigPath;
    public string AllUnldPath = AppSettings.AllUnldPath;
    public string OneDayUnldPath = AppSettings.OneDayUnldPath;

    private CommandHandler _commandHandler;
    private DispatcherTimer _timer;
    private int _seconds;
    public ICommand GotoSettingsCommand => _commandHandler.GotoSettingsCommand;
    public ICommand GotoMainCommand => _commandHandler.GotoMainCommand;
    public ICommand UploadCommand { get; set; }

    private string _currentFile;

    private string _elapsedTime;

    private bool _unloadTomorrow;

    private bool _unloadFull;

    private double _uploadProgress;

    private string _uploadPercentage;

    public double UploadProgress
    {
        get => _uploadProgress; set { _uploadProgress = value; OnPropertyChanged(); UploadPercentage = $"{UploadProgress:F0}%"; }
    }
    public string UploadPercentage
    {
        get => _uploadPercentage; set { _uploadPercentage = value; OnPropertyChanged(); }
    }
    public bool unloadTomorrow
    {
        get => _unloadTomorrow; set { _unloadTomorrow = value; OnPropertyChanged(); }
    }

    public bool unloadFull
    {
        get => _unloadFull; set { _unloadFull = value; OnPropertyChanged(); }
    }

    public string CurrentFile
    {
        get => _currentFile; set { _currentFile = value; OnPropertyChanged(); }
    }

    public string ElapsedTime
    {
        get => _elapsedTime; set { _elapsedTime = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ViewModel()
    {
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += TimerTick;
        UploadCommand = new RelayCommand(async () => await CheckRadioButton());
        _commandHandler = new CommandHandler();
    }

    public async Task StartDefaultUnload()
    {
        Action<string> fileHandler = fileName => CurrentFile = fileName;
        Action<double> progressHandler = progress => UploadProgress = progress;

        ftpUnload.DefaultFile += fileHandler;
        ftpUnload.DefaultProgress += progressHandler;
        _seconds = 0;

        MessageBoxResult result = MessageBox.Show("Хотите ли вы начать выгрузку заново?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

        switch (result)
        {
            case MessageBoxResult.Yes:

                _timer.Start();

                jsonHandler.SaveJson<AllUnload>("ba.htm", 0, AllUnldPath, fileHandler, progressHandler);
                await ftpUnload.OneDayUnload(false);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.No:

                _timer.Start();

                await ftpUnload.OneDayUnload(false);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.Cancel:
                return;
        }
    }
    public async Task StartOneDayUnload()
    {
        Action<string> fileHandler = fileName => CurrentFile = fileName;
        Action<double> progressHandler = progress => UploadProgress = progress;

        ftpUnload.OneDayFile += fileName => CurrentFile = fileName;
        ftpUnload.OneDayProgress += progress => UploadProgress = progress;
        _seconds = 0;
        MessageBoxResult result = MessageBox.Show("Хотите ли вы начать выгрузку заново?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

        switch (result)
        {
            case MessageBoxResult.Yes:

                _timer.Start();

                jsonHandler.SaveJson<OneDayUnload>("ca.htm", 0, OneDayUnldPath, fileHandler, progressHandler);
                await ftpUnload.OneDayUnload(true);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.No:

                _timer.Start();

                await ftpUnload.OneDayUnload(true);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.Cancel:
                return;
        }
    }

    public async Task CheckRadioButton()
    {
        if (unloadTomorrow)
        {
            await StartOneDayUnload();
        }
        else if (unloadFull)
        {
            await StartDefaultUnload();
        }
    }

    private void TimerTick(object sender, EventArgs e)
    {
        _seconds++;
        int minutes = _seconds / 60;
        int seconds = _seconds % 60;
        ElapsedTime = $"Прошло времени: {minutes:D2}:{seconds:D2}";
    }
}
