using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using unloadSchedule.MVVM.Model;

namespace unloadSchedule.Classes
{
    public class FtpHelper
    {
        string ConfigPath = AppSettings.ConfigPath;
        string AllUnldPath = AppSettings.AllUnldPath;
        string OneDayUnldPath = AppSettings.OneDayUnldPath;
        public string jsonFile;

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
