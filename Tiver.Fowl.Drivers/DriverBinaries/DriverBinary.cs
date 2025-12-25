using System.IO;
using System.Linq;

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
            return stream.ReadLine();
        }

        public string[] GetExtractedFiles()
        {
            if (!File.Exists(DriverBinaryVersionFilepath))
            {
                return new string[0];
            }

            var lines = File.ReadAllLines(DriverBinaryVersionFilepath);
            // Skip first line (version) and second line (empty line)
            return lines.Length > 2 ? lines.Skip(2).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray() : new string[0];
        }

        public void RemoveBinaryFiles()
        {
            // Delete all extracted files tracked in version file
            var extractedFiles = GetExtractedFiles();
            foreach (var file in extractedFiles)
            {
                var filePath = Path.Combine(Context.Configuration.DownloadLocation, file);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Delete version file
            if (File.Exists(DriverBinaryVersionFilepath))
            {
                File.Delete(DriverBinaryVersionFilepath);
            }
        }

        private string DriverBinaryFilepath => Path.Combine(Context.Configuration.DownloadLocation, DriverBinaryFilename);
        public string DriverBinaryVersionFilepath => Path.Combine(Context.Configuration.DownloadLocation, $"{DriverBinaryFilename}.version");
        public string DriverBinaryLockFilepath => Path.Combine(Context.Configuration.DownloadLocation, $"{DriverBinaryFilename}.lock");
    }
}
