using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.DriverDownloaders;

namespace Tiver.Fowl.Drivers.Tests
{
    [TestFixture]
    public class DownloadersChromeDriver
    {
        private static DriversConfiguration Config
        {
            get
            {
                var driversConfiguration = new DriversConfiguration();

                var config = new ConfigurationBuilder()
                    .AddJsonFile("Tiver_config.json", optional: true)
                    .Build();
                config.GetSection("Tiver.Fowl.Drivers").Bind(driversConfiguration);
                return driversConfiguration;
            }
        }

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
        public void Download_v76()
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "76.0.3809.25";
            var result = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
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
            var downloader = Downloaders.Get("chrome");
            var result = downloader.DownloadBinary("76.0.3809.25");
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual("76.0.3809.25", downloader.Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_From_Configuration_directly()
        {
            var result = Downloaders.DownloadBinaryFor("chrome");
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = new ChromeDriverDownloader().Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual("76.0.3809.25", new ChromeDriverDownloader().Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_Latest_Release()
        {
            var downloader = new ChromeDriverDownloader();
            var result = downloader.DownloadBinary("LATEST_RELEASE");
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
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
            const string versionNumber = "76.0.3809.25";
            var result = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var resultTwo = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(resultTwo.Successful);
            Assert.AreEqual(DownloaderAction.NoDownloadNeeded, resultTwo.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }       
                
        [Test]
        public void DownloadIncorrectVersion()
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "xxx";
            var result = downloader.DownloadBinary(versionNumber);
            Assert.IsFalse(result.Successful);
            Assert.AreEqual(DownloaderAction.Unknown, result.PerformedAction);
            Assert.AreEqual("Cannot find specified version to download.", result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsFalse(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsFalse(exists);
        }
        
        [Test]
        public void DownloadDifferentVersion()
        {
            var downloader = new ChromeDriverDownloader();
            var versionNumber = "76.0.3809.12";
            var result = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
            
            versionNumber = "76.0.3809.25";
            result = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryUpdated, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }
    }
}
