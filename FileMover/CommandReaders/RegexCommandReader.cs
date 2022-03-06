using FileMover.Interface;
using System.Text.RegularExpressions;

namespace FileMover.CommandReaders
{
    public abstract class RegexCommandReader : ICommandReader
    {
        protected Regex Regex { get; }

        protected RegexCommandReader(string pattern, RegexOptions options = RegexOptions.None)
        {
            Regex = new Regex(pattern, options);
        }

        protected abstract Task RunCommandAsync(string commandText);

        public async Task ValidateAndRunAsync(string commandText)
        {
            if(Regex.IsMatch(commandText))
            {
                await RunCommandAsync(commandText);
            }
        }
    }
}
