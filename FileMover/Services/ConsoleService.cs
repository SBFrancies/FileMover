using FileMover.Interface;

namespace FileMover.Services
{
    public class ConsoleService : IReader, IWriter
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}
