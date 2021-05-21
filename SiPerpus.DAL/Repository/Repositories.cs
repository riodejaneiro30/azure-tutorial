using Microsoft.Azure.Documents.Client;
using Nexus.Base.CosmosDBRepository;
using SiPerpus.DAL.Models;
using System;

namespace SiPerpus.DAL.Repository
{
    public class Repositories
    {
        private static readonly string _eventGridEndPoint = Environment.GetEnvironmentVariable("eventGridEndPoint");
        private static readonly string _eventGridKey = Environment.GetEnvironmentVariable("eventGridEndKey");
        public class BookNexusRepository : DocumentDBRepository<BookNexus>
        {
            public BookNexusRepository(DocumentClient client) :
                base("CatalogNexus", client, partitionProperties: "Code", eventGridEndPoint: _eventGridEndPoint,
                    eventGridKey: _eventGridKey)
            { }
            //public BookNexusRepository(DocumentClient client) :
            //    base("CatalogNexus", client, partitionProperties: "Code")
            //{ }
        }
    }
}
