using System.Configuration;

namespace Tiver.Fowl.Drivers.Configuration
{
    public static class ConfigurationReader
    {
        public static IDriversConfiguration ReadFromFileOrDefault(string configSectionName = "driversConfiguration")
        {
            var configFromFile = (DriversConfigurationSection) ConfigurationManager.GetSection(configSectionName);
            return configFromFile ?? new DriversConfigurationSection();
        }
    }
}