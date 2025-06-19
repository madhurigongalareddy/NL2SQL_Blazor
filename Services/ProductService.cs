using NL2SQL_Blazor.Components.Models;
using static DatabaseEngine;

/// <summary>
/// Provides CRUD operations for <see cref="Product"/> entities.
/// </summary>
public class ProductService
{
    /// <summary>
    /// In-memory list of products.
    /// </summary>
    public readonly List<Product> _products = new()
    {
        new Product { Id = 1, ProductName = "Claims", DBServerName = "MS-SQL", Version = "2019" },
        new Product { Id = 2, ProductName = "Provider", DBServerName = "PostgreSQL", Version = "2024" },
        new Product { Id = 3, ProductName = "Member", DBServerName = "MS-SQL", Version = "2019" }
    };

    /// <summary>
    /// Retrieves all products asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of products.</returns>
    public Task<List<Product>> GetProductsAsync()
    {
        return Task.FromResult(_products);
    }

    /// <summary>
    /// Retrieves a product by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable of matching products.
    /// </returns>
    public Task<IEnumerable<Product?>> GetProductByIdAsync(int id)
    {
        var product = _products.Where(p => p.Id == id);
        return Task.FromResult(product);
    }

    /// <summary>
    /// Retrieves products by user identifier asynchronously.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable of matching products.
    /// </returns>
    public Task<IEnumerable<Product?>> GetProductsByUserIdAsync(int id)
    {
        var product = _products.Where(p => p.Id == id);
        return Task.FromResult(product);
    }

    /// <summary>
    /// Adds a new product asynchronously.
    /// </summary>
    /// <param name="product">The product to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddProductAsync(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates an existing product asynchronously.
    /// </summary>
    /// <param name="product">The product with updated values.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    /// <summary>
    /// Deletes a product by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

