namespace unloadSchedule
{
    public class JsonConfiguration
    {
        public string schedulePath { get; set; }
        public string Ip { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public JsonConfiguration(string path, string ip, string login, string password)
        {
            schedulePath = path;
            Ip = ip;
            Login = login;
            Password = password;
        }
    }

}
