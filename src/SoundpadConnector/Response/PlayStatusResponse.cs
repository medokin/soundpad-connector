using System;

namespace SoundpadConnector.Response
{
    public class PlayStatusResponse : ResponseBase<PlayStatus>
    {
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
