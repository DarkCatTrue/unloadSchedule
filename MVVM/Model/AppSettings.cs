using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unloadSchedule.Classes
{
    public class AppSettings
    {

        public const string ConfigPath = @"Jsons\configuration.json";

        public const string QueuePath = @"Jsons\queue.txt";

        public const string AllUnldPath = @"Jsons\AllUnload.json";

        public const string OneDayUnldPath = @"Jsons\OneDayUnload.json";

        public const string ScheduleFolders = "ScheduleFolders";

        string pathConfig = "Jsons";

        string logsFolder = "Logs";

        public void InitializeFolders()
        {

            Directory.CreateDirectory(pathConfig);

            Directory.CreateDirectory(logsFolder);

            Directory.CreateDirectory(ScheduleFolders);

            if (!File.Exists(ConfigPath))
                File.Create(ConfigPath).Dispose();

            if (!File.Exists(OneDayUnldPath))
                File.Create(OneDayUnldPath).Dispose();

            if (!File.Exists(AllUnldPath))
                File.Create(AllUnldPath).Dispose();

            if (!File.Exists(QueuePath))
                File.Create(QueuePath).Dispose();
        }
    }
}
