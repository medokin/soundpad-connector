namespace SoundpadConnector.Response
{
    public class NoContentResponse : IResponse
    {
        public bool IsSuccessful { get; private set; }

        public virtual void Parse(string response)
        {
            IsSuccessful = response.StartsWith("R-200");
        }
    }
}