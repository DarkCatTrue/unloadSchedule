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

        public async Task DefaultUnload(string SchedulePath, string Ip, string Login, string Password, string Mask)
        {

            string[] Files = Directory.GetFiles(SchedulePath, Mask);

            string lastUploadedFile = jsHandler.ReadCurrentFile();
            double CurrentProgress = jsHandler.ReadCurrentProgress();

            bool startUploading = string.IsNullOrEmpty(lastUploadedFile);

            bool fileFound = false;
            
            double totalFiles = Files.Length;

            double progress = 0;

            using (var Ftp = new AsyncFtpClient(Ip, Login, Password))
            {
                try
                {
                    await Ftp.Connect();

                    foreach (var File in Files)
                    {
                        if (!startUploading && !fileFound)
                        {
                            if (Path.GetFileName(File) == Path.GetFileName(lastUploadedFile))
                            {
                                fileFound = true;
                                progress = CurrentProgress;
                                continue;
                            }
                            continue;
                        }
                        try
                        {
                            await Ftp.UploadFile(File, $"/{Path.GetFileName(File)}", FtpRemoteExists.Overwrite, false, FtpVerify.None);
                            progress += (1 / totalFiles) * 100;
                            SaveAllUnloadJson(File, progress);
                        }
                        catch (FtpCommandException ex)
                        {
                            MessageBox.Show($"Ошибка во время загрузки файла '{File}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Неизвестная ошибка при загрузке файла '{File}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    await Ftp.Disconnect();
                }
                catch (FtpCommandException ex)
                {
                    MessageBox.Show($"Ошибка подключения к FTP серверу: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неизвестная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task UnloadTomorrow(string SchedulePath, string Ip, string Login, string Password, string Mask)
        {
            var files = Directory.GetFiles(SchedulePath);

            var filteredFiles = files.Where(file =>
                Path.GetFileName(file).StartsWith("c", StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(file).StartsWith("h", StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(file).StartsWith("v", StringComparison.OrdinalIgnoreCase)).ToList();

            string lastUploadedFile = jsHandler.ReadOneDayFile();
            double CurrentProgress = jsHandler.ReadOneDayProgress();

            bool startUploading = string.IsNullOrEmpty(lastUploadedFile);

            bool fileFound = false;

            double totalFiles = filteredFiles.Count;
            
            double progress = 0;
            
            using (var Ftp = new AsyncFtpClient(Ip, Login, Password))
            {
                await Ftp.Connect();

                foreach (var file in filteredFiles)
                {
                    if (!startUploading && !fileFound)
                    {
                        if (Path.GetFileName(file) == Path.GetFileName(lastUploadedFile))
                        {
                            fileFound = true;
                            progress = CurrentProgress;
                            continue;
                        }
                        continue;
                    }
                    try
                    {
                        await Ftp.UploadFile(file, $"/{Path.GetFileName(file)}");
                        progress += (1 / totalFiles) * 100;
                        SaveOneDayJson(file, progress);
                    }
                    catch (FtpCommandException ex)
                    {
                        MessageBox.Show($"Ошибка во время загрузки файла '{file}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        public void SaveAllUnloadJson(string FileName, double progress)
        {
            AllUnload crnt = new AllUnload(FileName, progress);
            string json = JsonConvert.SerializeObject(crnt);
            File.WriteAllText(filecrnt, json);
            string LoadedFile = Path.GetFileName(FileName);
            OnAllUnload?.Invoke(LoadedFile);
            OnAllProgress?.Invoke(progress);
        }
        public void SaveOneDayJson(string FileName, double progress)
        {
            OneDayUnload tmrw = new OneDayUnload(FileName, progress);
            string json = JsonConvert.SerializeObject(tmrw);
            File.WriteAllText(filetmrw, json);
            string LoadedFile = Path.GetFileName(FileName);
            OnDayUnload?.Invoke(LoadedFile);
            OnDayProgress?.Invoke(progress);
        }
        public async Task StartDefaultUpload()
        {
            string jsonFile = File.ReadAllText(filepath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string path = json.SchedulePath;
            string ip = json.Ip;
            string login = json.Login;
            string password = json.Password;
            await DefaultUnload(path, ip, login, password, "*.htm");
        }
        public async Task StartTommorowUpload()
        {
            string jsonFile = File.ReadAllText(filepath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string path = json.SchedulePath;
            string ip = json.Ip;
            string login = json.Login;
            string password = json.Password;
            await UnloadTomorrow(path, ip, login, password, "*.htm");
        }
    }
}
