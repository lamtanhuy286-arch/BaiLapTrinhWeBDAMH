using OutfitShop.Interfaces;
using OutfitShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutfitShop.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repo;

        public ProductService(IRepository<Product> repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task AddProductAsync(Product product)
        {
            await _repo.AddAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _repo.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}