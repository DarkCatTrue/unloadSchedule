using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentFTP.Exceptions;
using FluentFTP;
using System.Windows;
using Newtonsoft.Json;
using unloadSchedule.MVVM.Model;
using System.Diagnostics;

namespace unloadSchedule.Classes
{
    public class FtpHelper
    {
        JsonHandler jsonHandler = new JsonHandler();
        string ConfigPath = AppSettings.ConfigPath;
        string AllUnldPath = AppSettings.AllUnldPath;
        string OneDayUnldPath = AppSettings.OneDayUnldPath;
        public string jsonFile;

        public async Task<AsyncFtpClient> ConnectToFtpAsync(string ip, string login, string password)
        {
            var ftp = new AsyncFtpClient(ip, login, password);
            try
            {
                await ftp.Connect();
                return ftp;
            }
            catch (FtpCommandException ex)
            {
                MessageBox.Show($"Не удалось подключиться к FTP серверу, ошибка: {ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public async Task FtpUploader(AsyncFtpClient ftp, string file, string schedulePath, bool isOneDayUnload)
        {
            try
            {
                await ftp.UploadFile(file, $"/{Path.GetFileName(file)}", FtpRemoteExists.Overwrite, false, FtpVerify.None);
            }
            catch (FtpCommandException ex)
            { MessageBox.Show($"Ошибка загрузки файла: {file} \r Ошибка: {ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public void SaveProgress(string file, double progress, bool isOneDayUnload, Action<string> FileName, Action<double> Progress)
        {
            if (isOneDayUnload)
            {
                jsonHandler.SaveJson<OneDayUnload>(file, progress, jsonFile, FileName, Progress);
            }
            else
            {
                jsonHandler.SaveJson<AllUnload>(file, progress, jsonFile, FileName, Progress);
            }
        }

        public string GetProgressPath(bool isOneDayUnload = false)
        {
            return isOneDayUnload ? jsonFile = OneDayUnldPath : jsonFile = AllUnldPath;
        }

        public string GetSchedulePath()
        {
            string jsonFile = File.ReadAllText(ConfigPath);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            string path = json.SchedulePath;
            return path;
        }

        public string[] GetFiles(string schedulePath, bool isOneDayUnload = false)
        {
            string[] allfiles = Directory.GetFiles(schedulePath, "*.htm");
            List<string> filesToUpload = new List<string>();
            if (isOneDayUnload)
            {
                foreach (string files in allfiles)
                {
                    string filename = Path.GetFileName(files).ToLower();
                    if (filename.StartsWith("c") || filename.StartsWith("h") || filename.StartsWith("j") || filename.StartsWith("v"))
                    {
                        filesToUpload.Add(files);
                    }
                }
                return filesToUpload.ToArray();
            }
            else
            {
                filesToUpload.AddRange(allfiles);
                return filesToUpload.ToArray();
            }
        }
    }
}
