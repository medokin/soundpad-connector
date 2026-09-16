using System.IO.Pipes;
using System;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using SoundpadConnector.Response;
using Xunit;

namespace SoundpadConnector.Tests
{
    [SupportedOSPlatform("windows")]
    public class ApiCommandTests
    {
        [Theory]
        [InlineData("GetMainFrameTitleText", "GetTitleText()", "Soundpad - Grüße 🔊")]
        [InlineData("GetSoundpadVersion", "GetVersion()", "4.0.30")]
        [InlineData("GetVersion", "GetRemoteControlVersion()", "1.1.2")]
        [InlineData("GetSoundpadVersion", "GetVersion()", "R-500: Failed")]
        public async Task SendsCanonicalTextRequest(string methodName, string request, string reply)
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var serve = RespondAsync(fixture, request, reply);

            var call = methodName switch
            {
                "GetMainFrameTitleText" => soundpad.GetMainFrameTitleText(),
                "GetSoundpadVersion" => soundpad.GetSoundpadVersion(),
                "GetVersion" => soundpad.GetVersion(),
                _ => throw new ArgumentException("Unknown test command", nameof(methodName))
            };
            await serve;
            var response = await call.WaitAsync(fixture.Token);

            Assert.Equal(!reply.StartsWith("R-"), response.IsSuccessful);
            if (response.IsSuccessful) Assert.Equal(reply, response.Value);
            else Assert.Equal(reply, response.ErrorMessage);
        }

        [Theory]
        [InlineData(-1000, "DoSeekMs(-1000)", "R-200", true)]
        [InlineData(0, "DoSeekMs(0)", "R-200", true)]
        [InlineData(5000, "DoSeekMs(5000)", "R-200", true)]
        [InlineData(5000, "DoSeekMs(5000)", "R-500: Failed", false)]
        public async Task SeeksToAbsolutePosition(int milliseconds, string request, string reply, bool success)
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var serve = RespondAsync(fixture, request, reply);

            var call = soundpad.Seek(milliseconds);
            await serve;
            var response = await call.WaitAsync(fixture.Token);

            Assert.Equal(success, response.IsSuccessful);
        }

        [Theory]
        [InlineData(2, 5, true, false, "DoPlaySoundFromCategory(2, 5, True, False)", "R-200", true)]
        [InlineData(-1, 7, false, true, "DoPlaySoundFromCategory(-1, 7, False, True)", "R-200", true)]
        [InlineData(2, 5, true, true, "DoPlaySoundFromCategory(2, 5, True, True)", "R-500: Failed", false)]
        public async Task PlaysCategoryRelativePosition(int categoryIndex, int soundIndex, bool renderLine, bool captureLine,
            string request, string reply, bool success)
        {
            await using var fixture = await PipeFixture.ConnectAsync(PipeTransmissionMode.Message);
            using var soundpad = new Soundpad(fixture.Client);
            var serve = RespondAsync(fixture, request, reply);

            var call = soundpad.PlaySoundFromCategory(categoryIndex, soundIndex, renderLine, captureLine);
            await serve;
            var response = await call.WaitAsync(fixture.Token);

            Assert.Equal(success, response.IsSuccessful);
        }

        private static async Task RespondAsync(PipeFixture fixture, string request, string response)
        {
            var bytes = new byte[2048];
            var length = await fixture.Server.ReadAsync(bytes, fixture.Token);
            Assert.True(fixture.Server.IsMessageComplete);
            Assert.Equal(request, Encoding.UTF8.GetString(bytes, 0, length));
            await fixture.Server.WriteAsync(Encoding.UTF8.GetBytes(response), fixture.Token);
        }
    }
}
