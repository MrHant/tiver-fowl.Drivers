using System;
using System.Net.Http;
using Tiver.Fowl.Drivers.Configuration;

namespace Tiver.Fowl.Drivers.Downloaders
{
    public static class Context
    {
        public static readonly HttpClient HttpClient = new HttpClient();

        static Context()
        {
            var config = ConfigurationReader.ReadFromFileOrDefault();
            HttpClient.Timeout = TimeSpan.FromSeconds(config.HttpTimeout);
        }
    }
}