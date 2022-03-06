using FileSystemWatcher.Enum;
using FileSystemWatcher.Models;
using log4net;
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

        private readonly ILog _logger;

        public FileSystemInitializer(ILog logger)
        {
            _kofaxRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxRepositoryPath.ToString()];
            _kofaxErrorsRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxErrorsRepositoryPath.ToString()];
            _dossiersEnAttenteRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersEnAttenteRepositoryPath.ToString()];
            _dossiersEnErreurRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersEnErreurRepositoryPath.ToString()];
            _dossiersTraitesRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersTraitesRepositoryPath.ToString()];

            _logger = logger;
        }

        public string GetFileName(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");
            string fileName = Path.GetFileName(path);
            return fileName;
        }

        public string GetDirectoryName(string path)
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

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            var files = GetFileOfDirectory(path);

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (File.Exists(file)) File.Delete(file);
                _logger.InfoFormat("Suppression du fichier {0} relatif au dossier {1} dont le chemin est {2}",
                    fileInfo.Name,
                    GetFileName(path),
                    Path.Combine(path, fileInfo.Name));
            }

            if (Directory.Exists(path)) Directory.Delete(path, true);

            _logger.InfoFormat("Suppression du repertoire {0} dont le chemin est {1}",
                    dirInfo.Name,
                    dirInfo.FullName);
        }

        public void WriteDirectory_To_DossiersTraites(string dirName)
        {
            if (dirName.Length == 0)
                throw new ArgumentNullException("le nom du répertoire est null");

            WriteDirectory(Path.Combine(_dossiersTraitesRepositoryPath, dirName));
            _logger.InfoFormat("Création du répertoire {0} dans le chemin {1}", dirName, _dossiersTraitesRepositoryPath);
        }

        public void WriteDirectory_To_DossiersEnErreur(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            FileInfo file = new FileInfo(path);
            string extension = file.Extension;
            string directoryName = file.Name.Replace(extension, "");

            WriteDirectory(Path.Combine(_dossiersEnErreurRepositoryPath, directoryName));
            _logger.InfoFormat("Création du répertoire {0} dans le chemin {1}", directoryName, _dossiersEnErreurRepositoryPath);
        }

        public void WriteFiles_To_KofaxPath_And_DossiersTraites(string path)
        {
            if (path.Length == 0)
                throw new ArgumentNullException("path of directory is null");

            var dossiersEnAttentefiles = GetFileOfDirectory(path);

            foreach (string file in dossiersEnAttentefiles)
            {
                if (!IsFileReadOnly(file))
                    SetFileReadAccess(file, false);

                FileInfo fileInfo = new FileInfo(file);

                File.Copy(file, Path.Combine(_kofaxRepositoryPath, fileInfo.Name), true);
                _logger.InfoFormat("Copier le fichier {0} dans le repertoire {1} dont le chemin est {2} vers le chemin {3}",
                    fileInfo.Name,
                    GetFileName(path),
                    path,
                    _kofaxRepositoryPath);

                File.Copy(file, Path.Combine(_dossiersTraitesRepositoryPath, GetFileName(path), fileInfo.Name), true);
                _logger.InfoFormat("Copier le fichier {0} dans le repertoire {1} dont le chemin est {2} vers le chemin {3}",
                    fileInfo.Name,
                    GetFileName(path),
                    path,
                    Path.Combine(_dossiersTraitesRepositoryPath, GetFileName(path)));
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

                _logger.InfoFormat("Déplacement du fichier {0} relatif au dossier {1} existant dans le chemin {2} vers le chemin {3}", 
                    item.ImportFileName,
                    directoryName,
                    _kofaxRepositoryPath,
                    Path.Combine(_dossiersEnErreurRepositoryPath, directoryName));
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

            MoveFile(Path.Combine(_kofaxErrorsRepositoryPath, path), Path.Combine(_dossiersEnErreurRepositoryPath, directoryName, string.Format("{0}{1}", prefix, file.Name)));

            _logger.InfoFormat("Deplacement du fichier xml {0} renomme en {1} relatif au dossier {2} dont le chemin est {3} vers le chemin {4}",
                    file.Name,
                    string.Format("{0}{1}", prefix, file.Name),
                    directoryName,
                    Path.Combine(_kofaxErrorsRepositoryPath, file.Name),
                    Path.Combine(_dossiersEnErreurRepositoryPath, directoryName));
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
