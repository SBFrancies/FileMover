using System.Collections.Concurrent;

namespace FileMover.Interface
{
    public interface IQueueProcessor<T>
    {
        Task ProcessQueueAsync(BlockingCollection<T> inputQueue);
    }
}
