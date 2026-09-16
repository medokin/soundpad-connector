using System;
using System.Threading;
using System.Threading.Tasks;
using Examples;
using SoundpadConnector.Response;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class PollingExampleTests
    {
        [Fact]
        public async Task WaitsUntilCountActuallyChanges()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var calls = 0;

            var count = await SoundlistPolling.WaitForCountChangeAsync(
                () => Task.FromResult(new NumberResponse { IsSuccessful = true, Value = ++calls == 1 ? 9 : 10 }), 9, timeout.Token);

            Assert.Equal(10, count);
            Assert.Equal(2, calls);
        }

        [Fact]
        public async Task StopsWhenCountRequestFails()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => SoundlistPolling.WaitForCountChangeAsync(
                () => Task.FromResult(new NumberResponse { ErrorMessage = "R-500: Failed" }), 9, timeout.Token));

            Assert.Equal("R-500: Failed", error.Message);
        }

        [Fact]
        public async Task UnchangedCountCanBeCanceled()
        {
            using var cancellation = new CancellationTokenSource();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SoundlistPolling.WaitForCountChangeAsync(() =>
            {
                cancellation.Cancel();
                return Task.FromResult(new NumberResponse { IsSuccessful = true, Value = 9 });
            }, 9, cancellation.Token));
        }
    }
}
