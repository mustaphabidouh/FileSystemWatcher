using System;
using System.Xml.Serialization;

namespace FileSystemWatcher.Models
{
    [Serializable]
    public class Batches
    {
        [XmlElement(typeof(Batche))]
        public Batche Batche;

        public Batches()
        {
            Batche = new Batche();
        }

        public Batches(Batche batche)
        {
            Batche = batche;
        }
    }
    [Serializable]
    public class Batche
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Priority")]
        public string Priority { get; set; }
        [XmlAttribute("BatchClassName")]
        public string BatchClassName { get; set; }
        [XmlAttribute("Processed")]
        public string Processed { get; set; }
        [XmlAttribute("RelativeImageFilePath")]
        public string RelativeImageFilePath { get; set; }

        [XmlElement(typeof(Documents))]
        public Documents Documents { get; set; }

        public Batche()
        {
            Name = string.Empty;
            Priority = string.Empty;
            BatchClassName = string.Empty;
            Processed = string.Empty;
            RelativeImageFilePath = string.Empty;
            Documents = new Documents();
        }

        public Batche(
            string name,
            string priority,
            string batchClassName,
            string processed,
            string relativeImageFilePath,
            Documents documents
            )
        {
            Name = name;
            Priority = priority;
            BatchClassName = batchClassName;
            Processed = processed;
            RelativeImageFilePath = relativeImageFilePath;
            Documents = documents;
        }
    }
}
