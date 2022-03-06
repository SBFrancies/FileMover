using FileMover.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMover.Services
{
    public class ApplicationProcesses : IApplicationProcesses
    {
        public void ExitApplication(int exitCode = 0)
        {
            Environment.Exit(exitCode);
        }
    }
}
