using System.Configuration;
using System.IO;
using Tiver.Fowl.Drivers.Configuration;

namespace Tiver.Fowl.Drivers.Binaries
{
    public class ChromeDriverBinary : IDriverBinary
    {
        public string DriverBinaryFilename => "chromedriver.exe";

        public bool CheckBinaryExists()
        {
            return File.Exists(Path.Combine(Config.DownloadLocation, DriverBinaryFilename));
        }

        public string GetExistingBinaryVersion()
        {
            var versionFilepath = Path.Combine(Config.DownloadLocation, $"{DriverBinaryFilename}.version");
            if (!File.Exists(versionFilepath))
            {
                return null;
            }

            using (var stream = File.OpenText(versionFilepath))
            {
                return stream.ReadToEnd();
            }
        }

        private static readonly IDriversConfiguration Config = ConfigurationReader.ReadFromFileOrDefault();
    }
}
