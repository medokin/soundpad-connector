namespace SoundpadConnector.Response {
    /// <summary>
    ///     Base class for reponses containing a return value
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class ResponseBase<TResult> : IResponse
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Response value
        /// </summary>
        public TResult Value { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <inheritdoc />
        public abstract void Parse(string response);
    }
}
