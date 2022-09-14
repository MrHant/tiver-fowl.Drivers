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
            var downloader = GetDownloader(driverConfig);
            return downloader;
        }

        public static DownloadResult DownloadBinaryFor(string name)
        {
            var driverConfig = GetDriverConfiguration(name);
            var downloader = GetDownloader(driverConfig);
            return downloader.DownloadBinary(driverConfig.Version, driverConfig.Platform);
        }

        private static IDriverDownloader GetDownloader(DriverElement driverConfig)
        {
            var typeName = $"Tiver.Fowl.Drivers.DriverDownloaders.{driverConfig.DownloaderType}";
            var type = Type.GetType(typeName) ?? throw new TypeLoadException($"Can't create instance of type '{typeName}'");
            var downloader = (IDriverDownloader)Activator.CreateInstance(type);
            return downloader;
        }

        private static DriverElement GetDriverConfiguration(string name)
        {
            return Context.Configuration.Drivers.Single(i => i.Name.Equals(name));
        }
    }
}