using Autofac;
using FileSystemWatcher.Enum;
using FileSystemWatcher.Services;
using log4net;
using System;
using System.Configuration;
using System.Reflection;
using Topshelf;

namespace FileSystemWatcher
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            try
            {
                var container = ContainerOperations.Container;
                Logger.Info("Début programme");

                HostFactory.Run(c =>
                {
                    c.Service<FileSystemWatcherService>(s =>
                    {
                        s.ConstructUsing(tc => container.Resolve<FileSystemWatcherService>());
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });

                    c.RunAsLocalSystem();

                    c.SetDescription(
                        string.Format("Monitoring des fichiers en mode de création sur les deux répertoires {0} et {1}", 
                        ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersEnAttenteRepositoryPath.ToString()], 
                        ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxErrorsRepositoryPath.ToString()]
                        ));
                    c.SetDisplayName(Assembly.GetExecutingAssembly().GetName().Name);
                    c.SetServiceName(Assembly.GetExecutingAssembly().GetName().Name);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                Logger.Error("Ferméture programme");
            }
        }
    }
}
