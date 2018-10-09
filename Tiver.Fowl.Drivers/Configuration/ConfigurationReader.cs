using System.Configuration;

namespace Tiver.Fowl.Drivers.Configuration
{
    public static class ConfigurationReader
    {
        public static IDriversConfiguration ReadFromFileOrDefault()
        {
            var configFromFile =
                (DriversConfigurationSection) ConfigurationManager.GetSection(
                    "driversConfigurationGroup/driversConfiguration");

            return configFromFile ?? new DriversConfigurationSection();
        }
    }
}