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
        private static IDriversConfiguration Config =>
            (IDriversConfiguration) ConfigurationManager.GetSection("driversConfigurationGroup/driversConfiguration");

        private static string DriverFilepath => Path.Combine(Config.DownloadLocation, "chromedriver.exe");

        private void DeleteDriverAndVersionFilesIfExist()
        {
            if (File.Exists(DriverFilepath))
            {
                File.Delete(DriverFilepath);
            }

            var versionFilepath = Path.Combine(Config.DownloadLocation, "chromedriver.exe.version");
            if (File.Exists(versionFilepath))
            {
                File.Delete(versionFilepath);
            }
        }

        [TestInitialize]
        public void SetUp()
        {
            DeleteDriverAndVersionFilesIfExist();
        }

        [TestMethod]
        public void Download_v29()
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "2.9";
            var uri = downloader.GetLinkForVersion(versionNumber);
            Assert.IsNotNull(uri);
            var result = downloader.DownloadBinary(uri, versionNumber);
            Assert.IsTrue(result);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }

        [TestMethod]
        public void Download_From_Configuration()
        {
            var driverConfig = Config.Instances.Cast<DriverElement>().Single(d => d.Name.Equals("chrome"));
            var downloader = (IDriverDownloader)Activator.CreateInstance(
                "Tiver.Fowl.Drivers", 
                $"Tiver.Fowl.Drivers.Downloaders.{driverConfig.DownloaderType}")
                    .Unwrap();

            var result = downloader.DownloadBinary(driverConfig.Version);
            Assert.IsTrue(result);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(driverConfig.Version, downloader.Binary.GetExistingBinaryVersion());
        }
    }
}
