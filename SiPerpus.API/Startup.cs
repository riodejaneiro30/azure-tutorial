using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

[assembly: WebJobsStartup(typeof(SiPerpus.API.Startup))]
namespace SiPerpus.API
{
    public class Startup : FunctionsStartup
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables()
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());

            builder.Services.AddSingleton(s =>
            {
                var connectionString = Configuration.GetConnectionString("CosmosDB");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDB connection string in the appSettings.json file or your Azure Functions Settings.");
                }
                return new CosmosClientBuilder(connectionString).Build();
            });
        }
    }
}
