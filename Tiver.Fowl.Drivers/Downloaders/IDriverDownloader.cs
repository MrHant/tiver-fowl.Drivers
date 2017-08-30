using System;
using Tiver.Fowl.Drivers.Binaries;

namespace Tiver.Fowl.Drivers.Downloaders
{
    public interface IDriverDownloader
    {
        IDriverBinary Binary { get; }
        Uri LinkForDownloadsPage { get; }

        /// <summary>
        /// Get download link for specific driver version
        /// </summary>
        /// <param name="versionNumber">Driver version to be downloaded</param>
        /// <returns>Link to file download</returns>
        Uri GetLinkForVersion(string versionNumber);

        /// <summary>
        /// Download binary of driver
        /// Create .version file with version as well
        /// </summary>
        /// <param name="downloadLink">Link to file download</param>
        /// <param name="versionNumber">Version to be written in .version file</param>
        /// <returns>Status whether download was successful or not</returns>
        bool DownloadBinary(Uri downloadLink, string versionNumber);

        /// <summary>
        /// Download binary of driver
        /// Create .version file with version as well
        /// </summary>
        /// <param name="versionNumber">Version to be downloaded</param>
        /// <returns>Status whether download was successful or not</returns>
        bool DownloadBinary(string versionNumber);
    }
}
