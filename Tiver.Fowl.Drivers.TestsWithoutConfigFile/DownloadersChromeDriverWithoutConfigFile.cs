using System.IO;
using NUnit.Framework;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.DriverDownloaders;

namespace Tiver.Fowl.Drivers.TestsWithoutConfigFile
{
    [TestFixture]
    public class DownloadersChromeDriverWithoutConfigFile
    {
        private static DriversConfiguration Config => new DriversConfiguration();

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
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath);
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }
    }
}