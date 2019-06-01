using System;
using System.Xml.Serialization;

namespace SoundpadConnector.XML
{
    [XmlRoot(ElementName = "Sound")]
    public class Sound {
        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }

        [XmlAttribute(AttributeName = "artist")]
        public string Artist { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }

        [XmlIgnore]
        public DateTime? AddedOn { get; set; }

        [XmlAttribute(AttributeName = "addedOn")]
        public string AddedOnString {
            get => AddedOn?.ToString("o");
            set {
                DateTime.TryParse(value, out var addedOn);
                AddedOn = addedOn;
            }
        }

        [XmlIgnore]
        public DateTime? LastPlayedOn { get; set; }

        [XmlAttribute(AttributeName = "lastPlayedOn")]
        public string LastPlayedOnString {
            get => LastPlayedOn?.ToString("o");
            set {
                DateTime.TryParse(value, out var lastPlayedOn);
                LastPlayedOn = lastPlayedOn;
            }
        }

        [XmlAttribute(AttributeName = "playCount")]
        public int PlayCount { get; set; }
    }
}