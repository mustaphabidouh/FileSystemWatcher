using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemWatcher.Services.FileWatcher
{
    public interface IDelayedEvent
    {
       bool IsDuplicate(object obj);
    }
}
