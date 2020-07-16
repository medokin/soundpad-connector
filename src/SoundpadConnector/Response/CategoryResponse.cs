using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SoundpadConnector.XML;

namespace SoundpadConnector.Response
{
    /// <summary>
    ///     Represents a <see cref="Category"/> response
    /// </summary>
    public class CategoryResponse : ResponseBase<Category>
    {
        /// <inheritdoc />
        public override void Parse(string response)
        {
            var categoryList = new CategoryListResponse();
            categoryList.Parse(response);

            IsSuccessful = categoryList.IsSuccessful;
            ErrorMessage = categoryList.ErrorMessage;

            if (!IsSuccessful)
            {
                return;
            }

            Value = categoryList.Value.Categories.First();
        }
    }
}