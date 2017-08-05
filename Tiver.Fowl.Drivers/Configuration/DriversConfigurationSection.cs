using System.Configuration;
using System.IO;
using System.Reflection;

namespace Tiver.Fowl.Drivers.Configuration
{
    public class DriversConfigurationSection : ConfigurationSection, IDriversConfiguration
    {
        public string DownloadLocation
        {
            get
            {
                if (string.IsNullOrEmpty(DownloadLocationElement))
                {
                    return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }

                return DownloadLocationElement;
            }
        }

        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public DriverCollection Instances
        {
            get => (DriverCollection)this[""];
            set => this[""] = value;
        }

        [ConfigurationProperty("downloadLocation", IsRequired = false)]
        private string DownloadLocationElement
        {
            get => (string)this["downloadLocation"];
            set => this["downloadLocation"] = value;
        }
    }
}
