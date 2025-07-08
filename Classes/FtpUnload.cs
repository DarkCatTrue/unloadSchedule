using FluentFTP;
using FluentFTP.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using unloadSchedule.MVVM.Model;
namespace unloadSchedule.Classes
{
    public class FtpUnload
    {
        FtpHelper ftpHelper = new FtpHelper();
        JsonHandler jsonHandler = new JsonHandler();

        string ConfigPath = AppSettings.ConfigPath;

        public Action<string> OneDayFile;

        public Action<string> DefaultFile;

        public Action<double> OneDayProgress;

        public Action<double> DefaultProgress;
        public async Task UnloadFiles(string ip, string login, string password, bool isOneDayUnload)
        {
            Action<string> FileName = isOneDayUnload ? OneDayFile : DefaultFile;
            Action<double> Progress = isOneDayUnload ? OneDayProgress : DefaultProgress;

            string progressPath = ftpHelper.GetProgressPath(isOneDayUnload);
            string schedulePath = ftpHelper.GetSchedulePath();

            string lastUploadedFile = jsonHandler.ReadFile(progressPath);
            double currentProgress = jsonHandler.ReadProgress(progressPath);

            while (true)
            {
                AsyncFtpClient ftp = null;
                try
                {
                    while (true)
                    {
                        try
                        {
                            ftp = new AsyncFtpClient(ip, login, password);
                            await ftp.Connect();
                            break;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка подключения: {ex.Message}. Повтор через 1 минуту.",
                                         "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            await Task.Delay(TimeSpan.FromMinutes(1));
                        }
                    }

                    string[] files = ftpHelper.GetFiles(schedulePath, isOneDayUnload);
                    bool shouldResume = !string.IsNullOrEmpty(lastUploadedFile);
                    bool foundLastFile = !shouldResume;

                    foreach (string file in files)
                    {
                        if (shouldResume && !foundLastFile)
                        {
                            if (Path.GetFileName(file) == Path.GetFileName(lastUploadedFile))
                            {
                                foundLastFile = true;
                            }
                            continue;
                        }

                        while (true)
                        {
                            try
                            {
                                await ftp.UploadFile(file, $"/{Path.GetFileName(file)}", FtpRemoteExists.Overwrite, false, FtpVerify.None);

                                currentProgress += (1.0 / files.Length) * 100;
                                ftpHelper.SaveProgress(file, currentProgress, isOneDayUnload, FileName, Progress);
                                lastUploadedFile = file;
                                break;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка загрузки {Path.GetFileName(file)}: {ex.Message}. Повтор через 1 минуту.",
                                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                await Task.Delay(TimeSpan.FromMinutes(1));

                                try { await ftp.Connect(); } catch {}
                            }
                        }
                    }
                    ftpHelper.SaveProgress("", 0, isOneDayUnload, FileName, Progress);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Критическая ошибка: {ex.Message}. Повтор через 1 минуту.",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                finally
                {
                    if (ftp != null)
                    {
                        try { await ftp.Disconnect(); } catch {}
                    }
                }
            }
        }
        public async Task Unload(bool isOneDayUnload)
        {
            string jsonFile = File.ReadAllText(ConfigPath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            try
            {
                string path = json.SchedulePath;
                string ip = json.Ip;
                string login = json.Login;
                string password = json.Password;
                await UnloadFiles(ip, login, password, isOneDayUnload);
            }
            catch
            { MessageBox.Show("Не найдены данные для начала выгрузки!", "Ввод данных", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}