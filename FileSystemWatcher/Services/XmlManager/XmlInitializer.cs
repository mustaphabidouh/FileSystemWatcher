using FileSystemWatcher.Enum;
using FileSystemWatcher.Models;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FileSystemWatcher.Services.XmlManager
{
    public class XmlInitializer : IXmlInitializer
    {
        private readonly string _kofaxRepositoryPath;
        private readonly string _dossiersTraitesRepositoryPath;

        // Enums
        private readonly string _xlmBatchAttrName = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrName.ToString()];
        private readonly string _xmlBatchAttrPriority = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrPriority.ToString()];
        private readonly string _xmlBatchAttrBatchClassName = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrBatchClassName.ToString()];
        private readonly string _xmlBatchAttrProcessed = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrProcessed.ToString()];
        private readonly string _xmlBatchAttrRelativeImageFilePath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrRelativeImageFilePath.ToString()];
        private readonly string _xmlBatchAttrImportFileName = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrImportFileName.ToString()];

        private static readonly XmlSerializerNamespaces _namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });


        public XmlInitializer()
        {
            _kofaxRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxRepositoryPath.ToString()];
            _dossiersTraitesRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersTraitesRepositoryPath.ToString()];
        }

        public void CreateXMLFile(string dirPath, string dirName)
        {
            List<Pages> listPage = new List<Pages>();

            var files = Directory.GetFiles(dirPath);
            foreach (var file in files)
            {
                Pages pages = new Pages(new Page { ImportFileName = Path.GetFileName(file), OriginalFileName = Path.Combine(_kofaxRepositoryPath, Path.GetFileName(file)) });
                listPage.Add(pages);
            }

            Documents documents = new Documents(_xmlBatchAttrImportFileName, listPage);

            Batche batche = new Batche(
                _xlmBatchAttrName,
                _xmlBatchAttrPriority,
                _xmlBatchAttrBatchClassName,
                _xmlBatchAttrProcessed,
                _xmlBatchAttrRelativeImageFilePath,
                documents
                );

            Batches batches = new Batches(batche);
            ImportSession importSession = new ImportSession(batches);

            XmlSerializer serializer = new XmlSerializer(typeof(ImportSession));
            System.Text.Encoding encoding = Encoding.GetEncoding("utf-8");

            using (Stream streamKofax = new FileStream(string.Format("{0}{1}.xml", _kofaxRepositoryPath, dirName), FileMode.Create))
            using (XmlWriter xmlWriterKofax = new XmlTextWriter(streamKofax, encoding){ Formatting = Formatting.Indented})
            {
                serializer.Serialize(xmlWriterKofax, importSession, _namespaces);
            }

            using (Stream streamDossiersTraites = new FileStream(string.Format("{0}\\{1}\\{1}.xml", _dossiersTraitesRepositoryPath, dirName), FileMode.Create))
            using (XmlWriter xmlWriterDossiersTraite = new XmlTextWriter(streamDossiersTraites, encoding) { Formatting = Formatting.Indented })
            {
                serializer.Serialize(xmlWriterDossiersTraite, importSession, _namespaces);
            }
        }

        public List<Page> DeserializeXmlFile(string dirPath)
        {
            List<Page> pageList = new List<Page>();
            ImportSession importSession = new ImportSession();

            XmlSerializer serializer = new XmlSerializer(typeof(ImportSession));

            StreamReader reader = new StreamReader(dirPath);
            importSession = (ImportSession)serializer.Deserialize(reader);
            reader.Close();

            var pages = importSession.Batches.Batche.Documents.Pages.ToArray();

            foreach(var item in pages)
            {
                pageList.Add(item.Page);
            }

            return pageList;
        }
    }
}
