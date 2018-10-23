namespace SoundpadConnector.Response
{
    /// <summary>
    ///     Responce interface
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        ///     Indicates if the request was successful
        /// </summary>
        bool IsSuccessful { get; set; }

        /// <summary>
        ///     Parses Soundpad's text response
        /// </summary>
        /// <param name="response"></param>
        void Parse(string response);
    }
}