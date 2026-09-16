using System;
using Xunit;

namespace SoundpadConnector.IntegrationTests
{
    public class SoundpadFactAttribute : FactAttribute
    {
        public SoundpadFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("SOUNDPAD_INTEGRATION_TESTS") != "1")
            {
                Skip = "Set SOUNDPAD_INTEGRATION_TESTS=1 to allow tests to launch Soundpad and load a soundlist.";
            }
        }
    }
}
