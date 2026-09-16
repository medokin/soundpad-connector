using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class NuGetReleaseTests
    {
        private const string StableReadme = "dotnet add package SoundpadConnector --version 1.5.0";

        [Theory]
        [InlineData("v1.5.0", "1.5.0", StableReadme, true, true)]
        [InlineData("v1.4.0", "1.5.0", StableReadme, true, false)]
        [InlineData("v1.5.0-preview", "1.5.0", StableReadme, true, false)]
        [InlineData("v1.5.0", "1.5.0-dev", StableReadme, true, false)]
        [InlineData("v1.5.0", "1.4.0", StableReadme, true, false)]
        [InlineData("v1.5.0", "1.5.0", StableReadme + " Source remains 1.5.0-dev", true, false)]
        [InlineData("v1.5.0", "1.5.0", "Missing installation guidance", true, false)]
        [InlineData("v1.5.0", "1.5.0", StableReadme, false, false)]
        public async Task RequiresStableReviewedRelease(string tag, string version, string readme, bool onMaster, bool success)
        {
            var script = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../validate-nuget-release.ps1"));
            Assert.True(File.Exists(script), "Release preflight is missing");
            var tempRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()));
            var root = Path.Combine(tempRoot, "soundpad-nuget-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "src/SoundpadConnector"));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            try
            {
                File.WriteAllText(Path.Combine(root, "src/SoundpadConnector/SoundpadConnector.csproj"),
                    "<Project><PropertyGroup><Version>" + version + "</Version></PropertyGroup></Project>");
                File.WriteAllText(Path.Combine(root, "README.md"), readme);
                await RunAsync("git", root, timeout.Token, "init", "-b", "master");
                await RunAsync("git", root, timeout.Token, "-c", "commit.gpgsign=false", "-c", "user.name=Release Tests", "-c", "user.email=tests@example.invalid",
                    "commit", "--allow-empty", "-m", "test(release): create fixture");
                await RunAsync("git", root, timeout.Token, "update-ref", "refs/remotes/origin/master", "HEAD");
                if (!onMaster)
                {
                    await RunAsync("git", root, timeout.Token, "-c", "commit.gpgsign=false", "-c", "user.name=Release Tests", "-c", "user.email=tests@example.invalid",
                        "commit", "--allow-empty", "-m", "test(release): create unreviewed fixture");
                }

                var result = await ExecuteAsync("pwsh", root, timeout.Token, "-NoProfile", "-File", script, "-Tag", tag);

                Assert.True((result.ExitCode == 0) == success, result.Output);
                if (!success) Assert.Contains("Refusing NuGet publication", result.Output);
            }
            finally
            {
                if (!Path.GetFullPath(root).StartsWith(tempRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Unsafe release-test cleanup path");
                // Git marks loose objects read-only on Windows.
                foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                    File.SetAttributes(file, FileAttributes.Normal);
                Directory.Delete(root, true);
            }
        }

        private static async Task RunAsync(string file, string root, CancellationToken token, params string[] arguments)
        {
            var result = await ExecuteAsync(file, root, token, arguments);
            Assert.True(result.ExitCode == 0, result.Output);
        }

        private static async Task<(int ExitCode, string Output)> ExecuteAsync(string file, string root,
            CancellationToken token, params string[] arguments)
        {
            var startInfo = new ProcessStartInfo(file)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);
            using var process = Process.Start(startInfo);
            try
            {
                var output = process.StandardOutput.ReadToEndAsync(token);
                var errors = process.StandardError.ReadToEndAsync(token);
                await process.WaitForExitAsync(token);
                return (process.ExitCode, await output + await errors);
            }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                }
            }
        }
    }
}
