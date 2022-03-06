using System.IO;
namespace FileSystemWatcher.Services.FileWatcher
{
    public class DelayedEvent
    {
        private readonly FileSystemEventArgs _args;
        private bool _delayed;

        public FileSystemEventArgs Args
        {
            get
            {
                return _args;
            }
        }
        public bool Delayed
        {
            get
            {
                return _delayed;
            }
            set
            {
                _delayed = value;
            }
        }

        public DelayedEvent()
        {
            _delayed = false;
        }
        public DelayedEvent(FileSystemEventArgs args)
        {
            _delayed = false;
            _args = args;
        }

        public virtual bool IsDuplicate(object obj)
        {
            DelayedEvent delayedEvent = obj as DelayedEvent;
            if (delayedEvent == null)
                return false; // this is not null so they are different
            FileSystemEventArgs eO1 = _args;
            RenamedEventArgs reO1 = _args as RenamedEventArgs;
            FileSystemEventArgs eO2 = delayedEvent._args;
            RenamedEventArgs reO2 = delayedEvent._args as RenamedEventArgs;

            return ((eO1 != null && eO2 != null && eO1.ChangeType == eO2.ChangeType
                && eO1.FullPath == eO2.FullPath && eO1.Name == eO2.Name) &&
                ((reO1 == null && reO2 == null) || (reO1 != null && reO2 != null &&
                reO1.OldFullPath == reO2.OldFullPath && reO1.OldName == reO2.OldName))) ||
                (eO1 != null && eO2 != null && eO1.ChangeType == WatcherChangeTypes.Created
                && eO2.ChangeType == WatcherChangeTypes.Changed
                && eO1.FullPath == eO2.FullPath && eO1.Name == eO2.Name);
        }
    }
}
