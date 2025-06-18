using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Http.Headers;
using NL2SQL_Blazor.Components.Models;

public class NLQueryEngine
{
    private readonly HttpClient _httpClient;
    private readonly string _connectionString = "";
    private readonly string _apiUrl;
    public NLQueryEngine(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _apiUrl = configuration["Ollama:NlToQueryApiUrl"] ?? throw new ArgumentNullException(nameof(configuration), "API URL 'NlToQueryApiUrl' is null.");
    }

    public async Task<string> ConvertToQueryAsync(string naturalLanguage, Product product)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(naturalLanguage))
            {
                return string.Empty;
            }

            // read the db table structure from schema file 
            var systemprompt = GetSystemPrompt(product.DBServerName, product.ProductName);

            var request = new
            {
                model = "llama3.2",
                messages = new[]
                {
                    new { role = "system", content = systemprompt },
                    new { role = "user", content = naturalLanguage.ToString() }
                },
                stream = false,
            };

            var req = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            req.Content = JsonContent.Create(request);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(req);
            var rawContent = string.Empty;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
                rawContent = result?.Message?.content ?? string.Empty;
            }
            else
            {
                rawContent = response.Content.ReadAsStringAsync().Result;
            }

            return ExtractQuery(rawContent);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private static string ExtractQuery(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // Split by semicolon, remove empty entries, trim each command
        var commands = content
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("\t", " ")
            .Replace("sql", " ")
            .Replace("```", " ")
            .Replace("  ", " ")
            .Split(';')
            .Select(cmd => cmd.Trim())
            .Where(cmd => !string.IsNullOrWhiteSpace(cmd))
            .Select(cmd => cmd.EndsWith(";") ? cmd : cmd + ";");

        // Join commands with a newline for readability
        return string.Join("\n", commands);
    }
    private string GetSystemPrompt(string dbServerName, string productname)
    {
        if (string.IsNullOrWhiteSpace(dbServerName) || string.IsNullOrWhiteSpace(productname))
        {
            throw new ArgumentException("dbServerName and sqlSchema cannot be null or empty.");
        }
        var sqlSchema = File.ReadAllTextAsync(Path.Combine("Schemas", $"{productname}_Schema.txt"));

        if (dbServerName.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            return $"You are a highly skilled {dbServerName} Server expert with deep knowledge of US health insurance claims data for payer organizations. You write clean, optimized PostgreSQL queries " +
            $"using the provided database schema. \n  Your goal is to convert natural language requests into accurate, fully-formed PostgreSQL queries that run against a health " +
            $"claims database. Always select the most efficient approach. \n ### General Rules:\n - Use ANSI SQL where possible.\n - Use PostgreSQL-specific functions only when " +
            $"needed (e.g., NOW(), AGE()).\n - Always join tables correctly using primary/foreign keys. \n - Always include WHERE clauses based on user filters.\n - " +
            $"Use explicit column names (never SELECT *).\n - Apply aggregation functions (SUM, COUNT, AVG, etc.) when requested.\n - Format dates in 'YYYY-MM-DD' format.\n - " +
            $"Avoid using reserved keywords as aliases.\n - Use LIMIT and OFFSET for top-N queries (if needed).\n - Always alias tables for clarity (e.g., ch for claims_header)." +
            $"\n - NEVER use SUBSTR, LEFT, RIGHT, or any string manipulation functions to extract date parts from date columns. Use EXTRACT or date_trunc for date parts if needed.\n" +
            $" - All date fields are stored as DATE or TIMESTAMP. You can directly compare date fields without conversions.\n - Always filter dates before aggregation whenever possible to improve query performance.\n " +
            $"### Database Schema:\n **{sqlSchema.Result}\n ### Output Format:\n Only output valid PostgreSQL code. Do not explain the query. Do not add comments. Do not include any" +
            $" markdown formatting. Be aware the sql injection check mentioned in method Nl-to_sql.HasSqlInjectionPattern";
        }
        else// if (dbServerName.Equals("SQLServer", StringComparison.OrdinalIgnoreCase))
        {
            return $"You are a highly skilled SQL Server expert with deep knowledge of US health insurance claims data for payer organizations. You write clean, optimized T-SQL queries " +
           $"using the provided database schema. \n  Your goal is to convert natural language requests into accurate, fully-formed SQL Server queries that run against a health " +
           $"claims database. Always select the most efficient approach. \n ### General Rules:\n - Use ANSI SQL where possible.\n - Use SQL Server-specific functions only when " +
           $"needed (e.g., GETDATE(), DATEDIFF()).\n - Always join tables correctly using primary/foreign keys. \n - Always include WHERE clauses based on user filters.\n - " +
           $"Use explicit column names (never SELECT *).\n - Apply aggregation functions (SUM, COUNT, AVG, etc.) when requested.\n - Format dates in 'YYYY-MM-DD' format.\n - " +
           $"Avoid using reserved keywords as aliases.\n - Use OFFSET ... FETCH for top-N queries (if needed).\n - Always alias tables for clarity (e.g., ch for Claims_Header)." +
           $"\n - NEVER use SUBSTR, SUBSTRING, LEFT, RIGHT,STRFTIME, or any string manipulation functions to extract date parts.\n - All date fields are stored as DATE or DATETIME." +
           $" You can directly compare date fields without conversions.\n - Always filter dates before aggregation whenever possible to improve query performance.\n " +
           $"### Database Schema:\n **{sqlSchema.Result}\n ### Output Format:\n Only output valid T-SQL code. Do not explain the query. Do not add comments. Do not include any" +
           $" markdown formatting. Be aware the sql injection check mentioned in method Nl-to_sql.HasSqlInjectionPattern";
        } 
        
      
    }
    private class OpenAiResponse
    {
        public message Message { get; set; }
        public string Model { get; set; }

    }
    private class message
    {
        public string role { get; set; }

        public string content { get; set; }
    }
}

