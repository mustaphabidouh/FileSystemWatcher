using System;
using System.Collections;
using System.IO;
using System.Timers;

namespace FileSystemWatcher.Services.FileWatcher
{
    public interface IFileSystemSafeWatcher : IDisposable
    {
        event FileSystemEventHandler Changed;
        event FileSystemEventHandler Created;
        event FileSystemEventHandler Deleted;
        event RenamedEventHandler Renamed;

        void BeginInit();
        void EndInit();
        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType);
        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout);
        void Initialize(string path, bool enableRaisingEvents);
        void Pause(bool enableRaisingEvents);
    }
}
