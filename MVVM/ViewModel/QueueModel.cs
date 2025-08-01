using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using unloadSchedule.Classes;
using unloadSchedule.MVVM.Model;

namespace unloadSchedule.MVVM.ViewModel
{
    public class QueueModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public event PropertyChangedEventHandler PropertyChanged;

        static string _filePath = AppSettings.QueuePath;

        private ObservableCollection<DataGridRowItem> _items;
        public ICommand DeleteRowCommand { get; }

        public ObservableCollection<DataGridRowItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ICommand SaveRowCommand { get; }
        public ICommand RefreshCommand { get; }

        public QueueModel()
        {
            DeleteRowCommand = new RelayCommand(DeleteRow);
            Items = new ObservableCollection<DataGridRowItem>();
            SaveRowCommand = new RelayCommand(SaveRow, CanSaveRow);
            RefreshCommand = new RelayCommand(_ => LoadDataFromTxt());

            LoadDataFromTxt();
        }

        private void LoadDataFromTxt()
        {
            try
            {
                var lines = File.ReadAllLines(_filePath)
                              .Where(line => line.Contains("|"))
                              .Select(line => line.Trim())
                              .ToList();

                Items.Clear();
                foreach (var line in lines)
                {
                    Items.Add(new DataGridRowItem(line));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        private void SaveRow(object parameter)
        {
            if (parameter is DataGridRowItem item && item.IsEdited)
            {
                try
                {
                    var allLines = Items.Select(x => x.GetEditedLine()).ToArray();
                    File.WriteAllLines(_filePath, allLines);

                    item.IsEdited = false;
                    Logger.Info($"Очередь расписания была изменена");
                    MessageBox.Show("Очередь расписания была изменена", "Очередь расписания", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void DeleteRow(object parameter)
        {
            if (parameter is DataGridRowItem item)
            {
                try
                {
                    Items.Remove(item);
                    var allLines = Items.Select(x => x.GetEditedLine()).ToArray();
                    File.WriteAllLines(_filePath, allLines);
                    Logger.Info($"Расписание было удалено из очереди.");
                    MessageBox.Show("Расписание было удалено из очереди.", "Удаление из очереди", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Расписание: {item} не удалось удалить. Ошибка: {ex}");
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanSaveRow(object parameter) => parameter is DataGridRowItem item && item.IsEdited;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

            public void Execute(object parameter) => _execute(parameter);

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }

    }
}
