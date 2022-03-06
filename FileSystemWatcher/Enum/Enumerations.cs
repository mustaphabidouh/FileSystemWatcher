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
            XmlBatchAttrImportFileName
        }
        public enum ConfigPrefix
        { DossiersEnErreurPrefix }
    }
}
