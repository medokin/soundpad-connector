using SoundpadConnector.Response;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class ResponseParserTests
    {
        [Theory]
        [InlineData("true", true)]
        [InlineData("False", false)]
        public void ParsesBoolean(string text, bool expected)
        {
            var response = new BooleanResponse();

            response.Parse(text);

            Assert.True(response.IsSuccessful);
            Assert.Equal(expected, response.Value);
        }

        [Theory]
        [InlineData("R-500: Failed")]
        [InlineData("not a boolean")]
        public void RejectsInvalidBoolean(string text)
        {
            var response = new BooleanResponse();

            response.Parse(text);

            Assert.False(response.IsSuccessful);
            Assert.Equal(text, response.ErrorMessage);
        }

        [Theory]
        [InlineData("R-200", true)]
        [InlineData("R-200: Success", true)]
        [InlineData("R-500: Failed", false)]
        public void ParsesCommandStatus(string text, bool expected)
        {
            var response = new NoContentResponse();

            response.Parse(text);

            Assert.Equal(expected, response.IsSuccessful);
        }

        [Theory]
        [InlineData("0", 0L)]
        [InlineData("42", 42L)]
        [InlineData("-1", -1L)]
        [InlineData("9223372036854775807", long.MaxValue)]
        public void ParsesNumericValue(string text, long expected)
        {
            var response = new NumberResponse();

            response.Parse(text);

            Assert.True(response.IsSuccessful);
            Assert.Equal(expected, response.Value);
        }

        [Fact]
        public void PreservesNumericError()
        {
            var response = new NumberResponse();

            response.Parse("R-500: Failed");

            Assert.False(response.IsSuccessful);
            Assert.Equal("R-500: Failed", response.ErrorMessage);
        }

        [Theory]
        [InlineData("not a number")]
        [InlineData("9223372036854775808")]
        public void RejectsInvalidNumber(string text)
        {
            var response = new NumberResponse();

            response.Parse(text);

            Assert.False(response.IsSuccessful);
            Assert.Equal(text, response.ErrorMessage);
        }

        [Fact]
        public void PreservesSoundlistError() => AssertXmlError(new SoundlistResponse());

        [Fact]
        public void PreservesCategoryListError() => AssertXmlError(new CategoryListResponse());

        [Fact]
        public void PreservesCategoryError() => AssertXmlError(new CategoryResponse());

        private static void AssertXmlError<T>(ResponseBase<T> response)
        {
            response.Parse("R-500: Failed");

            Assert.False(response.IsSuccessful);
            Assert.Equal("R-500: Failed", response.ErrorMessage);
        }

        [Theory]
        [InlineData("stopped", PlayStatus.Stopped)]
        [InlineData("Playing", PlayStatus.Playing)]
        [InlineData("PAUSED", PlayStatus.Paused)]
        [InlineData("seeking", PlayStatus.Seeking)]
        public void ParsesPlaybackStatus(string text, PlayStatus expected)
        {
            var response = new PlayStatusResponse();

            response.Parse(text);

            Assert.True(response.IsSuccessful);
            Assert.Equal(expected, response.Value);
        }

        [Fact]
        public void RejectsUnknownPlaybackStatus()
        {
            var response = new PlayStatusResponse();

            response.Parse("unknown");

            Assert.False(response.IsSuccessful);
            Assert.Equal("unknown", response.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Soundpad 4: Grüße 🔊")]
        public void PreservesText(string text)
        {
            var response = new TextResponse();

            response.Parse(text);

            Assert.True(response.IsSuccessful);
            Assert.Equal(text, response.Value);
        }

        [Fact]
        public void ParsesSoundlistFields()
        {
            var response = new SoundlistResponse();

            response.Parse("<Soundlist><Sound index=\"3\" url=\"C:/sounds/test.wav\" artist=\"Artist\" title=\"Sound\" duration=\"00:02\" playCount=\"7\" /></Soundlist>");

            Assert.True(response.IsSuccessful);
            var sound = Assert.Single(response.Value.Sounds);
            Assert.Equal(3, sound.Index);
            Assert.Equal("C:/sounds/test.wav", sound.Url);
            Assert.Equal("Artist", sound.Artist);
            Assert.Equal("Sound", sound.Title);
            Assert.Equal("00:02", sound.Duration);
            Assert.Equal(7, sound.PlayCount);
        }

        [Fact]
        public void ParsesCategoriesWithSounds()
        {
            var response = new CategoryListResponse();

            response.Parse("<Categories><Category index=\"2\" type=\"1\" name=\"Favorites\" icon=\"star\"><Sound index=\"3\" title=\"Sound\" /></Category></Categories>");

            Assert.True(response.IsSuccessful);
            var category = Assert.Single(response.Value.Categories);
            Assert.Equal(2, category.Index);
            Assert.Equal(1, category.Type);
            Assert.Equal("Favorites", category.Name);
            Assert.Equal("star", category.Icon);
            Assert.Equal(3, Assert.Single(category.Sounds).Index);
        }

        [Fact]
        public void ParsesSingleCategory()
        {
            var response = new CategoryResponse();

            response.Parse("<Categories><Category index=\"2\" name=\"Favorites\" /></Categories>");

            Assert.True(response.IsSuccessful);
            Assert.Equal(2, response.Value.Index);
            Assert.Equal("Favorites", response.Value.Name);
        }
    }
}
