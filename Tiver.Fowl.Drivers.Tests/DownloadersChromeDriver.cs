using System;
using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.Downloaders;

namespace Tiver.Fowl.Drivers.Tests
{
    [TestClass]
    public class DownloadersChromeDriver
    {
        [TestMethod]
        public void Download_v29()
        {
            var config = (DriversConfigurationSection)ConfigurationManager.GetSection("driversConfigurationGroup/driversConfiguration");

            var driverFilepath = Path.Combine(config.DownloadLocation, "chromedriver.exe");

            if (File.Exists(driverFilepath))
            {
                File.Delete(driverFilepath);
            }

            var downloader = new ChromeDriverDownloader();
            var uri = downloader.GetLinkForVersion("2.9");
            Assert.IsNotNull(uri);
            var result = downloader.DownloadBinary(uri);
            Assert.IsTrue(result);
            var exists = File.Exists(driverFilepath);
            Assert.IsTrue(exists);
        }
    }
}
