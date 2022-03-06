using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMover.IntegrationTests
{
    public class ApplicationFactory : IDisposable
    {
        private string DbLocation { get; } = Guid.NewGuid().ToString();

        public async Task<Process> StartApplication()
        {
            var testSettings = await CopyTestAppSettingsAsync();

            try
            {
                var processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory().Replace(".IntegrationTests", string.Empty), "FileMover.exe");
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                return Process.Start(processStartInfo);
            }

            finally
            {
                File.Delete(testSettings);
            }
        }

        private async Task<string> CopyTestAppSettingsAsync()
        {
            var folder = Directory.CreateDirectory(DbLocation);
            var appSettings = await File.ReadAllTextAsync("appSettings.test.json");
            appSettings = appSettings.Replace("FileMoverDB.db", $"{folder.FullName}\\FileMoverDB.db").Replace("\\", "\\\\");
            var path = Path.Combine(Directory.GetCurrentDirectory().Replace(".IntegrationTests", string.Empty), "appSettings.test.json");
            await File.WriteAllTextAsync(path, appSettings);

            return path;
        }

        public void Dispose()
        {
            Directory.Delete(DbLocation, true);
        }
    }
}
