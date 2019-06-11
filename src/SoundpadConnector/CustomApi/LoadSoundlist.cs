using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using SoundpadConnector.Response;

namespace SoundpadConnector.CustomApi
{
    public class LoadSoundlist
    {
        public async Task<NoContentResponse> Perform(string soundListPath)
        {
            var fileExists = File.Exists(soundListPath);
            var executablePath = GetSoundpadPath();

            if (!fileExists || executablePath == null)
            {
                return new NoContentResponse()
                {
                    IsSuccessful = false
                };
            }

            try
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = executablePath,
                        Arguments = Path.GetFullPath(soundListPath)
                    }
                };

                process.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            

            return new NoContentResponse()
            {
                IsSuccessful = true
            };
        }

        private string GetSoundpadPath()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"Soundpad\shell\open\command"))
            {

                if (key == null)
                {
                    return null;
                }

                var value = key.GetValue("").ToString();

                return value.TrimEnd(" -c \"%1\"".ToCharArray()).Trim('"');
            }
        }
    }
}
