using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace SiPerpus.API
{
    public static class TestCosmosDB
    {
        [FunctionName("TestCosmosDB")]
        public static void Run([CosmosDBTrigger(
            databaseName: "Catalog",
            collectionName: "Book",
            ConnectionStringSetting = "CosmosDB",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
