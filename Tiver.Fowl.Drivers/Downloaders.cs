using System;
using System.Linq;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.DriverDownloaders;

namespace Tiver.Fowl.Drivers
{
    public static class Downloaders
    {
        public static IDriverDownloader Get(string name)
        {
            var driverConfig = GetDriverConfiguration(name);
            var downloader = (IDriverDownloader)Activator.CreateInstance(
                    "Tiver.Fowl.Drivers", 
                    $"Tiver.Fowl.Drivers.DriverDownloaders.{driverConfig.DownloaderType}")
                    .Unwrap();
            return downloader;
        }
        
        public static DownloadResult DownloadBinaryFor(string name)
        {
            var driverConfig = GetDriverConfiguration(name);
            var downloader = (IDriverDownloader)Activator.CreateInstance(
                    "Tiver.Fowl.Drivers", 
                    $"Tiver.Fowl.Drivers.DriverDownloaders.{driverConfig.DownloaderType}")
                .Unwrap();
            return downloader.DownloadBinary(driverConfig.Version);
        }

        private static DriverElement GetDriverConfiguration(string name)
        {
            return Context.Configuration.Instances.Single(i => i.Name.Equals(name));
        }
    }
}