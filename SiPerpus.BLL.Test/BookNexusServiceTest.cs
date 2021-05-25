using Moq;
using Nexus.Base.CosmosDBRepository;
using SiPerpus.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SiPerpus.BLL.Test
{
    public class BookNexusServiceTest
    {
        private readonly static Mock<IDocumentDBRepository<BookNexus>> _reps = new Mock<IDocumentDBRepository<BookNexus>>();
        private readonly static IEnumerable<BookNexus> books = new List<BookNexus>
                {
                    {new BookNexus() { Id = "1", Title = "title1", Category = "category1", Code = "xxxx" } },
                    {new BookNexus() { Id = "2", Title = "title2", Category = "category2", Code = "xxxx" } }
                };

        public class GetAllBookNexus
        {
            [Fact]
            public async Task GetAllBookNexus_Success()
            {
                _reps.Setup(c => c.GetAsync(
                    It.IsAny<Expression<Func<BookNexus, bool>>>(),
                    It.IsAny<Func<IQueryable<BookNexus>, IOrderedQueryable<BookNexus>>>(),
                    It.IsAny<Expression<Func<BookNexus, BookNexus>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<Dictionary<string, string>>()
                )).Returns(Task.FromResult(new PageResult<BookNexus>(books, "")));

                var svc = new BookNexusService(_reps.Object);

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
                var bookData = books.Where(o => o.Id == id).FirstOrDefault();

                _reps.Setup(c => c.GetByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()
                )).Returns(
                    Task.FromResult<BookNexus>(bookData)
                );

                var bookNexusService = new BookNexusService(_reps.Object);

                // act
                var act = await bookNexusService.GetBookNexusById(id, new Dictionary<string, string> { { "Code", "xxxx" } });
                
                // assert
                Assert.Equal(bookData, act);
            }
        }

        public class UpdateBookNexus
        {
            [Theory]
            [InlineData("1")]
            public async Task UpdateBookNexus_Success(string id)
            {
                var newTitle = "title11";
                var newItem = new BookNexus()
                {
                    Id = "1",
                    Title = "title11",
                    Category = "category11",
                    Code = "xxxx"
                };

                _reps.Setup(c => c.UpdateAsync(
                    It.IsAny<string>(),
                    It.IsAny<BookNexus>(),
                    It.IsAny<EventGridOptions>(),
                    It.IsAny<string>()
                )).Returns(Task.FromResult(newItem));

                var svc = new BookNexusService(_reps.Object);

                // act
                var actual = await svc.UpdateBookNexus(id, newItem);

                // assert
                Assert.Equal(newTitle, actual?.Title);
            }
        }

        public class CreateBookNexus
        {
            [Fact]
            public async Task CreateBookNexus_Success()
            {
                var item = new BookNexus()
                {
                    Id = "3",
                    Title = "title3",
                    Category = "category3",
                    Code = "xxxx"
                };

                _reps.Setup(c => c.CreateAsync(
                    It.IsAny<BookNexus>(),
                    It.IsAny<EventGridOptions>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )).Returns(Task.FromResult(item));

                var svc = new BookNexusService(_reps.Object);

                // act
                var actual = await svc.CreateBookNexus(item);

                // assert
                Assert.Equal(item, actual);
            }
        }

        public class DeleteBookNexus
        {
            [Theory]
            [InlineData("1")]
            public async Task DeleteBookNexus_Success(string id)
            {
                _reps.Setup(c => c.GetByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>()
                )).ReturnsAsync((string id, Dictionary<string, string> dict) => books.FirstOrDefault(p => p.Id == id));

                _reps.Setup(c => c.DeleteAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<EventGridOptions>()
                )).Returns(Task.CompletedTask);

                var svc = new BookNexusService(_reps.Object);

                //act
                await svc.DeleteBookNexusAsync(id, new Dictionary<string, string> { { "Code", "xxxx" } });

                //assert
                _reps.Verify(c => c.DeleteAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<EventGridOptions>()
                ), Times.Once);
            }
        }
    }
}
