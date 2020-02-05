using System.Configuration;

namespace Tiver.Fowl.Drivers.Configuration
{
    public static class ConfigurationReader
    {
        public static IDriversConfiguration ReadFromFileOrDefault(string configSectionName = null)
        {
            // Setting default config section name
            configSectionName = configSectionName ?? "driversConfiguration";
            
            var configFromFile = (DriversConfigurationSection) ConfigurationManager.GetSection(configSectionName);
            return configFromFile ?? new DriversConfigurationSection();
        }
    }
}