namespace SoundpadConnector.Response
{
    /// <summary>
    ///     Represents a response without return value
    /// </summary>
    public class NoContentResponse : IResponse
    {
        /// <inheritdoc />
        public bool IsSuccessful { get; set; }

        /// <inheritdoc />
        public virtual void Parse(string response)
        {
            IsSuccessful = response.StartsWith("R-200");
        }
    }
}