using FileMover.Interface;
using System.Text.RegularExpressions;

namespace FileMover.CommandReaders
{
    public class TransferCommandReader : RegexCommandReader
    { 
        private IFileMoverService FileMoverService { get; }
        private IWriter Writer { get; }

        public TransferCommandReader(IFileMoverService fileMoverService, IWriter writer) : base(@"^transfer\s+-s\s+(?<source>\S+(.*\S+)*)\s+-d\s+(?<destination>\S+(.*\S+)*)$", RegexOptions.IgnoreCase)
        {
            FileMoverService = fileMoverService ?? throw new ArgumentNullException(nameof(fileMoverService));
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        protected override async Task RunCommandAsync(string commandText)
        {
            try
            {
                var result = Regex.Match(commandText);

                var source = result.Groups["source"].Value.Trim();
                var destination = result.Groups["destination"].Value.Trim();

                await FileMoverService.MoveFilesAsync(source, destination);
            }

            catch (Exception exception)
            {
                while (exception != null)
                {
                    Writer.WriteLine(exception.Message);
                    exception = exception.InnerException;
                }
            }
        }
    }
}
