using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tiver.Fowl.Drivers.Configuration;
using Tiver.Fowl.Drivers.DriverDownloaders;

namespace Tiver.Fowl.Drivers.Tests
{
    [TestFixture]
    public class DownloadersChromeDriverParallel
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

        [OneTimeSetUp]
        public void SetUp()
        {
            DeleteDriverAndVersionFilesIfExist();
        }

        [Test, Parallelizable(ParallelScope.All)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Download_ParallelTests(int threadNumber)
        {
            var downloader = new ChromeDriverDownloader();
            const string versionNumber = "76.0.3809.25";
            var result = downloader.DownloadBinary(versionNumber, "win32");
            ClassicAssert.IsTrue(result.Successful, $"Reported error message:{result.ErrorMessage}");
            ClassicAssert.IsTrue(result.PerformedAction == DownloaderAction.BinaryDownloaded || result.PerformedAction == DownloaderAction.NoDownloadNeeded);
            ClassicAssert.IsNull(result.ErrorMessage);
            var exists = File.Exists(DriverFilepath("win32"));
            ClassicAssert.IsTrue(exists);
            exists = downloader.Binary.CheckBinaryExists();
            ClassicAssert.IsTrue(exists);
            ClassicAssert.AreEqual(versionNumber, downloader.Binary.GetExistingBinaryVersion());
        }
    }
}
