
using NL2SQL_Blazor.Components.Models;

/// <summary>
/// Provides authentication services, including login and logout functionality.
/// </summary>
public class AuthenticationService
{
    private readonly UserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="userService">The user service dependency.</param>
    public AuthenticationService(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets the username of the currently authenticated user.
    /// </summary>
    public string CurrentUsername { get; private set; }

    /// <summary>
    /// Gets the user ID of the currently authenticated user.
    /// </summary>
    public int CurrentUserId { get; private set; }

    /// <summary>
    /// Gets the role of the currently authenticated user.
    /// </summary>
    public string CurrentUserRole { get; private set; }

    /// <summary>
    /// Attempts to log in a user asynchronously with the specified username and password.
    /// </summary>
    /// <param name="username">The username to log in with.</param>
    /// <param name="password">The password to log in with.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains <c>true</c> if login is successful; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> LoginAsync(string username, string password)
    {
        User? user = await _userService.GetUserByNameAsync(username);
        if (user == null)
        {
            CurrentUsername = null;
            CurrentUserId = 0;
            CurrentUserRole = "";
            return false;
        }
        else
        {
            // For demo purposes, we assume the password is always correct.
            if ((username == "admin" && password == "password") ||
                (username == "ba" && password == "password") ||
                (username == "tester" && password == "password"))
            {
                CurrentUsername = user.Username;
                CurrentUserId = user.UserId;
                CurrentUserRole = user.Role;
                return true;
            }
            else
            {
                CurrentUsername = null;
                CurrentUserRole = "";
                CurrentUserId = 0;
                return false;
            }
        }
    }

    /// <summary>
    /// Logs out the currently authenticated user asynchronously.
    /// </summary>
    /// <returns>A completed task representing the logout operation.</returns>
    public Task LogoutAsync()
    {
        // Clear user details
        CurrentUsername = null;
        CurrentUserRole = "";
        CurrentUserId = 0;
        return Task.CompletedTask;
    }
}
