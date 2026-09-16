using System.Text;
using System.Threading.Tasks;
using SoundpadConnector.Response;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class PipeResponseTests
    {
        [Fact]
        public async Task ParsesUtf8SoundlistReceivedInSingleByteFragments()
        {
            await using var fixture = await PipeFixture.ConnectAsync();
            const string xml = "<Soundlist><Sound index=\"1\" title=\"Grüße 🔊\" /></Soundlist>";
            var write = fixture.WriteResponseAsync(xml, 1);
            var bytes = new byte[Encoding.UTF8.GetByteCount(xml)];

            await fixture.Client.ReadExactlyAsync(bytes, fixture.Token);
            await write;
            var response = new SoundlistResponse();
            response.Parse(Encoding.UTF8.GetString(bytes));

            Assert.True(response.IsSuccessful);
            Assert.Equal("Grüße 🔊", Assert.Single(response.Value.Sounds).Title);
        }

        [Fact]
        public async Task ParsesLargeSoundlistWhileConnectionRemainsOpen()
        {
            await using var fixture = await PipeFixture.ConnectAsync();
            var xml = new StringBuilder("<Soundlist>");
            for (var index = 1; index <= 1000; index++)
            {
                xml.Append($"<Sound index=\"{index}\" title=\"Sound {index}\" />");
            }
            xml.Append("</Soundlist>");
            var text = xml.ToString();
            var write = fixture.WriteResponseAsync(text, 128);
            var bytes = new byte[Encoding.UTF8.GetByteCount(text)];

            await fixture.Client.ReadExactlyAsync(bytes, fixture.Token);
            await write;
            var response = new SoundlistResponse();
            response.Parse(Encoding.UTF8.GetString(bytes));

            Assert.True(response.IsSuccessful);
            Assert.Equal(1000, response.Value.Sounds.Count);
            Assert.Equal(1, response.Value.Sounds[0].Index);
            Assert.Equal("Sound 1000", response.Value.Sounds[999].Title);
        }

        [Fact]
        public async Task ParsesSuccessiveResponsesBeforeServerDisconnects()
        {
            await using var fixture = await PipeFixture.ConnectAsync();
            var firstWrite = fixture.WriteResponseAsync("4.0.0", 2);
            var versionBytes = new byte[5];
            await fixture.Client.ReadExactlyAsync(versionBytes, fixture.Token);
            await firstWrite;
            var version = new TextResponse();
            version.Parse(Encoding.UTF8.GetString(versionBytes));

            var secondWrite = fixture.WriteResponseAsync("42", 1);
            var countBytes = new byte[2];
            await fixture.Client.ReadExactlyAsync(countBytes, fixture.Token);
            await secondWrite;
            var count = new NumberResponse();
            count.Parse(Encoding.UTF8.GetString(countBytes));

            Assert.Equal("4.0.0", version.Value);
            Assert.Equal(42, count.Value);
            fixture.Server.Dispose();
            Assert.Equal(0, await fixture.Client.ReadAsync(new byte[1], fixture.Token));
        }
    }
}
