namespace FileSystemWatcher.Enum
{
    public static class Enumerations
    {
        public enum ConfigKeyPaths
        {
            KofaxRepositoryPath,
            KofaxErrorsRepositoryPath,
            DossiersEnAttenteRepositoryPath,
            DossiersEnErreurRepositoryPath,
            DossiersTraitesRepositoryPath,
        }

        public enum ConfigKeyXmlBatchAttibutes
        {
            XmlBatchAttrName,
            XmlBatchAttrPriority,
            XmlBatchAttrBatchClassName,
            XmlBatchAttrProcessed,
            XmlBatchAttrRelativeImageFilePath,
            XmlDocumentAttrImportFileName
        }
        public enum ConfigPrefix
        { DossiersEnErreurPrefix }

        public enum FileSourceAndTypeEvent
        { None, DossiersEnAttente_DirWithFile, DossiersEnAttente_DirWithoutFile, KofaxErrors_XmlFile }
    }
}
