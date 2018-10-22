namespace SoundpadConnector.Response
{
    public class NumberResponse : ResponseBase<long>
    {
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
