using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.Downloaders;

namespace Tiver.Fowl.Drivers.Tests
{
    [TestClass]
    public class DownloadersChromeDriver
    {
        private IDriversConfiguration Config =>
            (IDriversConfiguration) ConfigurationManager.GetSection("driversConfigurationGroup/driversConfiguration");

        private string DriverFilepath => Path.Combine(Config.DownloadLocation, "chromedriver.exe");

        private void DeleteDriverFileIfExists()
        {
            if (File.Exists(DriverFilepath))
            {
                File.Delete(DriverFilepath);
            }
        }

        [TestMethod]
        public void Download_v29()
        {
            DeleteDriverFileIfExists();

            var downloader = new ChromeDriverDownloader();
            var uri = downloader.GetLinkForVersion("2.9");
            Assert.IsNotNull(uri);
            var result = downloader.DownloadBinary(uri);
            Assert.IsTrue(result);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void Download_From_Configuration()
        {
            DeleteDriverFileIfExists();

            var driverConfig = Config.Instances.Cast<DriverElement>().Single(d => d.Name.Equals("chrome"));
            var downloader = (IDriverDownloader)Activator.CreateInstance(
                "Tiver.Fowl.Drivers", 
                $"Tiver.Fowl.Drivers.Downloaders.{driverConfig.DownloaderType}")
                    .Unwrap();

            var uri = downloader.GetLinkForVersion(driverConfig.Version);
            Assert.IsNotNull(uri);
            var result = downloader.DownloadBinary(uri);
            Assert.IsTrue(result);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
        }
    }
}
