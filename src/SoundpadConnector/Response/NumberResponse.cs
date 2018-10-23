namespace SoundpadConnector.Response
{
    /// <summary>
    ///     Represents a <see cref="long"/> response
    /// </summary>
    public class NumberResponse : ResponseBase<long>
    {
        /// <inheritdoc />
        public override void Parse(string response)
        {
            if (response.StartsWith("R")) {
                ErrorMessage = response;
                IsSuccessful = false;
                return;
            }

            if (long.TryParse(response, out var result)) {
                Value = result;
            }
        }
    }
}
