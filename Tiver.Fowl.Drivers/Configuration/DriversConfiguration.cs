namespace Tiver.Fowl.Drivers.Configuration
{
    public class DriversConfiguration : IDriversConfiguration
    {
        public DriversConfiguration(string downloadLocation)
        {
            DownloadLocation = downloadLocation;
        }

        public string DownloadLocation { get; }
    }
}
