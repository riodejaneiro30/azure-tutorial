using Nexus.Base.CosmosDBRepository;
using SiPerpus.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiPerpus.BLL
{
    public class BookNexusService
    {
        private readonly IDocumentDBRepository<BookNexus> _repository;
        public BookNexusService(IDocumentDBRepository<BookNexus> repository)
        {
            _repository ??= repository;
        }

        public async Task<BookNexus> CreateBookNexus(BookNexus bookNexus)
        {
            return await _repository.CreateAsync(bookNexus);
        }

        public async Task<IEnumerable<BookNexus>> GetAllBookNexus()
        {
            var result = await _repository.GetAsync(p => true);
            return result.Items;
        }

        public async Task<BookNexus> GetBookNexusById(string id, Dictionary<string, string> pk)
        {
            return await _repository.GetByIdAsync(id, pk);
        }

        public async Task<BookNexus> UpdateBookNexus(string id, BookNexus bookNexus)
        {
            return await _repository.UpdateAsync(id, bookNexus);
        }

        public async Task DeleteBookNexusAsync(string id, Dictionary<string, string> partitionKey)
        {
            await _repository.DeleteAsync(id, partitionKey);
        }
    }
}
