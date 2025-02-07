using FluentFTP;
using FluentFTP.Exceptions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
namespace unloadSchedule.Classes
{
    public class FtpUnload
    {
        string filepath = @"Jsons\currentTask.json";
        public async Task DefaultUnload(string SchedulePath, string Ip, string Login, string Password, string Mask)
        {
            string[] Files = Directory.GetFiles(SchedulePath, Mask);
            string lastUploadedFile = ReadCurrentJson();

            using (var Ftp = new AsyncFtpClient(Ip, Login, Password))
            {
                try
                {
                    await Ftp.Connect();
                    bool startUploading = string.IsNullOrEmpty(lastUploadedFile);
                    bool fileFound = false;

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
                    MessageBox.Show($"Ошибка подключения к FTP: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
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
            CurrentTask task = new CurrentTask(null, FileName);
            string json = JsonConvert.SerializeObject(task);
            File.WriteAllText(filepath, json);
        }
        public string ReadCurrentJson()
        {
            string jsonFile = File.ReadAllText(filepath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string FileName = json.CurrentFile;
            if (string.IsNullOrEmpty(FileName))
            {
                return string.Empty;
            }
            else
            { return FileName; }
        }
    }
}
