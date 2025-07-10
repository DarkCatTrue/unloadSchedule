using FluentFTP;
using FluentFTP.Exceptions;
using Newtonsoft.Json;
using NLog;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        FtpHelper ftpHelper = new FtpHelper();
        JsonHandler jsonHandler = new JsonHandler();

        string ConfigPath = AppSettings.ConfigPath;

        public Action<string> OneDayFile;

        public Action<string> DefaultFile;

        public Action<double> OneDayProgress;

        public Action<double> DefaultProgress;
        public async Task UnloadFiles(string ip, string login, string password, bool isOneDayUnload)
        {
            int waitTime = 1;
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
                            Logger.Info("Подключение к FTP серверу успешно выполнено.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Ошибка подключения к FTP серверу", ex);
                            Logger.Warn($"Попытка подключения к FTP серверу начнётся через: {waitTime} минут");
                            await Task.Delay(TimeSpan.FromMinutes(waitTime));
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
                                Logger.Error("При передаче файлов на FTP сервер произошла ошибка:", ex);
                                Logger.Warn($"Попытка продолжить загрузку файлов начнётся через: {waitTime} минут");
                                await Task.Delay(TimeSpan.FromMinutes(waitTime));
                                try
                                {
                                    await ftp.Connect();
                                    Logger.Info("Подключение к FTP серверу успешно восстановлено.");
                                }
                                catch (Exception e) 
                                {
                                    Logger.Error($"Ошибка подключения к FTP серверу после прерванной загрузки: {e}");
                                }
                            }
                        }
                    }
                    ftpHelper.SaveProgress("", 0, isOneDayUnload, FileName, Progress);
                    Logger.Info("Выгрузка полностью закончена, прогресс загрузки сброшен.");
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Error("Критическая ошибка", ex);
                    Logger.Warn($"Попытка восстановления начнётся через: {waitTime}");
                    await Task.Delay(TimeSpan.FromMinutes(waitTime));
                }
                finally
                {
                    if (ftp != null)
                    {
                        try
                        {
                            await ftp.Disconnect();
                            Logger.Info("Закрытие соединения с FTP сервером, после окончания загрузки.");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Ошибка закрытия соединения с FTP сервером:", ex);
                        }

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
                Logger.Info("Сбор информации о FTP сервере.");

                string path = json.SchedulePath;
                string ip = json.Ip;
                string login = json.Login;
                string password = json.Password;
                await UnloadFiles(ip, login, password, isOneDayUnload);
                Logger.Info("Выгрузка успешно завершена.");
            }

            catch (Exception ex)
            {
                Logger.Error("Ошибка начала загрузки:", ex);
            }
        }
    }
}