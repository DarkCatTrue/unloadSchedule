using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using unloadSchedule.MVVM.Model;

namespace unloadSchedule.Classes
{
    public class JsonHandler
    {
        static string filecrnt = @"Jsons\currentTask.json";
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
    }
}
