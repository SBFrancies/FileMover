using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMover.Interface
{
    public interface IApplicationProcesses
    {
        void ExitApplication(int exitCode = 0);
    }
}
