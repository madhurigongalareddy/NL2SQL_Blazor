using Microsoft.AspNetCore.Components.Routing;
using System.Text.RegularExpressions;
using System;

public class AuthenticationService
{
    // Replace with your real authentication logic
    public Task<bool> LoginAsync(string username, string password)
    {
        // Example: hardcoded user
        return Task.FromResult(username == "admin" && password == "password");
    }
}