using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.Downloaders;

namespace Tiver.Fowl.Drivers.Tests
{
    [TestFixture]
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

        [SetUp]
        public void SetUp()
        {
            DeleteDriverAndVersionFilesIfExist();
        }

        [Test]
        public void Download_v29()
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "2.9";
            var result = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(result.Successful);
            Assert.AreEqual(DownloaderAction.NewFileDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_From_Configuration()
        {
            var driverConfig = Config.Instances.Cast<DriverElement>().Single(d => d.Name.Equals("chrome"));
            var downloader = (IDriverDownloader)Activator.CreateInstance(
                "Tiver.Fowl.Drivers", 
                $"Tiver.Fowl.Drivers.Downloaders.{driverConfig.DownloaderType}")
                    .Unwrap();

            var result = downloader.DownloadBinary(driverConfig.Version);
            Assert.IsTrue(result.Successful);
            Assert.AreEqual(DownloaderAction.NewFileDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(driverConfig.Version, downloader.Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_Latest_Release()
        {
            var downloader = new ChromeDriverDownloader();
            var result = downloader.DownloadBinary("LATEST_RELEASE");
            Assert.IsTrue(result.Successful);
            Assert.AreEqual(DownloaderAction.NewFileDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.IsNotNull(downloader.Binary.GetExistingBinaryVersion());
        }
        
        [Test]
        public void DownloadTwoTimes()
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "2.9";
            var result = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(result.Successful);
            Assert.AreEqual(DownloaderAction.NewFileDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var resultTwo = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(resultTwo.Successful);
            Assert.AreEqual(DownloaderAction.NoDownloadedNeeded, resultTwo.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }
    }
}
