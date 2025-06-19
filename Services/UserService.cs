using System.Collections.Generic;
using System.Threading.Tasks;
using NL2SQL_Blazor.Components.Models;
using static DatabaseEngine;
using Product = NL2SQL_Blazor.Components.Models.Product;


/// <summary>
/// Provides user-related operations, including user retrieval and user-product associations.
/// </summary>
public class UserService
{
    private readonly ProductService _productService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="productService">The product service dependency.</param>
    public UserService(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves a list of users asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of users.</returns>
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

    /// <summary>
    /// Retrieves a list of user-product associations asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of user-product associations.</returns>
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

    /// <summary>
    /// Retrieves the list of products associated with a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of products for the user.</returns>
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

    /// <summary>
    /// Retrieves a user by username asynchronously.
    /// </summary>
    /// <param name="name">The username to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found; otherwise, null.</returns>
    public async Task<User?> GetUserByNameAsync(string name)
    {
        var users = await GetUsersAsync();
        return users.Where(u => u.Username == name).FirstOrDefault();
    }

    /// <summary>
    /// Updates the role of a user asynchronously.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="productName">The new product name or role to assign.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateUserRoleAsync(int userId, string productName)
    {
        // Update the user role in your data source.
        // For demo, do nothing or update your in-memory list.
        return Task.CompletedTask;
    }
}
