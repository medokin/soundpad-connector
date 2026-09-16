using System;
using System.IO;
using Markdig;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class ReadmeTests
    {
        [Fact]
        public void RendersPackageBannerWhenRawHtmlIsDisabled()
        {
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
            var readme = File.ReadAllText(Path.Combine(root, "README.md"));
            var pipeline = new MarkdownPipelineBuilder().DisableHtml().Build();

            var html = Markdown.ToHtml(readme, pipeline);

            Assert.Contains("<img src=\"https://raw.githubusercontent.com/medokin/soundpad-connector/4a9daae40d3c09dc11830c139ac89bcadd222207/docfx/images/SoundpadConnectorLogo.png\" alt=\"Logo SoundpadConnector .NET\"", html);
        }
    }
}
