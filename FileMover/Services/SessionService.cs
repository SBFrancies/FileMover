using FileMover.Interface;
using System.Collections.Concurrent;

namespace FileMover.Services
{
    public class SessionService : ISession
    {
        private ConcurrentDictionary<string, IList<string>> InternalDictionary {get;} = new ConcurrentDictionary<string, IList<string>>();

        public Guid SessionId { get; }

        public SessionService(IIdGenerator idGenerator)
        {
            SessionId = idGenerator?.GenerateId() ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public bool AddToSession(string sourceDirectory, IList<string> filesToTransfer)
        {
            return InternalDictionary.TryAdd(sourceDirectory, filesToTransfer);
        }

        public void TransferComplete(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);

            if(InternalDictionary.TryGetValue(directory, out var files))
            {
                files.Remove(filePath);

                if(!files.Any())
                {
                    InternalDictionary.TryRemove(directory, out _);
                }
            }
        }
    }
}
