using System;
using System.Threading;
using System.Threading.Tasks;
using SoundpadConnector.Response;

namespace Examples
{
    internal static class SoundlistPolling
    {
        internal static async Task<long> WaitForCountChangeAsync(Func<Task<NumberResponse>> getCount, long previousCount, CancellationToken token)
        {
            while (true)
            {
                var response = await getCount().WaitAsync(token);
                if (!response.IsSuccessful) throw new InvalidOperationException(response.ErrorMessage);
                if (response.Value != previousCount) return response.Value;
                await Task.Delay(100, token);
            }
        }
    }
}
