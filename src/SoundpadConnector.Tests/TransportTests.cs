using System.IO.Pipes;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SoundpadConnector.Tests
{
    [SupportedOSPlatform("windows")]
    public class TransportTests
    {
        [Fact]
        public async Task ReadsLargeUtf8ResponseWithoutClosingConnection()
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var text = new string('a', 4095) + "🔊 Grüße";
            var serve = RespondAsync(fixture, "GetRemoteControlVersion()", text);

            var response = await soundpad.GetVersion().WaitAsync(fixture.Token);
            await serve;

            Assert.Equal(text, response.Value);
        }

        [Fact]
        public async Task ReadsSoundlistLargerThanPipeBuffer()
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var title = new string('x', 10000);
            var xml = "<Soundlist><Sound index=\"1\" title=\"" + title + "\" /></Soundlist>";
            var serve = RespondAsync(fixture, "GetSoundlist()", xml);

            var response = await soundpad.GetSoundlist().WaitAsync(fixture.Token);
            await serve;

            Assert.Equal(title, Assert.Single(response.Value.Sounds).Title);
        }

        [Fact]
        public async Task KeepsQueuedResponsesInSeparateMessages()
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var serve = Task.Run(async () =>
            {
                await ReadRequestAsync(fixture, "GetRemoteControlVersion()");
                await fixture.Server.WriteAsync(Encoding.UTF8.GetBytes("4.0.0"), fixture.Token);
                await fixture.Server.WriteAsync(Encoding.UTF8.GetBytes("4.0.1"), fixture.Token);
                await ReadRequestAsync(fixture, "GetRemoteControlVersion()");
            });

            var first = await soundpad.GetVersion().WaitAsync(fixture.Token);
            var second = await soundpad.GetVersion().WaitAsync(fixture.Token);
            await serve;

            Assert.Equal("4.0.0", first.Value);
            Assert.Equal("4.0.1", second.Value);
        }

        private static async Task RespondAsync(PipeFixture fixture, string request, string response)
        {
            await ReadRequestAsync(fixture, request);
            await fixture.Server.WriteAsync(Encoding.UTF8.GetBytes(response), fixture.Token);
        }

        [Fact]
        public async Task PreservesLegacyBytePipeResponses()
        {
            await using var fixture = await PipeFixture.ConnectAsync();
            using var soundpad = new Soundpad(fixture.Client);
            var serve = RespondAsync(fixture, "GetRemoteControlVersion()", "1.1.2");

            var response = await soundpad.GetVersion().WaitAsync(fixture.Token);
            await serve;

            Assert.Equal("1.1.2", response.Value);
        }

        [Fact]
        public async Task ReadsBytePipeResponseWithDefaultBufferSize()
        {
            await using var fixture = await PipeFixture.ConnectAsync(bufferSize: 0);
            using var soundpad = new Soundpad(fixture.Client);
            var serve = RespondAsync(fixture, "GetRemoteControlVersion()", "1.1.2");

            var response = await soundpad.GetVersion().WaitAsync(fixture.Token);
            await serve;

            Assert.Equal("1.1.2", response.Value);
        }

        [Fact]
        public async Task PreservesLargeLegacyBytePipeResponses()
        {
            await using var fixture = await PipeFixture.ConnectAsync(bufferSize: 16384);
            using var soundpad = new Soundpad(fixture.Client);
            var text = new string('a', 8000);
            var serve = RespondAsync(fixture, "GetRemoteControlVersion()", text);

            var response = await soundpad.GetVersion().WaitAsync(fixture.Token);
            await serve;

            Assert.Equal(text, response.Value);
        }

        [Fact]
        public async Task ReportsDisconnectInsteadOfSuccessfulEmptyResponse()
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var serve = Task.Run(async () =>
            {
                await ReadRequestAsync(fixture, "GetRemoteControlVersion()");
                fixture.Server.Dispose();
            });

            await Assert.ThrowsAnyAsync<IOException>(() => soundpad.GetVersion().WaitAsync(fixture.Token));
            await serve;
        }

        [Fact]
        public async Task SerializesConcurrentRequests()
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var serve = Task.Run(async () =>
            {
                await RespondAsync(fixture, "GetRemoteControlVersion()", "first");
                await RespondAsync(fixture, "GetRemoteControlVersion()", "second");
            });

            var first = soundpad.GetVersion();
            var second = soundpad.GetVersion();
            await Task.WhenAll(serve, first, second).WaitAsync(fixture.Token);

            Assert.Equal("first", (await first).Value);
            Assert.Equal("second", (await second).Value);
        }

        private static async Task ReadRequestAsync(PipeFixture fixture, string expected)
        {
            var bytes = new byte[Encoding.UTF8.GetByteCount(expected)];
            await fixture.Server.ReadExactlyAsync(bytes, fixture.Token);
            Assert.Equal(expected, Encoding.UTF8.GetString(bytes));
        }

    }
}
