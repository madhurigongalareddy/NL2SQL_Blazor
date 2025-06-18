
using NL2SQL_Blazor.Components.Models;

public class AuthenticationService
{
    private readonly UserService _userService;

    public AuthenticationService(UserService userService)
    {
        _userService = userService;
    }

    public string CurrentUsername { get; private set; }
    public int CurrentUserId { get; private set; }
    public string CurrentUserRole { get; private set; }

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

    public void Logout()
    {
        CurrentUsername = null;
        CurrentUserId = 0;
    }
}
