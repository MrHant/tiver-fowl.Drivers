using System.Configuration;

namespace Tiver.Fowl.Drivers.Configuration
{
    public class DriverElement
    {
        public string Name { get; set; }
        public string DownloaderType { get; set; }
        public string Version { get; set; }
        public string Platform { get; set; }
    }
}
