using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoundpadConnector.XML
{
    [XmlRoot(ElementName = "Categories")]
    public class CategoryList
    {
        [XmlElement(ElementName = "Category")]
        public List<Category> Categories { get; set; }
    }
}