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
            var sqlSchema = await File.ReadAllTextAsync(Path.Combine("Schemas", $"{product.ProductName}_Schema.txt"));

            var request = new
            {
                model = "llama3.2",
                messages = new[]
                {
                    new { role = "system", content = string.Format($"As a professional {product.DBServerName} developer, only {product.DBServerName} queries should be in response " +
                    $"without extra things or descriptions because this will be input for Database Server. Assume I have a database with this structure:{sqlSchema}") },
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

