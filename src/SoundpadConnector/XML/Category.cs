using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoundpadConnector.XML
{
    [XmlRoot(ElementName = "Category")]
    public class Category
    {
        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public int Type { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "icon")]
        public string Icon { get; set; }

        [XmlElement(ElementName = "Sound")]
        public List<Sound> Sounds { get; set; }
    }
}