using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMover.Interface
{
    public interface ITaskFactory
    {
        void CreateAndStartTask(Action action, TaskCreationOptions taskCreationOptions);
    }
}
