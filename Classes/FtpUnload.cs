using FluentFTP;
using FluentFTP.Exceptions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using unloadSchedule.MVVM.Model;
namespace unloadSchedule.Classes
{
    public class FtpUnload
    {
        static string filecrnt = @"Jsons\currentTask.json";
        static string filepath = @"Jsons\configuration.json";

        public event Action<string> OnFileUploaded;

        public async Task DefaultUnload(string SchedulePath, string Ip, string Login, string Password, string Mask)
        {

            string[] Files = Directory.GetFiles(SchedulePath, Mask);

            string lastUploadedFile = ReadCurrentJson();

            bool startUploading = string.IsNullOrEmpty(lastUploadedFile);

            bool fileFound = false;

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
                                continue;
                            }
                            continue;
                        }
                        try
                        {
                            await Ftp.UploadFile(File, $"/{Path.GetFileName(File)}", FtpRemoteExists.Overwrite, false, FtpVerify.None);
                            SaveCurrentJson(File);
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
            MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public void SaveCurrentJson(string FileName)
        {
            CurrentTaskJson crnt = new CurrentTaskJson(null, FileName);
            string json = JsonConvert.SerializeObject(crnt);
            File.WriteAllText(filecrnt, json);
            string LoadedFile = Path.GetFileName(FileName);
            OnFileUploaded?.Invoke(LoadedFile);
        }
        public string ReadCurrentJson()
        {
            string jsonFile = File.ReadAllText(filecrnt);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            try
            {
                string FileName = json.CurrentFile;
                if (string.IsNullOrEmpty(FileName))
                {
                    return string.Empty;
                }
                else
                { return FileName; }
            }
            catch { return string.Empty; }
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
    }
}
