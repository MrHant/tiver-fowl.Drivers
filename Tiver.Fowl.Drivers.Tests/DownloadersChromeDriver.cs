using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
                var versionFilepath = Path.Combine(Config.DownloadLocation, $"{BinaryName(platform)}.version");
                if (File.Exists(versionFilepath))
                {
                    var fileList = File.ReadAllLines(versionFilepath)[2..];
                    foreach (var file in fileList)
                    {
                        var filePath = Path.Combine(Config.DownloadLocation, file);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }

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
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_From_Configuration([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = Downloaders.Get("chrome");
            var result = downloader.DownloadBinary("76.0.3809.25", platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual("76.0.3809.25", downloader.Binary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_From_Configuration_directly()
        {
            var result = Downloaders.DownloadBinaryFor("chrome");
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath("win32"));
            ClassicAssert.IsTrue(exists);
            var driverBinary = new DriverBinary(BinaryName("win32"));
            exists = driverBinary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual("76.0.3809.25", driverBinary.GetExistingBinaryVersion());
        }

        [Test]
        public void Download_Latest_Release([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            var result = downloader.DownloadBinary("LATEST_RELEASE", platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.IsNotNull(downloader.Binary.GetExistingBinaryVersion());
        }
        
        [Test]
        public void Download_Latest_Release_v97([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            var result = downloader.DownloadBinary("LATEST_RELEASE_97", platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.IsNotNull(downloader.Binary.GetExistingBinaryVersion());
        }
        
        [Test]
        public void DownloadTwoTimes([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "76.0.3809.25";
            var result = downloader.DownloadBinary(versionNumber, platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var resultTwo = downloader.DownloadBinary(versionNumber, platform);
            ClassicAssert.IsTrue(resultTwo.Successful);
            ClassicAssert.AreEqual(DownloaderAction.NoDownloadNeeded, resultTwo.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }       
                
        [Test]
        public void DownloadIncorrectVersion([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "xxx";
            var result = downloader.DownloadBinary(versionNumber, platform);
            ClassicAssert.IsFalse(result.Successful);
            ClassicAssert.AreEqual(DownloaderAction.Unknown, result.PerformedAction);
            ClassicAssert.AreEqual("Cannot find specified version to download.", result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsFalse(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsFalse(exists);
        }
        
        [Test]
        public void DownloadDifferentVersion([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            var versionNumber = "76.0.3809.12";
            var result = downloader.DownloadBinary(versionNumber, platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
            
            versionNumber = "76.0.3809.25";
            result = downloader.DownloadBinary(versionNumber, platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryUpdated, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }
        
        [Test]
        public void DownloadDifferentVersionUsingLatest([ValueSource(nameof(Platforms))]string platform)
        {
            var downloader = new ChromeDriverDownloader();
            var versionNumber = "LATEST_RELEASE_103";
            var result = downloader.DownloadBinary(versionNumber, platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryDownloaded, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.IsTrue(downloader.Binary.GetExistingBinaryVersion().StartsWith("103"));
            
            versionNumber = "LATEST_RELEASE_102";
            result = downloader.DownloadBinary(versionNumber, platform);
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.AreEqual(DownloaderAction.BinaryUpdated, result.PerformedAction);
            ClassicAssert.IsNull(result.ErrorMessage);
            exists = File.Exists(DriverFilepath(platform));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.IsTrue(downloader.Binary.GetExistingBinaryVersion().StartsWith("102"));
        }
    }
}
