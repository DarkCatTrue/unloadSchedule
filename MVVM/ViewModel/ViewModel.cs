using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using NLog;
using System;
using System.Globalization;
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
using unloadSchedule.MVVM.View;
using ICSharpCode.SharpZipLib.Zip;

public class ViewModel : INotifyPropertyChanged
{
    FtpUnload ftpUnload = new FtpUnload();
    JsonHandler jsonHandler = new JsonHandler();
    public static string QueuePath = AppSettings.QueuePath;

    Queue queue = new Queue(QueuePath);

    public string ConfigPath = AppSettings.ConfigPath;
    public string AllUnldPath = AppSettings.AllUnldPath;
    public string OneDayUnldPath = AppSettings.OneDayUnldPath;
    public string ScheduleFolders = AppSettings.ScheduleFolders;

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private CommandHandler _commandHandler;
    private DispatcherTimer _timer;
    private int _seconds;
    public ICommand GotoSettingsCommand => _commandHandler.GotoSettingsCommand;
    public ICommand UploadCommand { get; set; }

    public ICommand ScheduleIsChecked { get; }

    public ICommand queueCommand { get; set; }

    public ICommand EditQueue { get; set; }

    private string _currentFile;

    private string _elapsedTime;

    private bool _unloadOneDay;

    private bool _unloadFull;

    private bool _progressBarVisibility;

    private double _uploadProgress;

    private string _uploadPercentage;

    private string _timeReservText;

    private DateTime? _datePickerText;

    private string _scheduleName;

    private string _firstListText;

    private string _secondListText;

    private string _thirdListText;

    private bool _timeReservIsEnabled;

    private bool _datePickerIsEnabled;

    private bool _unloadListVisibility;

    private bool _firstListVisibility;

    private bool _secondListVisbility;

    private bool _thirdListVisibility;

    private bool _isScheduled;

    private bool _queueUnloadVisibility;

    public bool ProgressBarVisibility
    {
        get => _progressBarVisibility; set { _progressBarVisibility = value; OnPropertyChanged(); }
    }
    public bool queueUnloadVisibility
    {
        get => _queueUnloadVisibility; set { _queueUnloadVisibility = value; OnPropertyChanged(); }
    }
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


    public string FirstListText
    {
        get => _firstListText; set { _firstListText = value; OnPropertyChanged(); }
    }

    public string SecondListText
    {
        get => _secondListText; set { _secondListText = value; OnPropertyChanged(); }
    }

    public string ThirdListText
    {
        get => _thirdListText; set { _thirdListText = value; OnPropertyChanged(); }
    }

    public string ScheduleName
    {
        get => _scheduleName; set { _scheduleName = value; OnPropertyChanged(); }
    }

    public DateTime? DatePickerText
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
        queueCommand = new RelayCommand(async () => await planAdd());
        _commandHandler = new CommandHandler();
        EditQueue = new RelayCommand(OpenQueue);
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
    public void planScheduleUnload()
    {
        string[] DateInfo = queue.GetTimerInfo();
        string date = DateInfo[1];
        string time = DateInfo[2];
        DateTime datePart = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        TimeSpan timePart = TimeSpan.ParseExact(time, "h\\:mm", CultureInfo.InvariantCulture);
        DateTime FullDate = datePart.Add(timePart);
        TimerToRunSchedule timer = new TimerToRunSchedule();
        timer.Timer(queue.ReturnCopySchedule(), StartUpload: StartPlanUnload, targetTime: FullDate);
    }
    public void OpenQueue()
    {
        QueueWindow queueWindow = new QueueWindow();
        queueWindow.Show();
    }
    private async Task planAdd()
    {
        ParseSchedule parseSchedule = new ParseSchedule();
        DateTime Date = Convert.ToDateTime(DatePickerText);
        string DateText = Date.ToString("yyyy-MM-dd");
        string TimeText = TimeReservText;
        if (queue.CheckDate(DateText, TimeText))
        {
            string queueFolder = $"ScheduleFolders\\{DateText}";
            Directory.CreateDirectory(queueFolder);
            try
            {
                string jsonFile = File.ReadAllText(ConfigPath);
                dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
                string filepath = json.SchedulePath;
                string dateSchedule = await parseSchedule.ParseScheduleDay(filepath);
                queue.CopySchedulePlan(filepath, queueFolder);
                await queue.AddEntryAsync(dateSchedule, DateText, TimeText);
                await queue.UpdateFileWithSortedEntriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось определить дату расписания, проверьте директорию с расписанием.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка при определении даты расписания, для дальнейшей планировки выгрузки: {ex}");
            }
        }
    }
    private void ScheduleUnload_checked()
    {
        TimeReservIsEnabled = true;
        DatePickerIsEnabled = true;
        UnloadListVisibility = true;
        FirstListVisibility = true;
        SecondListVisbility = true;
        ThirdListVisibility = true;
        queueUnloadVisibility = true;
    }

    private void ScheduleUnload_unchecked()
    {
        DatePickerText = null;
        TimeReservIsEnabled = false;
        DatePickerIsEnabled = false;
        TimeReservText = string.Empty;
        UnloadListVisibility = false;
        FirstListVisibility = false;
        SecondListVisbility = false;
        ThirdListVisibility = false;
        queueUnloadVisibility = false;
    }
    public async Task StartPlanUnload()
    {
        bool OnedayUnload;

        Action<string> fileHandler = fileName => CurrentFile = fileName;
        Action<double> progressHandler = progress => UploadProgress = progress;

        ftpUnload.DefaultFile += fileHandler;
        ftpUnload.DefaultProgress += progressHandler;
        _seconds = 0;

        _timer.Start();

        jsonHandler.SaveJson<AllUnload>("ba.htm", 0, AllUnldPath, fileHandler, progressHandler);
        await ftpUnload.Unload(OnedayUnload = false);

        _timer.Stop();

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
        ProgressBarVisibility = true;
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
