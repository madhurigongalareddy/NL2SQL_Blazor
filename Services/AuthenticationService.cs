public class AuthenticationService
{
    public string CurrentUsername { get; private set; }

    public Task<bool> LoginAsync(string username, string password)
    {
        // Example: hardcoded user
        if (username == "admin" && password == "password")
        {
            CurrentUsername = username;
            return Task.FromResult(true);
        }
        CurrentUsername = null;
        return Task.FromResult(false);
    }

    public void Logout()
    {
        CurrentUsername = null;
    }
}
