using System;

namespace SoundpadConnector.Response
{
    /// <summary>
    ///     Represents a <see cref="PlayStatus"/> response
    /// </summary>
    public class PlayStatusResponse : ResponseBase<PlayStatus>
    {
        /// <inheritdoc />
        public override void Parse(string response)
        {
            if (Enum.TryParse<PlayStatus>(response, true, out var status)) {
                Value = status;
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
