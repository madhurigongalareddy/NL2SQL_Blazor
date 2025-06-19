using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Http.Headers;
using NL2SQL_Blazor.Components.Models;

public class NLQueryEngine
{
    private readonly HttpClient _httpClient;
    private readonly string _connectionString = "";
    private readonly string _apiUrl;
    /// <summary>
    /// Initializes a new instance of the <see cref="NLQueryEngine"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    /// <param name="configuration">The application configuration for retrieving API URLs.</param>
    public NLQueryEngine(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _apiUrl = configuration["Ollama:NlToQueryApiUrl"] ?? throw new ArgumentNullException(nameof(configuration), "API URL 'NlToQueryApiUrl' is null.");
    }

    /// <summary>
    /// Converts a natural language query into a SQL query using an external API and the provided product schema.
    /// </summary>
    /// <param name="naturalLanguage">The natural language query to convert.</param>
    /// <param name="product">The product containing database server and schema information.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with the generated SQL query as a string.
    /// Returns an error message if the conversion fails.
    /// </returns>
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
   
    /// <summary>
    /// Extracts and formats SQL queries from the provided content string.
    /// Cleans up the input by removing unnecessary whitespace, code block markers, and the word 'sql'.
    /// Splits the content by semicolons, trims each command, ensures each command ends with a semicolon,
    /// and joins the commands with newlines for readability.
    /// </summary>
    /// <param name="content">The raw content string potentially containing SQL queries.</param>
    /// <returns>
    /// A formatted string containing one or more SQL queries, each ending with a semicolon and separated by newlines.
    /// Returns an empty string if the input is null, empty, or contains only whitespace.
    /// </returns>
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
    /// <summary>
    /// Generates a system prompt for the natural language to SQL conversion engine,
    /// tailored to the specified database server and product schema.
    /// </summary>
    /// <param name="dbServerName">
    /// The name of the database server (e.g., "PostgreSQL" or "SQLServer").
    /// </param>
    /// <param name="productname">
    /// The name of the product, used to locate the corresponding schema file.
    /// </param>
    /// <returns>
    /// A string containing the system prompt, including database-specific instructions and the schema content.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="dbServerName"/> or <paramref name="productname"/> is null or empty.
    /// </exception>
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
            $"needed (e.g., NOW(), AGE()).\n - Join tables correctly using primary/foreign keys only when it is required. \n - Always include WHERE clauses based on user filters.\n - " +
            $"Use explicit column names (never SELECT *).\n - Apply aggregation functions (SUM, COUNT, AVG, etc.) when requested.\n - Format dates in 'YYYY-MM-DD' format.\n - " +
            $"Avoid using reserved keywords as aliases.\n - Use LIMIT and OFFSET for top-N queries (if needed).\n - Always alias tables for clarity." +
            $"\n - NEVER use SUBSTR, SUBSTRING, LEFT, RIGHT,STRFTIME, DATE_FORMAT, or any string manipulation functions to extract date parts from date columns. Use EXTRACT or date_trunc for date parts if needed.\n" +
            $" - All date fields are stored as DATE or TIMESTAMP. You can directly compare date fields without conversions.\n - Always filter dates before aggregation whenever possible to improve query performance.\n " +
            $"### Database Schema:\n **{sqlSchema.Result}\n ### Output Format:\n Only output valid PostgreSQL code. Do not explain the query.Do not guess columns or tables if not in the " +
            $"provided schema. Do not add comments. Do not include any markdown formatting. Be aware the sql injection check mentioned in method Nl-to_sql.HasSqlInjectionPattern";
        }
        else// if (dbServerName.Equals("SQLServer", StringComparison.OrdinalIgnoreCase))
        {
            return $"You are a highly skilled Microsoft SQL Server (version 2022) expert with deep knowledge of US health insurance claims data for payer organizations. " +
                $"You write clean, optimized T-SQL queries using the provided database schema. \n  Your goal is to convert natural language requests into accurate, fully-formed " +
                $"Microsoft SQL Server queries that run against a health claims database. Always select the most efficient approach and the query should be simple as much as possible " +
                $"and accurate  and there should not be any compilation errors\n ### General Rules:\n - Use ANSI SQL where possible.\n - Use only Microsoft SQL Server that supports version 2022 specific functions only when " +
           $"needed (e.g., GETDATE(), DATEDIFF()).\n - Join tables correctly using primary/foreign keys only when it is required. \n - Always include WHERE clauses based on user filters.\n - " +
           $"Use explicit column names (never SELECT *).\n - Apply aggregation functions (SUM, COUNT, AVG, etc.) when requested.\n - Format dates in 'YYYY-MM-DD' format.\n - " +
           $"Avoid using reserved keywords as aliases.\n - Use OFFSET ... FETCH for top-N queries (if needed).\n - " +
           //$"Always alias tables for clarity." +
           $"\n - NEVER use SUBSTR, SUBSTRING, LEFT, RIGHT,STRFTIME, DATE_FORMAT, or any string manipulation functions to extract date parts and date filters\n - All date fields are stored as DATE or DATETIME." +
           $" You can directly compare date fields without conversions.\n - Always filter data before aggregation whenever possible to improve query performance.\n " +
           $"### Database Schema:\n **{sqlSchema.Result}\n ### Output Format:\n Only output valid T-SQL code. Do not explain the query.Do not guess columns or tables if not in the provided schema." +
           $" Do not add comments. Do not include any markdown formatting. Be aware the sql injection check mentioned in method Nl-to_sql.HasSqlInjectionPattern";
        } 
        
      
    }
    /// <summary>
    /// Represents the response from the OpenAI API containing the generated message and model information.
    /// </summary>
    private class OpenAiResponse
    {
        /// <summary>
        /// Gets or sets the message returned by the API.
        /// </summary>
        public message Message { get; set; }

        /// <summary>
        /// Gets or sets the model used to generate the response.
        /// </summary>
        public string Model { get; set; }
    }
    /// <summary>
    /// Represents a message object with role and content, as returned by the OpenAI API.
    /// </summary>
    private class message
    {
        /// <summary>
        /// Gets or sets the role of the message sender (e.g., "system" or "user").
        /// </summary>
        public string role { get; set; }

        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        public string content { get; set; }
    }
}

