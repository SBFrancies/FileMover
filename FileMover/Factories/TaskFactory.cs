using FileMover.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMover.Factories
{
    public class TaskFactory : ITaskFactory
    {
        public void CreateAndStartTask(Action action, TaskCreationOptions taskCreationOptions)
        {
            new Task(action, taskCreationOptions).Start();
        }
    }
}
