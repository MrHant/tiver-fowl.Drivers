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
            return File.Exists(Path.Combine(_config.DownloadLocation, DriverBinaryFilename));
        }

        public string GetExistingBinaryVersion()
        {
            var versionFilepath = Path.Combine(_config.DownloadLocation, $"{DriverBinaryFilename}.version");
            if (!File.Exists(versionFilepath))
            {
                return null;
            }

            using (var stream = File.OpenText(versionFilepath))
            {
                return stream.ReadToEnd();
            }
        }

        readonly IDriversConfiguration _config = (DriversConfigurationSection)ConfigurationManager.GetSection("driversConfigurationGroup/driversConfiguration");
    }
}
