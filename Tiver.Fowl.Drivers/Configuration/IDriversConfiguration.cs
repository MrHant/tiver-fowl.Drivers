namespace Tiver.Fowl.Drivers.Configuration
{
    public interface IDriversConfiguration
    {
        /// <summary>
        /// Location for binaries to be saved
        /// If null - defaults to assembly location
        /// </summary>
        string DownloadLocation { get; }

        /// <summary>
        /// Configured driver instances
        /// </summary>
        DriverCollection Instances { get; }
    }
}
