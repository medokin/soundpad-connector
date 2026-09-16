using System.Threading.Tasks;
using FluentAssertions;
using SoundpadConnector.CustomApi;

namespace SoundpadConnector.IntegrationTests
{
    public class LoadSoundlistTests
    {
        [SoundpadFact]
        public async Task ShouldFailIfFileDoesNotExist()
        {
            var loadSoundList = new LoadSoundlist();

            var result = await loadSoundList.Perform("test.spl");

            result.IsSuccessful.Should().BeFalse();
        }

        [SoundpadFact]
        public async Task ShouldLoadSoundList()
        {
            var loadSoundList = new LoadSoundlist();

            var result = await loadSoundList.Perform("Soundlist.spl");

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
