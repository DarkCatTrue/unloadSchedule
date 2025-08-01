using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using unloadSchedule.Classes;

namespace unloadSchedule.MVVM.Model
{
    public class Queue
    {
        private string _filePath = AppSettings.QueuePath;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public Queue(string filePath)
        {
            _filePath = filePath;
        }

        public async Task AddEntryAsync(string name, string date, string time)
        {
            string entry = $"{name}|{date}|{time}";

            using (var stream = new FileStream(
                _filePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(entry);
            }
        }

        private async Task<string[]> ReadAllLinesAsync(string path)
        {
            var lines = new List<string>();

            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        lines.Add(line);
                    }
                }
            }

            return lines.ToArray();
        }

        private async Task WriteAllLinesAsync(string path, IEnumerable<string> lines)
        {
            using (var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
            }
        }

        public async Task<List<(int Number, string Name, string Date, string Time)>> GetSortedEntriesAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<(int, string, string, string)>();
            }

            var entries = new List<(string Name, string Date, string Time, DateTime FullDateTime)>();
            var currentDateTime = DateTime.Now;

            string[] lines = await ReadAllLinesAsync(_filePath);

            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 3)
                {
                    string name = parts[0];
                    string date = parts[1];
                    string time = parts[2];

                    if (DateTime.TryParse($"{date} {time}", out var dt))
                    {
                        entries.Add((name, date, time, dt));
                    }
                }
            }

            var sortedEntries = entries
                .OrderBy(e => Math.Abs((e.FullDateTime - currentDateTime).Ticks))
                .Select((e, index) => (index + 1, e.Name, e.Date, e.Time))
                .ToList();

            return sortedEntries;
        }

        public async Task UpdateFileWithSortedEntriesAsync()
        {
            var sortedEntries = await GetSortedEntriesAsync();
            var lines = sortedEntries.Select(e => $"{e.Name}|{e.Date}|{e.Time}");
            await WriteAllLinesAsync(_filePath, lines);
        }
        public void CopySchedulePlan(string SchedulePath, string ScheduleQueue)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    string[] htmFiles = Directory.GetFiles(SchedulePath, "*.htm");

                    foreach (string file in htmFiles)
                    {
                        string destFile = Path.Combine(ScheduleQueue, Path.GetFileName(file));
                        File.Copy(file, destFile, overwrite: true);
                    }
                    Logger.Info($"Расписание {ScheduleQueue} было добавлено в очередь");
                    MessageBox.Show("Расписание добавлено в очередь", "Добавление в очередь", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка при копировании файлов расписания в папку: {ScheduleQueue}, ошибка:{ex}");
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public string[] GetTimerInfo()
        {
            var lines = File.ReadAllLines(_filePath)
                .Skip(1)
                .Select(line => line.Split('|'))
                .Where(parts => parts.Length >= 3);
            foreach (var line in lines)
            {
                string date = line[1];
                string time = line[2];
                string[] TimeInfo = { date, time };
                return TimeInfo;
            }
            return null;
        }

        public async Task ReturnCopySchedule(string SchedulePath, string ScheduleQueue)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    string[] htmFiles = Directory.GetFiles(SchedulePath, "*.htm");

                    foreach (string file in htmFiles)
                    {
                        string destFile = Path.Combine(ScheduleQueue, Path.GetFileName(file));
                        File.Copy(file, destFile, overwrite: true);
                    }
                    Logger.Info($"Расписание {ScheduleQueue} было скопировано для начала текущей загрузки");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка при копировании файлов расписания в папку: {ScheduleQueue}, ошибка:{ex}");
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }
        public bool CheckDate(string DateText, string TimeText)
        {
            DateTime datePart = DateTime.ParseExact(DateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            TimeSpan timePart = TimeSpan.ParseExact(TimeText, "h\\:mm", CultureInfo.InvariantCulture);
            DateTime FullDate = datePart.Add(timePart);
            bool isBeforeAnotherDate = Convert.ToDateTime(FullDate) < DateTime.Now;
            if (isBeforeAnotherDate)
            {
                MessageBox.Show("Ошибка добавления в очередь, дата неактуальна", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error($"Ошибка добавления в очередь: {DateText}, дата неактуальна");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}