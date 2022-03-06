using System;
using System.Xml.Serialization;

namespace FileSystemWatcher.Models
{
    [Serializable]
    public class Pages
    {
        [XmlElement(typeof(Page))]
        public Page Page { get; set; }

        public Pages()
        {
            Page = new Page();
        }
        public Pages(Page page)
        {
            Page = page;
        }
    }

    [Serializable]
    public class Page
    {
        [XmlAttribute("ImportFileName")]
        public string ImportFileName { get; set; }
        [XmlAttribute("OriginalFileName")]
        public string OriginalFileName { get; set; }

        public Page()
        {
            ImportFileName = string.Empty;
            OriginalFileName = string.Empty;
        }
        public Page(string importFileName, string originalFileName)
        {
            ImportFileName = importFileName;
            OriginalFileName = originalFileName;
        }
    }
}
