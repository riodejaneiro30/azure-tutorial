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
using Microsoft.Azure.Documents;
using SiPerpus.DAL.Models;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.EventGrid;

namespace SiPerpus.API
{
    public static class SiPerpusController
    {
        private const string DatabaseName = "Catalog";
        private const string CollectionName = "Book";

        [FunctionName("BookCreate")]
        public static async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Book")] HttpRequest req,
            [CosmosDB(
                DatabaseName,
                CollectionName,
                ConnectionStringSetting = "CosmosDBConnection")]
            IAsyncCollector<object> books, ILogger log)
        {
            log.LogInformation("Creating a new book");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Book data;

            var eventGridEndPoint = Environment.GetEnvironmentVariable("eventGridEndPoint");
            var eventGridKey = Environment.GetEnvironmentVariable("eventGridEndKey");

            var topicHostname = new Uri(eventGridEndPoint).Host;
            TopicCredentials topicCredentials = new TopicCredentials(eventGridKey);
            EventGridClient eventGridClient = new EventGridClient(topicCredentials);

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
                eventGridClient.PublishEventsAsync(topicHostname, GetEventsList(book, "Create/")).GetAwaiter().GetResult();

                return new OkObjectResult(book);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to create book", data);
                return new BadRequestObjectResult("Failed to create book");
            }
        }

        [FunctionName("BookGetAll")]
        public static IActionResult GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Book")] HttpRequest req,
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

        [FunctionName("BookGetById")]
        public static IActionResult GetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Book/{id:guid}")] HttpRequest req,
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

        [FunctionName("BookUpdate")]
        public static async Task<IActionResult> Update(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Book")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")]
                DocumentClient client,
            ILogger log)
        {
            log.LogInformation("Updating book");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Book updated;

            var eventGridEndPoint = Environment.GetEnvironmentVariable("eventGridEndPoint");
            var eventGridKey = Environment.GetEnvironmentVariable("eventGridEndKey");

            var topicHostname = new Uri(eventGridEndPoint).Host;
            TopicCredentials topicCredentials = new TopicCredentials(eventGridKey);
            EventGridClient eventGridClient = new EventGridClient(topicCredentials);

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
                var document = client.CreateDocumentQuery(collectionUri).Where(t => t.Id == updated.Id)
                                .AsEnumerable().FirstOrDefault();

                if (document == null)
                {
                    return new BadRequestObjectResult("Failed to update book");
                }

                document.SetPropertyValue("Title", updated.Title);
                if (!string.IsNullOrEmpty(updated.Category))
                {
                    document.SetPropertyValue("Category", updated.Category);
                }

                await client.ReplaceDocumentAsync(document);
                Book book = (dynamic)document;

                eventGridClient.PublishEventsAsync(topicHostname, GetEventsList(book, "Update/")).GetAwaiter().GetResult();

                return new OkObjectResult(book);
            }
            catch (Exception e)
            {
                log.LogError(e, String.Format("Failed to update book {0}", updated.Id), updated);
                return new BadRequestObjectResult("Failed to update book");
            }
        }

        [FunctionName("BookDelete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Book/{id:guid}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection", PartitionKey = "id")] DocumentClient client,
            ILogger log, string id)
        {
            log.LogInformation("Deleting book");

            var eventGridEndPoint = Environment.GetEnvironmentVariable("eventGridEndPoint");
            var eventGridKey = Environment.GetEnvironmentVariable("eventGridEndKey");

            var topicHostname = new Uri(eventGridEndPoint).Host;
            TopicCredentials topicCredentials = new TopicCredentials(eventGridKey);
            EventGridClient eventGridClient = new EventGridClient(topicCredentials);

            try
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
                var document = client.CreateDocumentQuery(collectionUri).Where(t => t.Id == id)
                        .AsEnumerable().FirstOrDefault();

                if (document == null)
                {
                    return new BadRequestObjectResult("Failed to delete book");
                }
                await client.DeleteDocumentAsync(document.SelfLink, new RequestOptions() { PartitionKey = new PartitionKey(id) });
                eventGridClient.PublishEventsAsync(topicHostname, GetEventList(id)).GetAwaiter().GetResult();

                return new OkObjectResult(string.Format("Book {0} is deleted", id));
            }
            catch (Exception e)
            {
                log.LogError(e, String.Format("Failed to delete book {0}", id));
                return new BadRequestObjectResult("Failed to delete book");
            }
        }

        static IList<EventGridEvent> GetEventList(string id)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>();
            eventsList.Add(new EventGridEvent()
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "SiPerpus.DAL.Models.Book",
                Data = id,
                EventTime = DateTime.Now,
                Subject = "/Delete",
                DataVersion = "1.0"
            });

            return eventsList;
        }

        static IList<EventGridEvent> GetEventsList(Book book, string subject)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>();
            eventsList.Add(new EventGridEvent()
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "SiPerpus.DAL.Models.Book",
                Data = book,
                EventTime = DateTime.Now,
                Subject = subject,
                DataVersion = "1.0"
            });

            return eventsList;
        }
    }
}
