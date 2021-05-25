using Microsoft.Azure.Cosmos;
using Nexus.Base.CosmosDBRepository;
using System;
using SiPerpus.DAL.Models;

namespace SiPerpus.DAL.Repository
{
    public class Repositories
    {
        private static readonly string _eventGridEndPoint = Environment.GetEnvironmentVariable("EventGridEndPoint");
        private static readonly string _eventGridKey = Environment.GetEnvironmentVariable("EventGridKey");
        public class BookNexusRepository : DocumentDBRepository<BookNexus>
        {
            public BookNexusRepository(CosmosClient client) :
                base("CatalogNexus", client, partitionProperties: "Code", eventGridEndPoint: _eventGridEndPoint,
                    eventGridKey: _eventGridKey, createDatabaseIfNotExist: true)
            { }
        }
    }
}
