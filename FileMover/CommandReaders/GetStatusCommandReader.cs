using FileMover.Interface;
using System.Text.RegularExpressions;

namespace FileMover.CommandReaders
{
    public class GetStatusCommandReader : RegexCommandReader
    { 
        private IFileMoverService FileMoverService { get; }
        private IWriter Writer { get; }

        public GetStatusCommandReader(IFileMoverService fileMoverService, IWriter writer) : base(@"^status$", RegexOptions.IgnoreCase)
        {
            FileMoverService = fileMoverService ?? throw new ArgumentNullException(nameof(fileMoverService));
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        protected override async Task RunCommandAsync(string commandText)
        {
            try
            {
                await FileMoverService.PrintFileStatusAsync();
            }

            catch (Exception exception)
            {
                while(exception != null)
                {
                    Writer.WriteLine(exception.Message);
                    exception = exception.InnerException;
                }
            }
        }
    }
}
