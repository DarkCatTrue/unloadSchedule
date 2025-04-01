using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using unloadSchedule.MVVM.Model;

namespace unloadSchedule.Classes
{
    public class JsonHandler
    {
        public string ReadFile(string path)
        {
            string jsonFile = File.ReadAllText(path);
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
        public double ReadProgress(string path)
        {
            string jsonFile = File.ReadAllText(path);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonFile);
            try
            {
                double progress = json.CurrentProgress;
                if (progress == 0)
                {
                    return progress = 0;
                }
                else
                { return progress; }
            }
            catch { return 0; }
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
    }
}
