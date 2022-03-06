using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FileSystemWatcher.Models
{
    [Serializable]
    [XmlRoot("ImportSession")]
    public class ImportSession
    {
        [XmlElement(typeof(Batches))]
        public Batches Batches { get; set; }
        public ImportSession()
        {
            Batches = new Batches();
        }
        public ImportSession(Batches batches)
        {
            Batches = batches;
        }
    }
}
