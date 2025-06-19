using NL2SQL_Blazor.Components.Models;
using static DatabaseEngine;

public class ProductService
{
    public readonly List<Product> _products = new()
       {
           new Product { Id = 1, ProductName = "Claims", DBServerName = "MS-SQL", Version = "2019" },
           new Product { Id = 2, ProductName = "Provider", DBServerName = "PostgreSQL", Version = "2024" },
           new Product { Id = 3, ProductName = "Member", DBServerName = "MS-SQL", Version = "2019" }
       };

    public Task<List<Product>> GetProductsAsync()
    {
        return Task.FromResult(_products);
    }

    public Task<IEnumerable<Product?>> GetProductByIdAsync(int id)
    {
        var product = _products.Where(p => p.Id == id);
        return Task.FromResult(product);
    }
    public Task<IEnumerable<Product?>> GetProductsByUserIdAsync(int id)
    {
        var product = _products.Where(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task AddProductAsync(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateProductAsync(Product product)
    {
        var existing = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existing != null)
        {
            existing.ProductName = product.ProductName;
            existing.DBServerName = product.DBServerName;
            existing.Version = product.Version;
        }
        return Task.CompletedTask;
    }

    public Task DeleteProductAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }
}

