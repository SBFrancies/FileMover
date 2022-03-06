using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FileMover.IntegrationTests
{
    public class ApplicationTests
    {
        [Fact(Skip ="Intermittent unresponsive behaviour")]
        public async Task Application_CanGetStatusWhenNoDownloads()
        {
            using var app = new ApplicationFactory();
            var process = await app.StartApplication();

            try
            {
                var waitForResponse = WaitForResponse(process);

                await process.StandardInput.WriteLineAsync("status");

                var line = await waitForResponse;

                Assert.Equal("No transfers to display", line);
            }

            finally
            {
                process.Kill();
            }

        }

        [Fact]
        public async Task Application_CanQuitApplication()
        {
            using var app = new ApplicationFactory();
            var process = await app.StartApplication();
            
            try
            {
                await process.StandardInput.WriteLineAsync("quit");
                await Task.Delay(TimeSpan.FromSeconds(5));
                Assert.True(process.HasExited);
            }

            finally
            {
                process.Kill();
            }
        }

        [Fact(Skip = "Intermittent unresponsive behaviour")]
        public async Task Application_CanTriggerFileTransfer()
        {
            using var app = new ApplicationFactory();
            var process = await app.StartApplication();

            var source = Directory.CreateDirectory(Guid.NewGuid().ToString());
            var destination = Directory.CreateDirectory(Guid.NewGuid().ToString());

            try
            {
                await File.WriteAllTextAsync($"{source}\\newFile1.txt", "test content");

                var waitForResponse = WaitForResponse(process);

                await process.StandardInput.WriteLineAsync($"transfer -s {source.FullName} -d {destination.FullName}");

                var line = await waitForResponse;

                Assert.Equal($"Transferring files from {source.FullName} to {destination.FullName}", line);
            }

            finally
            {
                process.Kill();
                Directory.Delete(source.FullName, true);
                Directory.Delete(destination.FullName, true);
            }
        }

        [Fact(Skip = "Intermittent unresponsive behaviour")]
        public async Task Application_CanTriggerTransferAndThenGetStatus()
        {
            using var app = new ApplicationFactory();
            var process = await app.StartApplication();

            var source = Directory.CreateDirectory(Guid.NewGuid().ToString());
            var destination = Directory.CreateDirectory(Guid.NewGuid().ToString());

            try
            {
                await File.WriteAllTextAsync($"{source.FullName}\\newFile1.txt", "test content");
                await File.WriteAllTextAsync($"{source.FullName}\\newFile2.txt", "test content2");

                var waitForResponse = WaitForResponse(process);

                await process.StandardInput.WriteLineAsync($"transfer -s {source.FullName} -d {destination.FullName}");

                var line = await waitForResponse;

                Assert.Equal($"Transferring files from {source.FullName} to {destination.FullName}", line);

                await Task.Delay(TimeSpan.FromSeconds(5));

                var waitForStatusResponse = WaitForResponse(process, 2);

                await process.StandardInput.WriteLineAsync("status");

                var lines = await waitForStatusResponse;

                Assert.Equal(2, lines.Length);
                Assert.NotEqual("No transfers to display", lines.First());
                Assert.Single(lines.Where(a => a.Contains("newFile1")));
                Assert.Single(lines.Where(a => a.Contains("newFile2")));
            }

            finally
            {
                process.Kill();
                Directory.Delete(source.FullName, true);
                Directory.Delete(destination.FullName, true);
            }
        }

        private Task<string> WaitForResponse(Process process)
        {
            return Task.Run(async () =>
            {
                var output = await process.StandardOutput.ReadLineAsync();

                return output;
            });
        }

        private Task<string[]> WaitForResponse(Process process, int lines)
        {
            return Task.Run(async () =>
            {
                string output = null;
                var list = new List<string>();

                for (var i = 0; i < lines; i++)
                {
                    output = await process.StandardOutput.ReadLineAsync();
                    list.Add(output);
                }

                return list.ToArray();
            });
        }
    }
}
