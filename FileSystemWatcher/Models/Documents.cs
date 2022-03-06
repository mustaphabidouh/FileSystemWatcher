using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FileSystemWatcher.Models
{
    [Serializable]
    public class Documents
    {
        [XmlAttribute("FormTypeName")]
        public string FormTypeName { get; set; }

        [XmlElement(typeof(Pages))]
        public List<Pages> Pages { get; set; }

        public Documents()
        {
            FormTypeName = string.Empty;
            Pages = new List<Pages>();
        }

        public Documents(string formTypeName, List<Pages> pages)
        {
            FormTypeName = formTypeName;
            Pages = pages;
        }

        [XmlInclude(typeof(Pages))]
        public void Add(Pages i)
        {
            Pages.Add(i);
        }
    }
}
