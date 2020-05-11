using System.IO;
using Tiver.Fowl.Drivers.Configuration;

namespace Tiver.Fowl.Drivers.DriverBinaries
{
    public class ChromeDriverBinary : IDriverBinary
    {
        public string DriverBinaryFilename => "chromedriver.exe";

        public bool CheckBinaryExists()
        {
            return File.Exists(DriverBinaryFilepath);
        }

        public string GetExistingBinaryVersion()
        {
            var versionFilepath = DriverBinaryVersionFilepath;
            if (!File.Exists(versionFilepath))
            {
                return null;
            }

            using (var stream = File.OpenText(versionFilepath))
            {
                return stream.ReadToEnd();
            }
        }

        public void RemoveBinaryFiles()
        {
            File.Delete(DriverBinaryFilepath);
            File.Delete(DriverBinaryVersionFilepath);
        }

        private string DriverBinaryFilepath => Path.Combine(Context.Configuration.DownloadLocation, DriverBinaryFilename);
        private string DriverBinaryVersionFilepath => Path.Combine(Context.Configuration.DownloadLocation, $"{DriverBinaryFilename}.version");
    }
}
