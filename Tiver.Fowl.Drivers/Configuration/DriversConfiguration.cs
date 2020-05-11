using System.IO;
using System.Reflection;

namespace Tiver.Fowl.Drivers.Configuration
{
    public class DriversConfiguration
    {
        private string _downloadLocation;

        /// <summary>
        /// Location for binaries to be saved
        /// Defaults to assembly location
        /// </summary>
        public string DownloadLocation { get; set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Configured driver instances
        /// </summary>
        public DriverElement[] Instances { get; set; } = {};

        /// <summary>
        /// Timeout to be used for HTTP requests
        /// Value in seconds
        /// </summary>
        public int HttpTimeout { get; set; } = 100;
    }
}
