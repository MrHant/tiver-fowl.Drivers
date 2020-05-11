using System;
using Tiver.Fowl.Drivers.DriverBinaries;

namespace Tiver.Fowl.Drivers.DriverDownloaders
{
    public interface IDriverDownloader
    {
        IDriverBinary Binary { get; set; }
        Uri LinkForDownloadsPage { get; }

        /// <summary>
        /// Download binary of driver
        /// Create .version file with version as well
        /// </summary>
        /// <param name="versionNumber">Version to be downloaded</param>
        /// <param name="platform">Target platform for driverb</param>
        /// <returns>Status whether download was successful or not</returns>
        DownloadResult DownloadBinary(string versionNumber, string platform);
    }
}
