using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using SiPerpus.DAL.Models;
using static SiPerpus.DAL.Repository.Repositories;
using SiPerpus.BLL;
using System.Net;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.Azure.Cosmos;

namespace SiPerpus.API
{
    public class SiPerpusNexusAPI
    {
        private readonly BookNexusService _bookNexusService;
        public SiPerpusNexusAPI(CosmosClient client)
        {
            _bookNexusService ??= new BookNexusService(new BookNexusRepository(client));
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BookNexus))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookCreateNexus")]
        public async Task<IActionResult> BookCreateNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "BookNexus")]
            [RequestBodyType(typeof(BookNexus), "Create book nexus")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a new book");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            BookNexus data;

            try
            {
                data = JsonConvert.DeserializeObject<BookNexus>(requestBody);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to parse input", requestBody);
                return new BadRequestObjectResult("Failed to parse input");
            }

            try
            {
                var bookNexus1 = new BookNexus() { Title = data.Title, Category = data.Category, Code = "xxxx" };
                var res = await _bookNexusService.CreateBookNexus(bookNexus1);

                return new OkObjectResult(res);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to create book", data);
                return new BadRequestObjectResult("Failed to create book");
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(List<BookNexus>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookReadNexus")]
        public async Task<IActionResult> BookReadNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BookNexus")] HttpRequest req,
            ILogger log)
        {
            var res = await _bookNexusService.GetAllBookNexus();
            return new OkObjectResult(res);
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BookNexus))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookReadByIdNexus")]
        public async Task<IActionResult> BookReadByIdNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BookNexus/{id:guid}")] HttpRequest req,
            string id,
            ILogger log)
        {
            var data = await _bookNexusService.GetBookNexusById(id, new Dictionary<string, string> { { "Code", "xxxx" } });
            
            return new OkObjectResult(data);
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BookNexus))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookUpdateNexus")]
        public async Task<IActionResult> BookUpdateNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "BookNexus")]
            [RequestBodyType(typeof(BookNexus), "Update book nexus")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("BookUpdateNexus processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            BookNexus data;

            try
            {
                data = JsonConvert.DeserializeObject<BookNexus>(requestBody);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to parse input", requestBody);
                return new BadRequestObjectResult("Failed to parse input");
            }

            try
            {
                await _bookNexusService.UpdateBookNexus(data.Id, data);

                log.LogInformation($"Book sucesfully updated. Id: {data.Id}, Title: {data.Title}");

                return data != null
                    ? (ActionResult)new OkObjectResult(data)
                    : new BadRequestObjectResult("Invalid entry");
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to update book", data);
                return new BadRequestObjectResult("Failed to update book");
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookDeleteNexus")]
        public async Task<IActionResult> DeleteBookNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "BookNexus/{id:guid}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("BookDeleteNexus processed a request.");

            try
            {
                await _bookNexusService.DeleteBookNexusAsync(id, new Dictionary<string, string> { { "Code", "xxxx" } });
                
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
