using FluentFTP;
using FluentFTP.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
namespace unloadSchedule.Classes
{
    public class FtpUnload
    {
        public async Task DefaultUnload(string SchedulePath, string Ip, string Login, string Password, string Mask)
        {
            string[] Files = Directory.GetFiles(SchedulePath, Mask);

            using (var Ftp = new AsyncFtpClient(Ip, Login, Password))
            {
                try
                {
                    await Ftp.Connect();

                    foreach (var File in Files)
                    {
                        try
                        {
                            await Ftp.UploadFile(File, $"/{Path.GetFileName(File)}", FtpRemoteExists.Overwrite, false, FtpVerify.None);
                        }
                        catch (FtpCommandException ex)
                        {
                            MessageBox.Show($"Ошибка во время загрузки файла '{File}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Неизвестная ошибка при загрузке файла '{File}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    await Ftp.Disconnect();
                }
                catch (FtpCommandException ex)
                {
                    MessageBox.Show($"Ошибка подключения к FTP: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неизвестная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            MessageBox.Show("Выгрузка всех файлов завершилась!", "Состояние загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
