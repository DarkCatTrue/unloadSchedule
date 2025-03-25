using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using unloadSchedule.Classes;
using unloadSchedule.MVVM.ViewModel;

public class ViewModel : INotifyPropertyChanged
{
    private CommandHandler _commandHandler;
    public ICommand GotoSettingsCommand => _commandHandler.GotoSettingsCommand;
    public ICommand GotoMainCommand => _commandHandler.GotoMainCommand;
    public ICommand UploadCommand { get; set; }

    private string _currentFile;
    private string _elapsedTime;
    private bool _unloadTomorrow;
    private bool _unloadFull;

    public bool unloadTomorrow
    {
        get => _unloadTomorrow;
        set
        {
            _unloadTomorrow = value;
            OnPropertyChanged();
        }
    }

    public bool unloadFull
    {
        get => _unloadFull;
        set
        {
            _unloadFull = value;
            OnPropertyChanged();
        }
    }

    public string CurrentFile
    {
        get => _currentFile;
        set { _currentFile = value; OnPropertyChanged(); }
    }

    public string ElapsedTime
    {
        get => _elapsedTime;
        set { _elapsedTime = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private DispatcherTimer _timer;
    private int _seconds;

    public ViewModel()
    {
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += TimerTick;
        UploadCommand = new RelayCommand(async () => await CheckRadioButton());
        _commandHandler = new CommandHandler();
    }

    public async Task StartDefUploadAsync()
    {
        FtpUnload ftpUnload = new FtpUnload();
        JsonHandler jsonHandler = new JsonHandler();
        MessageBoxResult result = MessageBox.Show("Хотите ли вы начать выгрузку заново?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
        
        switch (result)
        {
            case MessageBoxResult.Yes:
                ftpUnload.SaveCurrentJson("ba.htm");
                ftpUnload.OnFileUploaded += fileName => CurrentFile = fileName;
                _seconds = 0;
                _timer.Start();

                await ftpUnload.StartDefaultUpload();

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.No:
                
                ftpUnload.OnFileUploaded += fileName => CurrentFile = fileName;
                _seconds = 0;
                _timer.Start();
                await ftpUnload.StartDefaultUpload();

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.Cancel:
                return;
        }
    }
    public async Task StartTomUpload()
    {
        FtpUnload ftpUnload = new FtpUnload();
        JsonHandler jsonHandler = new JsonHandler();
        MessageBoxResult result = MessageBox.Show("Хотите ли вы начать выгрузку заново?", "Подтверждение", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

        switch (result)
        {
            case MessageBoxResult.Yes:
                ftpUnload.SaveCurrentJson("cg.htm");
                ftpUnload.OnFileUploaded += fileName => CurrentFile = fileName;
                _seconds = 0;
                _timer.Start();

                await ftpUnload.StartTommorowUpload();

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.No:

                ftpUnload.OnFileUploaded += fileName => CurrentFile = fileName;
                _seconds = 0;
                _timer.Start();
                await ftpUnload.StartTommorowUpload();

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
            await StartTomUpload();
        }
        else if (unloadFull)
        {
            await StartDefUploadAsync();
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
