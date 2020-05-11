namespace Tiver.Fowl.Drivers.Configuration
{
    public class DriversConfiguration
    {
        /// <summary>
        /// Location for binaries to be saved
        /// If null - defaults to assembly location
        /// </summary>
        public string DownloadLocation { get; set; }

        /// <summary>
        /// Configured driver instances
        /// </summary>
        public DriverElement[] Instances { get; set; }
        
        /// <summary>
        /// Timeout to be used for HTTP requests
        /// Value in seconds
        /// </summary>
        public int HttpTimeout { get; set; }
    }
}
