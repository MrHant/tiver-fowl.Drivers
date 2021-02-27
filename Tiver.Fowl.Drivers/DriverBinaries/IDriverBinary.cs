namespace Tiver.Fowl.Drivers.DriverBinaries
{
    public interface IDriverBinary
    {
        string DriverBinaryFilename { get; }
        string DriverBinaryVersionFilepath { get; }
        string DriverBinaryLockFilepath { get; }

        /// <summary>
        /// Checks whether binary exists
        /// </summary>
        /// <returns>Status</returns>
        bool CheckBinaryExists();

        /// <summary>
        /// Checks binary is locked (other download in progress)
        /// </summary>
        /// <returns>Status</returns>
        bool CheckBinaryLocked();

        void AcquireBinaryLock();
        
        void ReleaseBinaryLock();

        /// <summary>
        /// Get version of binary
        /// </summary>
        /// <returns>Version number or null if binary is not existing</returns>
        string GetExistingBinaryVersion();

        /// <summary>
        /// Remove binary and .version files
        /// </summary>
        void RemoveBinaryFiles();
    }
}
