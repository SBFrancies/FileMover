using FileMover.Interface;
using System.Text.RegularExpressions;

namespace FileMover.CommandReaders
{
    public class ExitCommandReader : RegexCommandReader
    {
        private IApplicationProcesses ApplicationProcesses { get; }

        public ExitCommandReader(IApplicationProcesses applicationProcesses) : base("^quit$", RegexOptions.IgnoreCase)
        {
            ApplicationProcesses = applicationProcesses ?? throw new ArgumentNullException(nameof(applicationProcesses));
        }

        protected override Task RunCommandAsync(string commandText)
        {
            ApplicationProcesses.ExitApplication();
            return Task.CompletedTask;
        }
    }
}
