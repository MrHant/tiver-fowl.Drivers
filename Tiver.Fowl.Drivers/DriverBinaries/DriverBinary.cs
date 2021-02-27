using System.IO;
using Tiver.Fowl.Drivers.Configuration;

namespace Tiver.Fowl.Drivers.DriverBinaries
{
    public class DriverBinary : IDriverBinary
    {
        public DriverBinary(string driverBinaryFilename)
        {
            DriverBinaryFilename = driverBinaryFilename;
        }

        public string DriverBinaryFilename { get; private set; }

        public bool CheckBinaryExists()
        {
            return File.Exists(DriverBinaryFilepath);
        }

        public string GetExistingBinaryVersion()
        {
            if (!File.Exists(DriverBinaryVersionFilepath))
            {
                return null;
            }

            using var stream = File.OpenText(DriverBinaryVersionFilepath);
            return stream.ReadToEnd();
        }

        public void RemoveBinaryFiles()
        {
            File.Delete(DriverBinaryFilepath);
            File.Delete(DriverBinaryVersionFilepath);
        }

        private string DriverBinaryFilepath => Path.Combine(Context.Configuration.DownloadLocation, DriverBinaryFilename);
        public string DriverBinaryVersionFilepath => Path.Combine(Context.Configuration.DownloadLocation, $"{DriverBinaryFilename}.version");
    }
}
