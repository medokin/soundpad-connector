using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoundpadConnector.XML
{
    [XmlRoot(ElementName = "Soundlist")]
    public class Soundlist
    {
        [XmlAttribute(AttributeName = "rel")]
        public string Rel => "true";

        [XmlElement(ElementName = "Sound")]
        public List<Sound> Sounds { get; set; }
    }
}