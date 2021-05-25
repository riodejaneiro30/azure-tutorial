﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using SiPerpus.DAL.Models;
using static SiPerpus.DAL.Repository.Repositories;
using SiPerpus.BLL;
using System.Net;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using SiPerpus.DAL.Repository;

namespace SiPerpus.API
{
    public static class SiPerpusNexusAPI
    {
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BookNexus))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        //[RequestBodyType(typeof(< RequestBodyObj >), <Description>)]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookCreateNexus")]
        public static async Task<IActionResult> BookCreateNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "BookNexus")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
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

                BookNexusService bookNexusService = new BookNexusService(new BookNexusRepository(documentClient));
                var res = await bookNexusService.CreateBookNexus(bookNexus1);

                return new OkObjectResult(res);
            }
            catch (Exception e)
            {
                log.LogError(e, "Failed to create book", data);
                return new BadRequestObjectResult("Failed to create book");
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookReadNexus")]
        public static async Task<IActionResult> BookReadNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BookNexus")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
            ILogger log)
        {
            //BookNexusService bookNexusService = new BookNexusService(new BookNexusRepository(documentClient));
            //var res = await bookNexusService.GetAllBookNexus();
            //return new OkObjectResult(res);
            using var bookNexusRep = new Repositories.BookNexusRepository(documentClient);
            var data = await bookNexusRep.GetAsync();
            return new OkObjectResult(data);
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [QueryStringParameter("id", "book id")]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookReadByIdNexus")]
        public static async Task<IActionResult> BookReadByIdNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BookNexus/{id:guid}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
            string id,
            ILogger log)
        {
            BookNexusService bookNexusService = new BookNexusService(new BookNexusRepository(documentClient));
            var data = await bookNexusService.GetBookNexusById(id, new Dictionary<string, string> { { "Code", "xxxx" } });
            
            return new OkObjectResult(data);
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BookNexus))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookUpdateNexus")]
        public static async Task<IActionResult> BookUpdateNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "BookNexus")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection", PartitionKey = "Code")] DocumentClient documentClient,
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
                //using (var reps = new Repositories.BookNexusRepository(documentClient))
                //{
                //    await reps.UpdateAsync(data.Id, data);
                //}
                BookNexusService bookNexusService = new BookNexusService(new BookNexusRepository(documentClient));
                await bookNexusService.UpdateBookNexus(data.Id, data);

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

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BookNexus))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [QueryStringParameter("id", "book id")]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: false)]
        [FunctionName("BookDeleteNexus")]
        public static async Task<IActionResult> DeleteBookNexus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "BookNexus/{id:guid}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDBConnection")] DocumentClient documentClient,
            ILogger log, string id)
        {
            log.LogInformation("BookDeleteNexus processed a request.");

            try
            {
                BookNexusService bookNexusService = new BookNexusService(new BookNexusRepository(documentClient));
                await bookNexusService.DeleteBookNexusAsync(id, new Dictionary<string, string> { { "Code", "xxxx" } });
                
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
