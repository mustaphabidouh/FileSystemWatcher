using Autofac;
using FileSystemWatcher.Services;
using FileSystemWatcher.Services.FileWatcher;
using FileSystemWatcher.Services.XmlManager;
using log4net;
using System;

namespace FileSystemWatcher
{
    public class ContainerOperations
    {
        private static Lazy<IContainer> _containerSingleton = new Lazy<IContainer>(CreateContainer);

        public static IContainer Container => _containerSingleton.Value;


        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<XmlInitializer>().As<IXmlInitializer>().InstancePerLifetimeScope();
            builder.RegisterType<FileSystemInitializer>().As<IFileSystemInitializer>().InstancePerLifetimeScope();
            builder.RegisterType<FileSystemSafeWatcher>().As<IFileSystemSafeWatcher>().InstancePerLifetimeScope();
            builder.Register(c => LogManager.GetLogger(typeof(Object))).As<ILog>();
            builder.RegisterType<FileSystemWatcherService>();
            return builder.Build(); ;
        }
    }
}
