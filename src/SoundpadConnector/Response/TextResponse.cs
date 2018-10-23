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
            Value = response;
            IsSuccessful = true;
        }
    }
}
