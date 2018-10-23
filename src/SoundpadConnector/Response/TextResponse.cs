namespace SoundpadConnector.Response
{
    public class TextResponse : ResponseBase<string>
    {
        public override void Parse(string response)
        {
            Value = response;
            IsSuccessful = true;
        }
    }
}
