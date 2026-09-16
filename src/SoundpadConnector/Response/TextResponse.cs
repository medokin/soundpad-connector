namespace SoundpadConnector.Response
{
    /// <summary>
    ///     Represents a <see cref="string"/> response
    /// </summary>
    public class TextResponse : ResponseBase<string>
    {
        /// <inheritdoc />
        public override void Parse(string response)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(response, "^R-[0-9]{3}(:|$)"))
            {
                IsSuccessful = false;
                ErrorMessage = response;
                return;
            }
            Value = response;
            IsSuccessful = true;
        }
    }
}
