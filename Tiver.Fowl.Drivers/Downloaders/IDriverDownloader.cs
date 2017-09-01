using System;
using Tiver.Fowl.Drivers.Binaries;

namespace Tiver.Fowl.Drivers.Downloaders
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
        bool DownloadBinary(string versionNumber);
    }
}
