using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SiPerpus.Model;
using Microsoft.Azure.Documents;

namespace SiPerpus
{
    public static class SiPerpusController
    {
        private const string DatabaseName = "siperpus";
        private const string CollectionName = "book";
        private const string Route = "book";

        [FunctionName("book-create")]
        public static async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
            [CosmosDB(
                DatabaseName,
                CollectionName,
                ConnectionStringSetting = "CosmosDBConnection")]
            IAsyncCollector<object> books, ILogger log)
        {
            log.LogInformation("Creating a new book");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Book data;

            try
            {
                data = JsonConvert.DeserializeObject<Book>(requestBody);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to parse input", requestBody);
                return new BadRequestObjectResult("Failed to parse input");
            }

            try
            {
                var book = new Book()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = data.Title,
                    Category = data.Category
                };

                await books.AddAsync(book);

                return new OkObjectResult(book);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to create book", data);
                return new BadRequestObjectResult("Failed to create book");
            }
        }

        [FunctionName("book-get-all")]
        public static IActionResult GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
            [CosmosDB(
                DatabaseName,
                CollectionName,
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c order by c._ts desc")]
                IEnumerable<Book> books,
            ILogger log)
        {
            log.LogInformation("Getting book list");
            return new OkObjectResult(books);
        }

        [FunctionName("book-get-by-id")]
        public static IActionResult GetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id:guid}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection", PartitionKey = "id")] DocumentClient client,
            ILogger log, string id)
        {
            log.LogInformation("Getting book by id");

            try
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                var document = client.CreateDocumentQuery(collectionUri).Where(t => t.Id == id)
                        .AsEnumerable().FirstOrDefault();

                Book book = (dynamic)document;
                return new OkObjectResult(book);
            } 
            catch (Exception e)
            {
                log.LogError(e, "Get book by id failed processing a request");
                return new BadRequestObjectResult("Failed processing a request");
            }
        }

        [FunctionName("book-update")]
        public static async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id:guid}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")]
                DocumentClient client,
            ILogger log, string id)
        {
            log.LogInformation("Updating book");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Book updated;

            try
            {
                updated = JsonConvert.DeserializeObject<Book>(requestBody);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to parse input", requestBody);
                return new BadRequestObjectResult("Failed to parse input");
            }

            try
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                var document = client.CreateDocumentQuery(collectionUri).Where(t => t.Id == id)
                                .AsEnumerable().FirstOrDefault();

                if (document == null)
                {
                    return new NotFoundResult();
                }

                document.SetPropertyValue("Title", updated.Title);
                if (!string.IsNullOrEmpty(updated.Category))
                {
                    document.SetPropertyValue("Category", updated.Category);
                }

                await client.ReplaceDocumentAsync(document);

                Book book = (dynamic)document;

                return new OkObjectResult(book);
            }
            catch (Exception e)
            {
                log.LogError(e, String.Format("Failed to update book {0}", id), updated);
                return new BadRequestObjectResult("Failed to update book");
            }

            
        }

        [FunctionName("book-delete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id:guid}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection", PartitionKey = "id")] DocumentClient client,
            ILogger log, string id)
        {
            log.LogInformation("Deleting book");

            try
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                var document = client.CreateDocumentQuery(collectionUri).Where(t => t.Id == id)
                        .AsEnumerable().FirstOrDefault();

                if (document == null)
                {
                    return new NotFoundResult();
                }
                await client.DeleteDocumentAsync(document.SelfLink, new RequestOptions() { PartitionKey = new PartitionKey(id) });
                return new OkObjectResult(string.Format("Book {0} is deleted", id));
            }
            catch (Exception e)
            {
                log.LogError(e, String.Format("Failed to delete book {0}", id));
                return new BadRequestObjectResult("Failed to delete book");
            }
        }
    }
}
