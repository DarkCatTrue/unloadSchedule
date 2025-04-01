using Newtonsoft.Json;
using System.IO;
using System.Windows.Shapes;
using unloadSchedule.Classes;

namespace unloadSchedule
{
    public class JsonConfiguration
    {
        string ConfigPath = AppSettings.ConfigPath;
        public string SchedulePath { get; set; }
        public string Ip { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public JsonConfiguration(string schedulePath, string ip, string login, string password)
        {
            SchedulePath = schedulePath;
            Ip = ip;
            Login = login;
            Password = password;
        }
        public void SaveConfiguration(string path, string ip, string login, string password)
        {
            JsonConfiguration configuration = new JsonConfiguration(path, ip, login, password);
            string json = JsonConvert.SerializeObject(configuration);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
