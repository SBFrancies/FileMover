using FileMover.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMover.Services
{
    public class IdGeneratorService : IIdGenerator
    {
        public Guid GenerateId()
        {
            return Guid.NewGuid();
        }
    }
}
