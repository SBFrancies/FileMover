using FileMover.Enums;

namespace FileMover.Data.Entities
{
    public class FileTransfer
    {
        public Guid Id { get; set; }

        public Guid OriginalSessionId { get; set; }

        public Guid? CurrentSessionId { get; set; }

        public string SourcePath { get; set; }

        public string DestinationPath { get; set; }

        public TransferStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime Created { get; set; }

        public override string ToString()
        {
            var retString = $"{SourcePath} -> {DestinationPath} Status: {Status}";

            if(!string.IsNullOrEmpty(ErrorMessage))
            {
                retString += $" - {ErrorMessage}";
            }

            return retString;
        }
    }
}
