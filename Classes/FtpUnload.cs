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
            Action<string> FileName;
            Action<double> Progress;

            FileName = isOneDayUnload ? OneDayFile : DefaultFile;
            Progress = isOneDayUnload ? OneDayProgress : DefaultProgress;

            string progressPath = ftpHelper.GetProgressPath(isOneDayUnload);
            string schedulePath = ftpHelper.GetSchedulePath();

            string[] files = ftpHelper.GetFiles(schedulePath, isOneDayUnload);

            string lastUploadedFile = jsonHandler.ReadFile(progressPath);
            double currentProgress = jsonHandler.ReadProgress(progressPath);

            bool startUploading = string.IsNullOrEmpty(lastUploadedFile);
            bool fileFound = false;
            double progress = 0;

            var ftp = await ftpHelper.ConnectToFtpAsync(ip, login, password);

            foreach (string file in files)
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
                await ftpHelper.FtpUploader(ftp, file, schedulePath, isOneDayUnload);
                progress += (1.0 / files.Length) * 100;
                ftpHelper.SaveProgress(file, progress, isOneDayUnload, FileName, Progress);

            }
        }
        public async Task OneDayUnload(bool isOneDayUnload)
        {
            string jsonFile = File.ReadAllText(ConfigPath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string path = json.SchedulePath;
            string ip = json.Ip;
            string login = json.Login;
            string password = json.Password;
            await UnloadFiles(ip, login, password, isOneDayUnload);
        }
    }
}