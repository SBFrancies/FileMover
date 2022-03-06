namespace FileMover.Interface
{
    public interface ICommandReader
    {
        Task ValidateAndRunAsync(string commandText);
    }
}
