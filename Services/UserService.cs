using System.Collections.Generic;
using System.Threading.Tasks;
using NL2SQL_Blazor.Components.Models;
using static DatabaseEngine;
using Product = NL2SQL_Blazor.Components.Models.Product;


public class UserService
{
    private readonly ProductService _productService;

    public UserService(ProductService productService)
    {
        _productService = productService;
    }

    // Replace with real data source as needed
    public Task<List<User>> GetUsersAsync()
    {
        var users = new List<User>
            {
                new User { UserId = 1, Username = "admin", Role = "Administrator" },
                new User { UserId = 2, Username = "ba", Role = "Business Analyst"},
                new User { UserId = 3, Username = "tester", Role = "Tester"}
            };
        return Task.FromResult(users);
    }

    public async Task<List<UserProducts>> GetUserProductsAsync()
    {
        var users = await GetUsersAsync();
        var userProducts = new List<UserProducts>
            {
                new UserProducts { UserId = 1, ProductId = 1 },
                new UserProducts { UserId = 1, ProductId = 2 },
                new UserProducts { UserId = 1, ProductId = 3 },
                new UserProducts { UserId = 2, ProductId = 1 },
                new UserProducts { UserId = 2, ProductId = 2 },
                new UserProducts { UserId = 3, ProductId = 1 }
            };
        return userProducts
            .Where(up => users.Any(u => u.UserId == up.UserId))
            .ToList();
    }

    public async Task<List<Product>> GetProductsByUserIdAsync(int userId)
    {
        var userProducts = await GetUserProductsAsync();
        var products = await _productService.GetProductsAsync();

        // Get all ProductIds for the given userId
        var productIds = userProducts
            .Where(up => up.UserId == userId)
            .Select(up => up.ProductId)
            .ToHashSet();

        // Return the matching products
        return products
            .Where(p => productIds.Contains(p.Id))
            .ToList();
    }

    public async Task<User?> GetUserByNameAsync(string name)
    {
        var users = await GetUsersAsync();
        return users.Where(u => u.Username == name).FirstOrDefault();
    }

    public Task UpdateUserRoleAsync(int userId, string productName)
    {
        // Update the user role in your data source.
        // For demo, do nothing or update your in-memory list.
        return Task.CompletedTask;
    }
}
