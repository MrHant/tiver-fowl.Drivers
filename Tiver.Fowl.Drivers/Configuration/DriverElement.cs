using System.Configuration;

namespace Tiver.Fowl.Drivers.Configuration
{
    public class DriverElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        [ConfigurationProperty("downloaderType", IsRequired = true)]
        public string DownloaderType
        {
            get => (string)base["downloaderType"];
            set => base["downloaderType"] = value;
        }

        [ConfigurationProperty("version", IsRequired = true)]
        public string Version
        {
            get => (string)base["version"];
            set => base["version"] = value;
        }
    }
}
