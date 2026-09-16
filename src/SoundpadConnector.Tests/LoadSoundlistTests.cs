using System.IO;
using SoundpadConnector.CustomApi;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class SoundlistLaunchTests
    {
        [Theory]
        [InlineData("C:/sound lists/my sounds.spl")]
        [InlineData("C:/sounds/Grüße.spl")]
        [InlineData("relative folder/sounds.spl")]
        public void PassesSoundlistAsOneQuotedArgument(string path)
        {
            var info = LoadSoundlist.CreateStartInfo("C:/Program Files/Soundpad/Soundpad.exe", path);

            Assert.Equal("C:/Program Files/Soundpad/Soundpad.exe", info.FileName);
            Assert.Equal("\"" + Path.GetFullPath(path) + "\"", info.Arguments);
        }
    }
}
