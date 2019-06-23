using System.IO;
using NUnit.Framework;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.DriverDownloaders;

namespace Tiver.Fowl.Drivers.Tests
{
    [TestFixture]
    public class DownloadersChromeDriver
    {
        private static IDriversConfiguration Config => ConfigurationReader.ReadFromFileOrDefault();

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
            var result = downloader.DownloadBinary("2.9");
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual("2.9", downloader.Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_From_Configuration_directly()
        {
            var result = Downloaders.DownloadBinary("chrome");
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = new ChromeDriverDownloader().Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual("2.9", new ChromeDriverDownloader().Binary.GetExistingBinaryVersion());
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
            const string versionNumber = "2.9";
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
            var versionNumber = "2.9";
            var result = downloader.DownloadBinary(versionNumber);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
            
            versionNumber = "2.10";
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
