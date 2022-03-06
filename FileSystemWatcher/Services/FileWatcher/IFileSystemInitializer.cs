﻿using FileSystemWatcher.Models;
using System.Collections.Generic;

namespace FileSystemWatcher.Services.FileWatcher
{
    public interface IFileSystemInitializer
    {
        string GetFileName(string path);
        string[] GetFileOfDirectory(string path);
        bool IsFileReadOnly(string FileName);
        void SetFileReadAccess(string FileName, bool readOnly);
        void WriteDirectory(string path);
        void MoveFile(string src, string dest);
        void DeleteDirWithFiles(string path);
        void WriteDirectoryToDossiersTraites(string path);
        void WriteDirectoryToDossiersEnErreur(string path);
        void WriteFilesToKofaxPathAndDossiersTraites(string path);
        void MoveFiles_From_Kofax_To_DossiersEnErreur(List<Page> pages, string path);
        void MoveXmlFiles_From_KofaxErrors_To_DossiersEnErreur(string path);
        void DeleteDirWithFiles_From_DossiersEnAttente(string path);
        void DeleteDirWithFiles_From_DossiersTraites(string path);
    }
}
