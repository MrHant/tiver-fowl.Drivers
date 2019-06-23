using System;
using Tiver.Fowl.Drivers.DriverBinaries;

namespace Tiver.Fowl.Drivers.DriverDownloaders
{
    public interface IDriverDownloader
    {
        IDriverBinary Binary { get; }
        Uri LinkForDownloadsPage { get; }

        /// <summary>
        /// Download binary of driver
        /// Create .version file with version as well
        /// </summary>
        /// <param name="versionNumber">Version to be downloaded</param>
        /// <returns>Status whether download was successful or not</returns>
        DownloadResult DownloadBinary(string versionNumber);
    }
}
