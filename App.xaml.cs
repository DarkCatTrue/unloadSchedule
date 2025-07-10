using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            LoadNLogConfig();

            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("=== Программа запущена ===");

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("=== Завершение работы ===");
            LogManager.Shutdown();
            base.OnExit(e);
        }
        private void LoadNLogConfig()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "unloadSchedule.NLog.config";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException("Ресурс NLog.config не найден!");

                    LogManager.Configuration = new XmlLoggingConfiguration(XmlReader.Create(stream), null);
                }
            }
            catch (Exception ex)
            {
                var config = new LoggingConfiguration();
                var consoleTarget = new NLog.Targets.ConsoleTarget("console");
                config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
                LogManager.Configuration = config;

                Console.WriteLine($"Ошибка загрузки NLog.config: {ex.Message}");
            }
        }
    }
}
