using FluentFTP;
using FluentFTP.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using unloadSchedule.MVVM.Model;
namespace unloadSchedule.Classes
{
    public class FtpUnload
    {
        static string filepath = @"Jsons\configuration.json";

        static string filecrnt = @"Jsons\AllUnload.json";

        static string filetmrw = @"Jsons\OneDayUnload.json";

        JsonHandler jsHandler = new JsonHandler();

        public event Action<string> OnDayUnload;

        public event Action<string> OnAllUnload;

        public event Action<double> OnDayProgress;

        public event Action<double> OnAllProgress;

        public async Task UnloadFiles(string schedulePath, string ip, string login, string password, string mask, bool isTomorrowUnload = false)
        {
            string jsonFile;
            Action<string> onUnloadEvent;
            Action<double> onProgressEvent;

            if (isTomorrowUnload)
            {
                jsonFile = filetmrw;
                onUnloadEvent = OnDayUnload;
                onProgressEvent = OnDayProgress;
            }
            else
            {
                jsonFile = filecrnt;
                onUnloadEvent = OnAllUnload;
                onProgressEvent = OnAllProgress;
            }

            string[] allFiles = Directory.GetFiles(schedulePath, mask);
            List<string> filesToUpload = new List<string>();

            if (isTomorrowUnload)
            {
                foreach (string file in allFiles)
                {
                    string filename = Path.GetFileName(file);
                    if (filename.StartsWith("c", StringComparison.OrdinalIgnoreCase) ||
                        filename.StartsWith("h", StringComparison.OrdinalIgnoreCase) ||
                        filename.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                    {
                        filesToUpload.Add(file);
                    }
                }
            }
            else
            {
                filesToUpload.AddRange(allFiles);
            }

            string lastUploadedFile = jsHandler.ReadFile(jsonFile);
            double currentProgress = jsHandler.ReadProgress(jsonFile);
            bool startUploading = string.IsNullOrEmpty(lastUploadedFile);
            bool fileFound = false;
            double progress = 0;

            using (var ftp = new AsyncFtpClient(ip, login, password))
            {
                try
                {
                    await ftp.Connect();

                    foreach (string file in filesToUpload)
                    {
                        if (!startUploading && !fileFound)
                        {
                            if (Path.GetFileName(file) == Path.GetFileName(lastUploadedFile))
                            {
                                fileFound = true;
                                progress = currentProgress;
                                continue;
                            }
                            continue;
                        }

                        try
                        {
                            await ftp.UploadFile(file, $"/{Path.GetFileName(file)}", FtpRemoteExists.Overwrite, false, FtpVerify.None);

                            progress += (1.0 / filesToUpload.Count) * 100;

                            if (isTomorrowUnload)
                            {
                                SaveJson<OneDayUnload>(file, progress, jsonFile, onUnloadEvent, onProgressEvent);
                            }
                            else
                            {
                                SaveJson<AllUnload>(file, progress, jsonFile, onUnloadEvent, onProgressEvent);
                            }
                        }
                        catch (FtpCommandException ex)
                        {
                            MessageBox.Show($"Ошибка загрузки файла '{file}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Неизвестная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    await ftp.Disconnect();
                }
                catch (FtpCommandException ex)
                {
                    MessageBox.Show($"Ошибка подключения к FTP: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неизвестная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void SaveJson<T>(string fileName, double progress, string outputPath, Action<string> onUnloadEvent, Action<double> onProgressEvent)
        {
            T data = (T)Activator.CreateInstance(typeof(T), fileName, progress);
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(outputPath, json);
            string loadedFile = Path.GetFileName(fileName);
            onUnloadEvent?.Invoke(loadedFile);
            onProgressEvent?.Invoke(progress);
        }
        public async Task StartUpload(bool isTomorrowUnload)
        {
            string jsonFile = File.ReadAllText(filepath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string path = json.SchedulePath;
            string ip = json.Ip;
            string login = json.Login;
            string password = json.Password;
            await UnloadFiles(path, ip, login, password, "*.htm", isTomorrowUnload);
        }
    }
}