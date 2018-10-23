namespace SoundpadConnector.Response
{
    /// <summary>
    ///      Represents a <see cref="bool"/> response
    /// </summary>
    public class BooleanResponse : ResponseBase<bool> {

        /// <inheritdoc />
        public override void Parse(string response)
        {
            if (bool.TryParse(response, out var result)) {
                Value = result;
                IsSuccessful = true;
            }
            else
            {
                IsSuccessful = false;
                ErrorMessage = response;
            }
        }
    }
}
