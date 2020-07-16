using System.IO;
using System.Text;
using System.Xml.Serialization;
using SoundpadConnector.XML;

namespace SoundpadConnector.Response
{
    /// <summary>
    ///     Represents a <see cref="CategoryList"/> response
    /// </summary>
    public class CategoryListResponse : ResponseBase<CategoryList>
    {
        /// <inheritdoc />
        public override void Parse(string response)
        {
            if (response.StartsWith("R"))
            {
                ErrorMessage = response;
                IsSuccessful = false;
            }
            var deserializer = new XmlSerializer(typeof(CategoryList));
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(response));
            Value = (CategoryList)deserializer.Deserialize(resultStream);
            IsSuccessful = true;
        }
    }
}