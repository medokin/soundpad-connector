using System.Threading.Tasks;
using FluentAssertions;
using SoundpadConnector.CustomApi;
using Xunit;

namespace SoundpadConnector.IntegrationTests
{
    public class LoadSoundlistTests
    {
        [Fact]
        public async Task ShouldFailIfFileDoesNotExist()
        {
            var loadSoundList = new LoadSoundlist();

            var result = await loadSoundList.Perform("test.spl");

            result.IsSuccessful.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldLoadSoundList()
        {
            var loadSoundList = new LoadSoundlist();

            var result = await loadSoundList.Perform("Soundlist.spl");

            result.IsSuccessful.Should().BeTrue();
        }
    }
}