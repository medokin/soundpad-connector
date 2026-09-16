using System;
using System.Linq;
using SoundpadConnector.IntegrationTests;
using Xunit;

namespace SoundpadConnector.Tests
{
    [CollectionDefinition("Integration environment", DisableParallelization = true)]
    public class IntegrationEnvironmentCollection { }

    [Collection("Integration environment")]
    public class IntegrationTestSafetyTests : IDisposable
    {
        private readonly string _previous = Environment.GetEnvironmentVariable("SOUNDPAD_INTEGRATION_TESTS");

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("true")]
        public void LiveTestsAreSkippedWithoutExplicitOptIn(string enabled)
        {
            Environment.SetEnvironmentVariable("SOUNDPAD_INTEGRATION_TESTS", enabled);

            var facts = GetLiveFacts();

            Assert.NotEmpty(facts);
            Assert.All(facts, fact => Assert.False(string.IsNullOrEmpty(fact.Skip)));
        }

        [Fact]
        public void LiveTestsAreEnabledWithExplicitOptIn()
        {
            Environment.SetEnvironmentVariable("SOUNDPAD_INTEGRATION_TESTS", "1");

            var facts = GetLiveFacts();

            Assert.NotEmpty(facts);
            Assert.All(facts, fact => Assert.Null(fact.Skip));
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("SOUNDPAD_INTEGRATION_TESTS", _previous);
        }

        private static FactAttribute[] GetLiveFacts()
        {
            return typeof(LoadSoundlistTests).Assembly.GetTypes()
                .SelectMany(type => type.GetMethods())
                .SelectMany(method => method.GetCustomAttributes(typeof(FactAttribute), true))
                .Cast<FactAttribute>().ToArray();
        }
    }
}
