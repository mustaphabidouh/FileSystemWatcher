using FileSystemWatcher.Enum;
using FileSystemWatcher.Models;
using log4net;
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
        private readonly ILog _logger;

        // Enums
        private readonly string _xlmBatchAttrName = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrName.ToString()];
        private readonly string _xmlBatchAttrPriority = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrPriority.ToString()];
        private readonly string _xmlBatchAttrBatchClassName = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrBatchClassName.ToString()];
        private readonly string _xmlBatchAttrProcessed = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrProcessed.ToString()];
        private readonly string _xmlBatchAttrRelativeImageFilePath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlBatchAttrRelativeImageFilePath.ToString()];
        private readonly string _xmlDocumentAttrImportFileName = ConfigurationManager.AppSettings[Enumerations.ConfigKeyXmlBatchAttibutes.XmlDocumentAttrImportFileName.ToString()];

        private static readonly XmlSerializerNamespaces _namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });


        public XmlInitializer(ILog logger)
        {
            _kofaxRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.KofaxRepositoryPath.ToString()];
            _dossiersTraitesRepositoryPath = ConfigurationManager.AppSettings[Enumerations.ConfigKeyPaths.DossiersTraitesRepositoryPath.ToString()];
            _logger = logger;
        }

        public void CreateXMLFile(string dirPath, string dirName)
        {
            List<Document> listDocument = new List<Document>();

            var files = Directory.GetFiles(dirPath);
            foreach (var file in files)
            {
                Pages pages = new Pages(new Page { ImportFileName = Path.GetFileName(file), OriginalFileName = Path.Combine(_kofaxRepositoryPath, Path.GetFileName(file)) });
                Document document = new Document(_xmlDocumentAttrImportFileName, pages);
                listDocument.Add(document);
            }

            Documents documents = new Documents(listDocument);

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
            Encoding encoding = Encoding.GetEncoding("utf-8");

            using (Stream streamKofax = new FileStream(Path.Combine(_kofaxRepositoryPath, string.Format("{0}.xml", dirName)), FileMode.Create))
            using (XmlWriter xmlWriterKofax = new XmlTextWriter(streamKofax, encoding){ Formatting = Formatting.Indented})
            {
                serializer.Serialize(xmlWriterKofax, importSession, _namespaces);
            }

            _logger.InfoFormat("Creation du fichier {0}.xml relatif au dossier {1} dont le chemin est {2}",
                    dirName,
                    dirName,
                    Path.Combine(_kofaxRepositoryPath, string.Format("{0}.xml", dirName)));

            using (Stream streamDossiersTraites = new FileStream(Path.Combine(_dossiersTraitesRepositoryPath, dirName, string.Format("{0}.xml", dirName)), FileMode.Create))
            using (XmlWriter xmlWriterDossiersTraite = new XmlTextWriter(streamDossiersTraites, encoding) { Formatting = Formatting.Indented })
            {
                serializer.Serialize(xmlWriterDossiersTraite, importSession, _namespaces);
            }

            _logger.InfoFormat("Creation du fichier {0}.xml relatif au dossier {1} dont le chemin est {2}",
                    dirName,
                    dirName,
                    Path.Combine(_dossiersTraitesRepositoryPath, dirName, string.Format("{0}.xml", dirName)));
        }

        public List<Page> DeserializeXmlFile(string dirPath)
        {
            List<Page> pageList = new List<Page>();
            ImportSession importSession = new ImportSession();

            XmlSerializer serializer = new XmlSerializer(typeof(ImportSession));

            StreamReader reader = new StreamReader(dirPath);
            importSession = (ImportSession)serializer.Deserialize(reader);
            reader.Close();

            var documents = importSession.Batches.Batche.Documents.Document.ToArray();

            foreach(var item in documents)
            {
                pageList.Add(item.Pages.Page);
            }

            return pageList;
        }
    }
}
