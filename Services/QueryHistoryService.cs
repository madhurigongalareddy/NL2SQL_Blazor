using System.Text;
using System.Security.Claims;

public class QueryHistoryService
{
    private readonly string _filePath;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public QueryHistoryService(IHttpContextAccessor httpContextAccessor)
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QueryHistory.txt");
        _httpContextAccessor = httpContextAccessor;
    }

    // Save a query with username
    public async Task AddAsync(string query, string username)
    {
        // Check for duplicate
        if (File.Exists(_filePath))
        {
            var lines = await File.ReadAllLinesAsync(_filePath);
            string lastUser = null;
            string lastQuery = null;

            // Scan backwards for efficiency (most recent at the end)
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].StartsWith("User: "))
                    lastUser = lines[i].Substring(6).Trim();
                else if (lines[i].StartsWith("Query: "))
                    lastQuery = lines[i].Substring(7).Trim();

                if (!string.IsNullOrEmpty(lastUser) && !string.IsNullOrEmpty(lastQuery))
                {
                    if (lastUser == username && lastQuery == query)
                    {
                        // Duplicate found, do not add
                        return;
                    }
                    // Reset for next entry
                    lastUser = null;
                    lastQuery = null;
                }
            }
        }

        var entry = new StringBuilder();
        entry.AppendLine("----- Query Executed -----");
        entry.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
        entry.AppendLine($"User: {username}");
        entry.AppendLine($"Query: {query}");
        entry.AppendLine();

        await File.AppendAllTextAsync(_filePath, entry.ToString());
    }

    // Get recent queries for the current user
    public async Task<List<string>> GetRecentQueriesForCurrentUserAsync(string username, int topN = 10)
    {
        if (!File.Exists(_filePath))
            return new List<string>();

        var lines = await File.ReadAllLinesAsync(_filePath);
        var recentQueries = new List<(DateTime Timestamp, string Query)>();

        string currentUser = null;
        string currentQuery = null;
        DateTime currentTimestamp = DateTime.MinValue;

        foreach (var line in lines)
        {
            if (line.StartsWith("Timestamp: "))
                DateTime.TryParse(line.Substring(11).Trim(), out currentTimestamp);
            else if (line.StartsWith("User: "))
                currentUser = line.Substring(6).Trim();
            else if (line.StartsWith("Query: "))
                currentQuery = line.Substring(7).Trim();
            else if (string.IsNullOrWhiteSpace(line) && currentUser == username && !string.IsNullOrEmpty(currentQuery))
            {
                recentQueries.Add((currentTimestamp, currentQuery));
                currentUser = null;
                currentQuery = null;
                currentTimestamp = DateTime.MinValue;
            }
        }

        return recentQueries
            .OrderByDescending(x => x.Timestamp)
            .Take(topN)
            .Select(x => x.Query)
            .ToList();
    }


}
