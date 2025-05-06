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

        public const string AllUnldPath = @"Jsons\AllUnload.json";

        public const string OneDayUnldPath = @"Jsons\OneDayUnload.json";

        static string ScheduleFolders = "ScheduleFolders";
        
        string pathConfig = "Jsons";

        string firstScheduleFolder = $"{ScheduleFolders}\\First";

        string secondScheduleFolder = $"{ScheduleFolders}\\Second";

        string thirdScheduleFolder = $"{ScheduleFolders}\\Third";

        public void InitializeFolders()
        {
            if (!Directory.Exists(pathConfig))
                Directory.CreateDirectory(pathConfig);

            if (!File.Exists(ConfigPath))
                File.Create(ConfigPath);

            if (!File.Exists(OneDayUnldPath))
                File.Create(OneDayUnldPath);

            if (!File.Exists(AllUnldPath))
                File.Create(AllUnldPath);

            if (!Directory.Exists(ScheduleFolders))
                Directory.CreateDirectory(ScheduleFolders);

            if (!Directory.Exists(firstScheduleFolder))
                Directory.CreateDirectory(firstScheduleFolder);

            if (!Directory.Exists(secondScheduleFolder))
                Directory.CreateDirectory(secondScheduleFolder);

            if (!Directory.Exists(thirdScheduleFolder))
                Directory.CreateDirectory(thirdScheduleFolder);
        }
    }
}
