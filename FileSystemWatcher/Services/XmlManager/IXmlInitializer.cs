using FileSystemWatcher.Models;
using System.Collections.Generic;

namespace FileSystemWatcher.Services.XmlManager
{
    public interface IXmlInitializer
    {
        void CreateXMLFile(string dirPath, string dirName);
        List<Page> DeserializeXmlFile(string dirPath);
    }
}
