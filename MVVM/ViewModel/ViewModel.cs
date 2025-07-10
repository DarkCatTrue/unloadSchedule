using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    public ICommand UploadCommand { get; set; }

    public ICommand ScheduleIsChecked { get; }

    private string _currentFile;

    private string _elapsedTime;

    private bool _unloadOneDay;

    private bool _unloadFull;

    private double _uploadProgress;

    private string _uploadPercentage;

    private string _timeReservText;

    private string _datePickerText;

    private string _scheduleName;

    private bool _timeReservIsEnabled;

    private bool _datePickerIsEnabled;

    private bool _unloadListVisibility;

    private bool _firstListVisibility;

    private bool _secondListVisbility;

    private bool _thirdListVisibility;

    private bool _isScheduled;
    public bool IsScheduled
    {
        get => _isScheduled;
        set
        {
            if (_isScheduled != value)
            {
                _isScheduled = value;
                OnPropertyChanged();
            }
        }
    }

    public string ScheduleName
    {
        get => _scheduleName; set { _scheduleName = value; OnPropertyChanged(); }
    }

    public string DatePickerText
    {
        get => _datePickerText; set { _datePickerText = value; OnPropertyChanged(); }
    }

    public string TimeReservText
    {
        get => _timeReservText; set { _timeReservText = value; OnPropertyChanged(); }
    }

    public bool FirstListVisibility
    {
        get => _firstListVisibility; set { _firstListVisibility = value; OnPropertyChanged(); }
    }

    public bool SecondListVisbility
    {
        get => _secondListVisbility; set { _secondListVisbility = value; OnPropertyChanged(); }
    }

    public bool ThirdListVisibility
    {
        get => _thirdListVisibility; set { _thirdListVisibility = value; OnPropertyChanged(); }
    }


    public bool UnloadListVisibility
    {
        get => _unloadListVisibility; set { _unloadListVisibility = value; OnPropertyChanged(); }
    }

    public bool DatePickerIsEnabled
    {
        get => _datePickerIsEnabled; set { _datePickerIsEnabled = value; OnPropertyChanged(); }
    }

    public bool TimeReservIsEnabled
    {
        get => _timeReservIsEnabled; set { _timeReservIsEnabled = value; OnPropertyChanged(); }
    }

    public double UploadProgress
    {
        get => _uploadProgress; set { _uploadProgress = value; OnPropertyChanged(); UploadPercentage = $"{UploadProgress:F0}%"; }
    }
    public string UploadPercentage
    {
        get => _uploadPercentage; set { _uploadPercentage = value; OnPropertyChanged(); }
    }
    public bool unloadOneDay
    {
        get => _unloadOneDay; set { _unloadOneDay = value; OnPropertyChanged(); }
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
        LoadScheduleDate();
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += TimerTick;
        UploadCommand = new RelayCommand(async () => await CheckRadioButton());
        _commandHandler = new CommandHandler();
        ScheduleIsChecked = new RelayCommand(() =>
        {
            if (IsScheduled)
            {
                ScheduleUnload_checked();
            }
            else
            {
                ScheduleUnload_unchecked();
            }
        });
    }

    private void ScheduleUnload_checked()
    {
        TimeReservIsEnabled = true;
        DatePickerIsEnabled = true;
        UnloadListVisibility = true;
        FirstListVisibility = true;
        SecondListVisbility = true;
        ThirdListVisibility = true;
    }

    private void ScheduleUnload_unchecked()
    {
        TimeReservIsEnabled = false;
        DatePickerIsEnabled = false;
        TimeReservText = string.Empty;
        DatePickerText = string.Empty;
        UnloadListVisibility = false;
        FirstListVisibility = false;
        SecondListVisbility = false;
        ThirdListVisibility = false;
    }


    public async Task StartDefaultUnload()
    {
        bool OnedayUnload;

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
                await ftpUnload.Unload(OnedayUnload = false);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.No:

                _timer.Start();

                await ftpUnload.Unload(OnedayUnload = false);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.Cancel:
                return;
        }
    }
    public async Task StartOneDayUnload()
    {
        bool OneDayUnload;

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
                await ftpUnload.Unload(OneDayUnload = true);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.No:

                _timer.Start();

                await ftpUnload.Unload(OneDayUnload = true);

                _timer.Stop();
                MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            case MessageBoxResult.Cancel:
                return;
        }
    }

    private async Task LoadScheduleDate()
    {
        ParseSchedule parseSchedule = new ParseSchedule();
        try
        {
            string jsonFile = File.ReadAllText(ConfigPath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string filepath = json.SchedulePath;
            ScheduleName = "Выгрузка на " + await parseSchedule.ParseScheduleDay(filepath);
        }
        catch
        { }
    }

    public async Task CheckRadioButton()
    {
        if (unloadOneDay)
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
