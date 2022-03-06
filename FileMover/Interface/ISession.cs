namespace FileMover.Interface
{
    public interface ISession
    {
        public Guid SessionId { get; } 

        public bool AddToSession(string sourceDirectory, IList<string> filesToTransfer);

        public void TransferComplete(string filePath);
    }
}
