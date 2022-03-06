using FileSystemWatcher.Enum;
using FileSystemWatcher.Models;
using FileSystemWatcher.Services.XmlManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace FileSystemWatcher.Services.FileWatcher
{
    public class FileSystemInitializer: IFileSystemInitializer
    {
        private readonly string _kofaxRepositoryPath;
        private readonly string _kofaxErrorsRepositoryPath;
        private readonly string _dossiersEnAttenteRepositoryPath;
        private readonly string _dossiersEnErreurRepositoryPath;
        private readonly string _dossiersTraitesRepositoryPath;

        public FileSystemInitializer()
        {
            _kofaxRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxRepositoryPath.ToString()];
            _kofaxErrorsRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxErrorsRepositoryPath.ToString()];
            _dossiersEnAttenteRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersEnAttenteRepositoryPath.ToString()];
            _dossiersEnErreurRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersEnErreurRepositoryPath.ToString()];
            _dossiersTraitesRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersTraitesRepositoryPath.ToString()];
        }

        public string GetFileName(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");
            string directoryName = Path.GetFileName(path);
            return directoryName;
        }

        public string[] GetFileOfDirectory(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToArray();
            return files;
        }

        public bool IsFileReadOnly(string FileName)
        {
            if (FileName.Length == 0)
                throw new ArgumentNullException("FileName of directory is null");

            return new FileInfo(FileName).IsReadOnly;
        }

        public void SetFileReadAccess(string FileName, bool readOnly)
        {
            if (FileName.Length == 0)
                throw new ArgumentNullException("FileName of directory is null");
            File.SetAttributes(FileName, File.GetAttributes(FileName) & (readOnly ? FileAttributes.ReadOnly : ~FileAttributes.ReadOnly));
        }

        public void WriteDirectory(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");
            Directory.CreateDirectory(path);
        }

        public void MoveFile(string src, string dest)
        {
            if (src.Length == 0)
                throw new ArgumentNullException("path of source is null");
            if (dest.Length == 0)
                throw new ArgumentNullException("path of destination is null");

            File.Move(src, dest);
        }

        public void DeleteDirWithFiles(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of source is null");

            var files = GetFileOfDirectory(path);

            foreach (string file in files)
            {
                if (File.Exists(file)) File.Delete(file);
            }

            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        public void WriteDirectoryToDossiersTraites(string dirName)
        {
            if (dirName.Length == 0)
                throw new ArgumentNullException("le nom du répertoire est null");

            WriteDirectory(Path.Combine(_dossiersTraitesRepositoryPath, dirName));
        }

        public void WriteDirectoryToDossiersEnErreur(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            FileInfo file = new FileInfo(path);
            string extension = file.Extension;
            string directoryName = file.Name.Replace(extension, "");

            WriteDirectory(Path.Combine(_dossiersEnErreurRepositoryPath, directoryName));
        }

        public void WriteFilesToKofaxPathAndDossiersTraites(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            var dossiersEnAttentefiles = GetFileOfDirectory(path);

            foreach (string file in dossiersEnAttentefiles)
            {
                if (!IsFileReadOnly(file))
                    SetFileReadAccess(file, false);

                var fileName = GetFileName(file);

                File.Copy(file,string.Format("{0}\\{1}", _kofaxRepositoryPath, fileName), true);
                File.Copy(file, string.Format("{0}\\{1}\\{2}", _dossiersTraitesRepositoryPath, GetFileName(path), fileName), true);
            }
        }

        public void MoveFiles_From_Kofax_To_DossiersEnErreur(List<Page> pages, string path)
        {
            if (pages.Count == 0)
                throw new ArgumentNullException("Has no file");
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");


            FileInfo file = new FileInfo(path);
            string extension = file.Extension;
            string directoryName = file.Name.Replace(extension, "");

            foreach (var item in pages)
            {
                MoveFile(Path.Combine(_kofaxRepositoryPath, item.ImportFileName), Path.Combine(_dossiersEnErreurRepositoryPath, directoryName, item.ImportFileName));
            }
        }

        public void MoveXmlFiles_From_KofaxErrors_To_DossiersEnErreur(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            FileInfo file = new FileInfo(path);
            string extension = file.Extension;
            string directoryName = file.Name.Replace(extension, "");
            string prefix = ConfigurationManager.AppSettings[Enumerations.ConfigPrefix.DossiersEnErreurPrefix.ToString()];

            MoveFile(Path.Combine(_kofaxErrorsRepositoryPath, path), string.Format("{0}\\{1}\\{2}_{3}",_dossiersEnErreurRepositoryPath, directoryName, prefix, file.Name));
        }

        public void DeleteDirWithFiles_From_DossiersEnAttente(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            DeleteDirWithFiles(Path.Combine(_dossiersEnAttenteRepositoryPath, path));
        }

        public void DeleteDirWithFiles_From_DossiersTraites(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            FileInfo file = new FileInfo(path);
            string extension = file.Extension;
            string directoryName = file.Name.Replace(extension, "");

            DeleteDirWithFiles(Path.Combine(_dossiersTraitesRepositoryPath, directoryName));
        }


    }
}
