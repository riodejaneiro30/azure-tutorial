using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using SiPerpus.DAL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SiPerpus.BLL.Test
{
    public class BookNexusServiceTest
    {
        readonly static IEnumerable<BookNexus> books = new List<BookNexus>
                {
                    {new BookNexus() { Id = "1", Title = "title1", Category = "category1", Code = "xxxx" } },
                    {new BookNexus() { Id = "2", Title = "title2", Category = "category2", Code = "xxxx" } }
                };

        public class GetAllBookNexus
        {
            [Fact]
            public async Task GetAllBookNexus_Success()
            {
                var repo = new Mock<IDocumentDBRepository<BookNexus>>();

                repo.Setup(c => c.GetAsync(
                    It.IsAny<Expression<Func<BookNexus, bool>>>(),
                    It.IsAny<Func<IQueryable<BookNexus>, IOrderedQueryable<BookNexus>>>(),
                    It.IsAny<Expression<Func<BookNexus, BookNexus>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<Dictionary<string, string>>()
                )).Returns(Task.FromResult(new PageResult<BookNexus>(books, "")));

                var svc = new BookNexusService(repo.Object);

                // act
                var actual = await svc.GetAllBookNexus();

                // assert
                Assert.Equal(books, actual);
            }
        }
        public class GetBookNexusById
        {
            [Theory]
            [InlineData("1")]
            public async Task GetDataById_ResultFound(string id)
            {
                var repo = new Mock<IDocumentDBRepository<BookNexus>>();

                var bookData = books.Where(o => o.Id == id).FirstOrDefault();

                repo.Setup(c => c.GetByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()
                )).Returns(
                    Task.FromResult<BookNexus>(bookData)
                );

                var bookNexusService = new BookNexusService(repo.Object);

                // act
                var act = await bookNexusService.GetBookNexusById(id, new Dictionary<string, string> { { "Code", "xxxx" } });
                
                // assert
                Assert.Equal(bookData, act);
            }
        }

        //public class CreateBookNexus
        //{
        //    [Fact]
        //    public async Task CreateBookNexus_Success()
        //    {
        //        var repo = new Mock<IDocumentDBRepository<BookNexus>>();

        //        var item = new BookNexus()
        //        {
        //            Id = "3",
        //            Title = "title3",
        //            Category = "category3",
        //            Code = "xxxx"
        //        };

        //        repo.Setup(c => c.CreateAsync(
        //            It.IsAny<BookNexus>(),
        //            It.IsAny<EventGridOptions>(),
        //            It.IsAny<string>(),
        //            It.IsAny<string>()
        //        )).Returns(Task.FromResult<Document>(item));

        //        var svc = new BookNexusService(repo.Object);

        //        act
        //       var actual = await svc.CreateBookNexus(item);

        //        assert
        //        Assert.Equal(5, 2+3);
        //    }
        //}
    }
}
