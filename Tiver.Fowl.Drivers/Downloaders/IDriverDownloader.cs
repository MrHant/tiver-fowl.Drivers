using System;

namespace Tiver.Fowl.Drivers.Downloaders
{
    public interface IDriverDownloader
    {
        Uri LinkForDownloadsPage { get; }
        string DriverBinaryFilename { get; }

        /// <summary>
        /// Checks whether binary exists
        /// </summary>
        /// <returns>Status</returns>
        bool CheckBinaryExists();

        /// <summary>
        /// Get download link for specific driver version
        /// </summary>
        /// <param name="versionNumber">Driver version to be downloaded</param>
        /// <returns>Link to file download</returns>
        Uri GetLinkForVersion(string versionNumber);

        /// <summary>
        /// Download binary of driver
        /// </summary>
        /// <param name="downloadLink">Link to file download</param>
        /// <returns>Status whether download was successful or not</returns>
        bool DownloadBinary(Uri downloadLink);
    }
}
