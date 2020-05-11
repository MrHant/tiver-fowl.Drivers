using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Tiver.Fowl.Drivers.Configuration;

namespace Tiver.Fowl.Drivers
{
    public static class Context
    {
        public static readonly HttpClient HttpClient = new HttpClient();

        public static readonly DriversConfiguration Configuration;
        
        static Context()
        {
            Configuration = new DriversConfiguration();

            var config = new ConfigurationBuilder()
                .AddJsonFile("Tiver_config.json", optional: true)
                .Build();
            config.GetSection("Tiver.Fowl.Drivers").Bind(Configuration);

            HttpClient.Timeout = TimeSpan.FromSeconds(Configuration.HttpTimeout);
        }
    }
}