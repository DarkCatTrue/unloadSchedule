using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using unloadSchedule;
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
        UploadCommand = new RelayCommand(async () => await StartUploadAsync());
        _commandHandler = new CommandHandler();
    }

    public async Task StartUploadAsync()
    {
        _seconds = 0;
        _timer.Start();

        FtpUnload ftpUnload = new FtpUnload();
        ftpUnload.OnFileUploaded += fileName => CurrentFile = fileName;
        await ftpUnload.StartDefaultUpload();

        _timer.Stop();
        MessageBox.Show("Выгрузка успешно закончена", "Окончание выгрузки", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void TimerTick(object sender, EventArgs e)
    {
        _seconds++;
        int minutes = _seconds / 60;
        int seconds = _seconds % 60;
        ElapsedTime = $"Прошло времени: {minutes:D2}:{seconds:D2}";
    }
}
