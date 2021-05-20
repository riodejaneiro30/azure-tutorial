using Microsoft.Azure.Documents.Client;
using Nexus.Base.CosmosDBRepository;
using SiPerpus.DAL.Models;

namespace SiPerpus.DAL.Repository
{
    public class Repositories
    {
        public class BookNexusRepository : DocumentDBRepository<BookNexus>
        {
            public BookNexusRepository(DocumentClient client) :
                base("CatalogNexus", client, partitionProperties: "Code")
            { }
        }
    }
}
