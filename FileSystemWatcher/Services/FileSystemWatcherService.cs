using FileSystemWatcher.Enum;
using FileSystemWatcher.Services.FileWatcher;
using FileSystemWatcher.Services.XmlManager;
using log4net;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace FileSystemWatcher.Services
{
    public class FileSystemWatcherService
    {
        private readonly ILog _logger;
        private static string _dossiersEnAttenteRepositoryPath;
        private static string _kofaxErrorsRepositoryPath;


        private readonly IFileSystemSafeWatcher _dossiersEnAttente_MonitoringInstance;
        private readonly IFileSystemSafeWatcher _kofaxErrors_MonitoringInstance;




        public FileSystemWatcherService(
            IFileSystemSafeWatcher dossiersEnAttente_MonitoringInstance,
            IFileSystemSafeWatcher kofaxErrors_MonitoringInstance,
            ILog logger)
        {
            _dossiersEnAttente_MonitoringInstance = dossiersEnAttente_MonitoringInstance;
            _kofaxErrors_MonitoringInstance = kofaxErrors_MonitoringInstance;
            _logger = logger;
        }
        public void Start()
        {
            #region  DossiersEnAttente

            _dossiersEnAttenteRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersEnAttenteRepositoryPath.ToString()];
            if (!Directory.Exists(_dossiersEnAttenteRepositoryPath))
            {
                _logger.Error(string.Format("Le fichier {0} n'existe pas.", _dossiersEnAttenteRepositoryPath));
                _logger.InfoFormat("Le programme {0} a fermé à {1}", Assembly.GetExecutingAssembly().GetName().Name, DateTime.Now.ToString(new CultureInfo("fr")));
                Stop();
            }

            _dossiersEnAttente_MonitoringInstance.Initialize(_dossiersEnAttenteRepositoryPath, true);
            _logger.InfoFormat("Le programme {0} commence a observer les dossiers créés sur le répertoire suivant {1}", Assembly.GetExecutingAssembly().GetName().Name, _dossiersEnAttenteRepositoryPath);

            #endregion

            #region  KofaxErrors

            _kofaxErrorsRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxErrorsRepositoryPath.ToString()];

            if (!Directory.Exists(_kofaxErrorsRepositoryPath))
            {
                _logger.Error(string.Format("Le fichier {0} n'existe pas.", _kofaxErrorsRepositoryPath));
                _logger.InfoFormat("Le programme {0} a fermé à {1}", Assembly.GetExecutingAssembly().GetName().Name, DateTime.Now.ToString(new CultureInfo("fr")));
                Stop();
            }

            //_kofaxErrors_MonitoringInstance = new FileSystemSafeWatcher(_kofaxErrorsRepositoryPath, _fileSystemInitializer, _xmlInitializer);
            _logger.InfoFormat("Le programme {0} est insialisé pour l'observation des dossiers créés sur le répertoire suivant {1}", Assembly.GetExecutingAssembly().GetName().Name, _kofaxErrorsRepositoryPath);

            _kofaxErrors_MonitoringInstance.Initialize(_kofaxErrorsRepositoryPath, true);
            _logger.InfoFormat("Le programme {0} commence a observer les dossiers créés sur le répertoire suivant {1}", Assembly.GetExecutingAssembly().GetName().Name, _kofaxErrorsRepositoryPath);

            #endregion

        }

        public void Pause()
        {
            #region  DossiersEnAttente

            _dossiersEnAttente_MonitoringInstance.Pause(false);
            _logger.InfoFormat("Le programme {0} a été mis en attente pour l'observation des dossiers créés sur le répertoire suivant {1}", Assembly.GetExecutingAssembly().GetName().Name, _dossiersEnAttenteRepositoryPath);

            #endregion

            #region  KofaxErrors

            _kofaxErrors_MonitoringInstance.Pause(false);
            _logger.InfoFormat("Le programme {0} a été mis en attente pour l'observation des dossiers créés sur le répertoire suivant", Assembly.GetExecutingAssembly().GetName().Name, _kofaxErrorsRepositoryPath);

            #endregion
        }

        public void Stop()
        {
            #region  DossiersEnAttente

            _dossiersEnAttente_MonitoringInstance.Dispose();
            _logger.InfoFormat("Le programme {0} a été arrêté pour l'observation des dossiers créés sur le répertoire suivant {1}", Assembly.GetExecutingAssembly().GetName().Name, _dossiersEnAttenteRepositoryPath);

            #endregion

            #region  KofaxErrors

            _kofaxErrors_MonitoringInstance.Dispose();
            _logger.InfoFormat("Le programme {0} a été arrêté pour l'observation des dossiers créés sur le répertoire suivant", Assembly.GetExecutingAssembly().GetName().Name, _kofaxErrorsRepositoryPath);

            #endregion
        }
    }
}
