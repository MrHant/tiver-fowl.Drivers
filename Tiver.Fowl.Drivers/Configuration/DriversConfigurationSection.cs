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

        [ConfigurationProperty("downloadLocation", IsRequired = false)]
        private string DownloadLocationElement
        {
            get
            {
                return (string)this["downloadLocation"];
            }

            set
            {
                this["downloadLocation"] = value;
            }
        }
    }
}
