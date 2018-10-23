namespace SoundpadConnector.Response {
    public abstract class ResponseBase<TResult> : IResponse
    {
        public bool IsSuccessful { get; set; }
        public TResult Value { get; set; }
        public string ErrorMessage { get; set; }
        public abstract void Parse(string response);
    }
}
