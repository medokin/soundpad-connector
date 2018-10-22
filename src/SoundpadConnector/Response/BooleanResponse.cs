namespace SoundpadConnector.Response
{
    public class BooleanResponse : ResponseBase<bool> {
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
