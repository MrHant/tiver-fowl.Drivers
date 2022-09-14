using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.DriverBinaries;
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

        private static string[] Platforms = {"win32", "linux64"};
        
        private static string BinaryName(string platform)
        {
            return platform switch
            {
                "win32" => "chromedriver.exe",
                _ => "chromedriver"
            };
        }
        
        private static string DriverFilepath(string platform)
        {
            return Path.Combine(Config.DownloadLocation, BinaryName(platform));
        }

        private void DeleteDriverAndVersionFilesIfExist()
        {
            foreach (var platform in Platforms)
            {
                if (File.Exists(DriverFilepath(platform)))
                {
                    File.Delete(DriverFilepath(platform));
                }

                var versionFilepath = Path.Combine(Config.DownloadLocation, $"{BinaryName(platform)}.version");
                if (File.Exists(versionFilepath))
                {
                    File.Delete(versionFilepath);
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            DeleteDriverAndVersionFilesIfExist();
        }

        [Test]
        public void Download_v76([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "76.0.3809.25";
            var result = downloader.DownloadBinary(versionNumber, platform);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_From_Configuration([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = Downloaders.Get("chrome");
            var result = downloader.DownloadBinary("76.0.3809.25", platform);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
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
            var exists = File.Exists(DriverFilepath("win32"));
            Assert.IsTrue(exists);
            var driverBinary = new DriverBinary(BinaryName("win32"));
            exists = driverBinary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual("76.0.3809.25", driverBinary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_Latest_Release([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            var result = downloader.DownloadBinary("LATEST_RELEASE", platform);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.IsNotNull(downloader.Binary.GetExistingBinaryVersion());
        }
        
        [Test]
        public void Download_Latest_Release_v97([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            var result = downloader.DownloadBinary("LATEST_RELEASE_97", platform);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.IsNotNull(downloader.Binary.GetExistingBinaryVersion());
        }
        
        [Test]
        public void DownloadTwoTimes([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "76.0.3809.25";
            var result = downloader.DownloadBinary(versionNumber, platform);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var resultTwo = downloader.DownloadBinary(versionNumber, platform);
            Assert.IsTrue(resultTwo.Successful);
            Assert.AreEqual(DownloaderAction.NoDownloadNeeded, resultTwo.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }       
                
        [Test]
        public void DownloadIncorrectVersion([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "xxx";
            var result = downloader.DownloadBinary(versionNumber, platform);
            Assert.IsFalse(result.Successful);
            Assert.AreEqual(DownloaderAction.Unknown, result.PerformedAction);
            Assert.AreEqual("Cannot find specified version to download.", result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            Assert.IsFalse(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsFalse(exists);
        }
        
        [Test]
        public void DownloadDifferentVersion([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            var versionNumber = "76.0.3809.12";
            var result = downloader.DownloadBinary(versionNumber, platform);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
            
            versionNumber = "76.0.3809.25";
            result = downloader.DownloadBinary(versionNumber, platform);
            Assert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            Assert.AreEqual(DownloaderAction.BinaryUpdated, result.PerformedAction);
            Assert.IsNull(result.ErrorMessage);
            exists = File.Exists(DriverFilepath(platform));
            Assert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            Assert.IsTrue(exists);
            Assert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }
    }
}
