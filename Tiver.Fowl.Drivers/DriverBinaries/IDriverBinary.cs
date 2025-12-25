namespace Tiver.Fowl.Drivers.DriverBinaries
{
    public interface IDriverBinary
    {
        string DriverBinaryFilename { get; }
        string DriverBinaryVersionFilepath { get; }

        /// <summary>
        /// Checks whether binary exists
        /// </summary>
        /// <returns>Status</returns>
        bool CheckBinaryExists();

        /// <summary>
        /// Get version of binary
        /// </summary>
        /// <returns>Version number or null if binary is not existing</returns>
        string GetExistingBinaryVersion();

        /// <summary>
        /// Get list of extracted files from version file
        /// </summary>
        /// <returns>List of extracted file paths or empty list if version file doesn't exist</returns>
        string[] GetExtractedFiles();

        /// <summary>
        /// Remove binary and .version files
        /// </summary>
        void RemoveBinaryFiles();
    }
}
