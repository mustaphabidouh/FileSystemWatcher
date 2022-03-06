using System;
using System.IO;
using System.Timers;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using FileSystemWatcher.Enum;
using FileSystemWatcher.Services.XmlManager;
using System.Collections.Generic;
using FileSystemWatcher.Models;
using log4net;
using static FileSystemWatcher.Enum.Enumerations;

namespace FileSystemWatcher.Services.FileWatcher
{
    public class FileSystemSafeWatcher : IFileSystemSafeWatcher
    {
        private readonly System.IO.FileSystemWatcher _fileSystemWatcher;

        private readonly IFileSystemInitializer _fileSystemInitializer;
        private readonly IXmlInitializer _xmlInitializer;
        private readonly ILog _logger;
        private readonly object _enterThread = new object(); // Only one timer event is processed at any given moment
        private ArrayList _events;
        private Timer _serverTimer;
        private int _consolidationInterval = 1000; // milliseconds
        private string _currentDiretoryPath = string.Empty;
        private string _currentDiretoryName = string.Empty;

        #region Delegate to FileSystemWatcher

        protected FileSystemSafeWatcher()
        {
        }

        public FileSystemSafeWatcher(string path)
        {
            _fileSystemWatcher = new System.IO.FileSystemWatcher(path);
        }

        public FileSystemSafeWatcher(
            string path,
            IFileSystemInitializer fileSystemInitializer,
            IXmlInitializer xmlInitializer,
            ILog logger)
        {
            _fileSystemWatcher = new System.IO.FileSystemWatcher(path);
            _fileSystemInitializer = fileSystemInitializer;
            _xmlInitializer = xmlInitializer;
            _logger = logger;
        }
        

        public FileSystemSafeWatcher(string path, string filter)
        {
            _fileSystemWatcher = new System.IO.FileSystemWatcher(path, filter);
        }

        public bool EnableRaisingEvents
        {
            get
            {
                return _fileSystemWatcher.EnableRaisingEvents;
            }
            set
            {
                _fileSystemWatcher.EnableRaisingEvents = value;
                if (value)
                {
                    _serverTimer.Start();
                }
                else
                {
                    _serverTimer.Stop();
                    _events.Clear();
                }
            }
        }

        public string Filter
        {
            get
            {
                return _fileSystemWatcher.Filter;
            }
            set
            {
                _fileSystemWatcher.Filter = value;
            }
        }

        public bool IncludeSubdirectories
        {
            get
            {
                return _fileSystemWatcher.IncludeSubdirectories;
            }
            set
            {
                _fileSystemWatcher.IncludeSubdirectories = value;
            }
        }

        public int InternalBufferSize
        {
            get
            {
                return _fileSystemWatcher.InternalBufferSize;
            }
            set
            {
                _fileSystemWatcher.InternalBufferSize = value;
            }
        }

        public NotifyFilters NotifyFilter
        {
            get
            {
                return _fileSystemWatcher.NotifyFilter;
            }
            set
            {
                _fileSystemWatcher.NotifyFilter = value;
            }
        }

        public string Path
        {
            get
            {
                return _fileSystemWatcher.Path;
            }
            set
            {
                _fileSystemWatcher.Path = value;
            }
        }

        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                return _fileSystemWatcher.SynchronizingObject;
            }
            set
            {
                _fileSystemWatcher.SynchronizingObject = value;
            }
        }

        public event FileSystemEventHandler Changed;

        public event FileSystemEventHandler Created;

        public event FileSystemEventHandler Deleted;

        public event ErrorEventHandler Error;

        public event RenamedEventHandler Renamed;

        public void BeginInit()
        {
            _fileSystemWatcher.BeginInit();
        }

        public void Dispose()
        {
            Uninitialize();
        }

        public void EndInit()
        {
            _fileSystemWatcher.EndInit();
        }

        protected void OnChanged(FileSystemEventArgs e)
        {
            if (Changed != null)
                Changed(this, e);

        }

        protected void OnCreated(FileSystemEventArgs e, FileSourceAndTypeEvent fileSourceAndTypeEvent)
        {

            _currentDiretoryPath = e.FullPath;
            _currentDiretoryName = e.Name;
            _logger.InfoFormat("le fichier {0} dont le chemin est {1} source d'événement {2}", _currentDiretoryName, _currentDiretoryPath, e.ChangeType);

            try
            {
                string path = e.FullPath.Replace(e.Name, "");

                switch (fileSourceAndTypeEvent)
                {
                    case FileSourceAndTypeEvent.DossiersEnAttente_DirWithFile:

                        // Créer le dossier traité dans le dossiersTraitesRepositoryPath
                        _fileSystemInitializer.WriteDirectory_To_DossiersTraites(_currentDiretoryName);

                        // Copier les fichiers du répertoire scanné par le watcher dans KofaxRepositoryPath et dossiersEnAttenteRepositoryPath
                        _fileSystemInitializer.WriteFiles_To_KofaxPath_And_DossiersTraites(_currentDiretoryPath);

                        // Générer fileName.xml dans le chemin KofaxRepositoryPath
                        _xmlInitializer.CreateXMLFile(_currentDiretoryPath, _currentDiretoryName);

                        // Supprimer les dossiers traités depuis le dossier DossiersEnAttente
                        _fileSystemInitializer.DeleteDirWithFiles_From_DossiersEnAttente(_currentDiretoryPath);
                        break;

                    case FileSourceAndTypeEvent.DossiersEnAttente_DirWithoutFile:


                        break;

                    case FileSourceAndTypeEvent.KofaxErrors_XmlFile:

                            List<Page> pages = _xmlInitializer.DeserializeXmlFile(_currentDiretoryPath);

                            // Créer le dossier en erreur dans le dossiersEnErreurRepositoryPath
                            _fileSystemInitializer.WriteDirectory_To_DossiersEnErreur(_currentDiretoryPath);

                            // Couper et coller les fichiers depuis le dossier Kofax vers DossiersEnErreur
                            _fileSystemInitializer.MoveFiles_From_Kofax_To_DossiersEnErreur(pages, _currentDiretoryPath);

                            // Couper et coller le fichier xml depuis le dossier KofaxErrors vers DossiersEnErreur
                            _fileSystemInitializer.MoveXmlFiles_From_KofaxErrors_To_DossiersEnErreur(_currentDiretoryPath);

                            // Supprimer les dossiers traités depuis le dossier DossiersTraites
                            _fileSystemInitializer.DeleteDirWithFiles_From_DossiersTraites(_currentDiretoryPath);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Message d'erreur : {0}", ex));
            }
        }

        protected void OnDeleted(FileSystemEventArgs e)
        {
            if (Deleted != null)
                Deleted(this, e);
        }

        protected void OnError(ErrorEventArgs e)
        {
            if (Error != null)
                Error(this, e);
        }

        protected void OnRenamed(RenamedEventArgs e)
        {
            if (Renamed != null)
                Renamed(this, e);
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
        {
            //TODO
            throw new NotImplementedException();
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
        {
            //TODO
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation

        public void Initialize(bool enableRaisingEvents)
        {
            _events = ArrayList.Synchronized(new ArrayList(1000));
            //_fileSystemWatcher.Changed += new FileSystemEventHandler(this.FileSystemEventHandler);
            _fileSystemWatcher.Created += new FileSystemEventHandler(this.FileSystemEventHandler);
            //_fileSystemWatcher.Deleted += new FileSystemEventHandler(this.FileSystemEventHandler);
            _fileSystemWatcher.Error += new ErrorEventHandler(this.ErrorEventHandler);
            _fileSystemWatcher.Renamed += new RenamedEventHandler(this.RenamedEventHandler);
            _fileSystemWatcher.EnableRaisingEvents = enableRaisingEvents;

            _serverTimer = new Timer(_consolidationInterval);
            _serverTimer.Elapsed += new ElapsedEventHandler(this.ElapsedEventHandler);
            _serverTimer.AutoReset = true;
            _serverTimer.Enabled = _fileSystemWatcher.EnableRaisingEvents;
        }

        public void Pause(bool enableRaisingEvents)
        {
            _fileSystemWatcher.EnableRaisingEvents = enableRaisingEvents;
        }

        private void Uninitialize()
        {
            if (_fileSystemWatcher != null)
                _fileSystemWatcher.Dispose();
            if (_serverTimer != null)
                _serverTimer.Dispose();
        }

        private void FileSystemEventHandler(object sender, FileSystemEventArgs e)
        {
            _events.Add(new DelayedEvent(e));
        }

        private void ErrorEventHandler(object sender, ErrorEventArgs e)
        {
            OnError(e);
        }

        private void RenamedEventHandler(object sender, RenamedEventArgs e)
        {
            _events.Add(new DelayedEvent(e));
        }

        private void ElapsedEventHandler(Object sender, ElapsedEventArgs e)
        {
            Queue eventsToBeFired = null;
            if (System.Threading.Monitor.TryEnter(_enterThread))
            {              
                try
                {
                    eventsToBeFired = new Queue(10000);
                    lock (_events.SyncRoot)
                    {
                        DelayedEvent current;
                        for (int i = 0; i < _events.Count; i++)
                        {
                            current = _events[i] as DelayedEvent;

                            

                            if (current.Delayed)
                            {
                                // This event has been delayed already so we can fire it
                                // We just need to remove any duplicates
                                for (int j = i + 1; j < _events.Count; j++)
                                {
                                    if (current.IsDuplicate(_events[j]))
                                    {
                                        // Removing later duplicates
                                        _events.RemoveAt(j);
                                        j--; // Don't skip next event
                                    }
                                }

                                bool raiseEvent = true;
                                if (current.Args.ChangeType == WatcherChangeTypes.Created)
                                {
                                    FileInfo fileInfo = new FileInfo(current.Args.FullPath);
                                    string path = fileInfo.FullName.Replace(fileInfo.Name, "");

                                    if (path == ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxErrorsRepositoryPath.ToString()])
                                    {
                                        if (fileInfo.Extension == ".xml")
                                            current.FileSourceAndTypeEvent = FileSourceAndTypeEvent.KofaxErrors_XmlFile;
                                        try
                                        {
                                            // If this succeeds, the file is finished
                                            using (FileStream stream = File.Open(current.Args.FullPath, FileMode.Open, FileAccess.Read, FileShare.None)) 
                                            { 

                                            }
                                        }
                                        catch (IOException)
                                        {
                                            raiseEvent = false;
                                        }
                                    }
                                    else if (path == ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersEnAttenteRepositoryPath.ToString()])
                                    {
                                        string[] files = Directory.GetFiles(current.Args.FullPath);
                                        if (files.Length == 0) current.FileSourceAndTypeEvent = FileSourceAndTypeEvent.DossiersEnAttente_DirWithoutFile;
                                        else
                                            current.FileSourceAndTypeEvent = FileSourceAndTypeEvent.DossiersEnAttente_DirWithFile;

                                        foreach (var file in files)
                                        {
                                            try
                                            {
                                                // If this succeeds, the file is finished
                                                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None)) 
                                                {

                                                }
                                            }
                                            catch (IOException)
                                            {
                                                raiseEvent = false;
                                            }
                                        }
                                    }    
                                }

                                if (raiseEvent)
                                {
                                    // Add the event to the list of events to be fired
                                    eventsToBeFired.Enqueue(current);
                                    // Remove it from the current list
                                    _events.RemoveAt(i);
                                    i--; // Don't skip next event
                                }
                            }
                            else
                            {
                                // This event was not delayed yet, so we will delay processing
                                // this event for at least one timer interval
                                current.Delayed = true;
                            }
                        }
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(_enterThread);
                }
            }

            // Now fire all the events if any events are in eventsToBeFired
            RaiseEvents(eventsToBeFired);
        }

        public int ConsolidationInterval
        {
            get
            {
                return _consolidationInterval;
            }
            set
            {
                _consolidationInterval = value;
                _serverTimer.Interval = value;
            }
        }

        protected void RaiseEvents(Queue deQueue)
        {
            if ((deQueue != null) && (deQueue.Count > 0))
            {
                DelayedEvent de;
                while (deQueue.Count > 0)
                {
                    de = deQueue.Dequeue() as DelayedEvent;
                    switch (de.Args.ChangeType)
                    {
                        case WatcherChangeTypes.Changed:
                            OnChanged(de.Args);
                            break;
                        case WatcherChangeTypes.Created:
                            OnCreated(de.Args, de.FileSourceAndTypeEvent);
                            break;
                        case WatcherChangeTypes.Deleted:
                            OnDeleted(de.Args);
                            break;
                        case WatcherChangeTypes.Renamed:
                            OnRenamed(de.Args as RenamedEventArgs);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
