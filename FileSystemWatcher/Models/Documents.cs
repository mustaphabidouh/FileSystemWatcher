using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FileSystemWatcher.Models
{
    [Serializable]
    public class Documents
    {
        [XmlElement(typeof(Document))]
        public List<Document> Document { get; set; }

        public Documents()
        {
            Document = new List<Document>();
        }

        public Documents(List<Document> document)
        {
            Document = document;
        }

        [XmlInclude(typeof(Document))]
        public void Add(Document i)
        {
            Document.Add(i);
        }
    }

    [Serializable]
    public class Document
    {
        [XmlAttribute("FormTypeName")]
        public string FormTypeName { get; set; }

        [XmlElement(typeof(Pages))]
        public Pages Pages { get; set; }

        public Document()
        {
            FormTypeName = string.Empty;
            Pages = new Pages();
        }

        public Document(string formTypeName, Pages pages)
        {
            FormTypeName = formTypeName;
            Pages = pages;
        }
    }
}
