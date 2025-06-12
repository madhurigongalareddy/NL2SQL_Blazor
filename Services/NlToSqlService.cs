using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class NlToSqlService
{
    private readonly HttpClient _httpClient;
    private readonly string _connectionString = "";

    public NlToSqlService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // Set a timeout for the HTTP requests
        _connectionString = configuration.GetConnectionString("Plandata_connect")
                            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'Plandata_connect' is null.");
    }

    public async Task<string> ConvertToSqlAsync(string naturalLanguage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(naturalLanguage))
            {
                return string.Empty;
            }

            // read file content from sqlschema.txt file 
            var sqlSchema = await File.ReadAllTextAsync("sqlschema.txt");

            var request = new
            {
                model = "llama3.2",
                messages = new[]
                {
                new { role = "system", content = string.Format("As a professional sql developer, only sql queries should be in response without extra things or descriptions because this will be input for Microsoft SQL Server. Assume I have a DB with this structure:{0}" ,sqlSchema) },
                new { role = "user", content = naturalLanguage.ToString() }
            },
                stream = false,
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "http://10.143.60.81:11434/api/chat");
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
             
            return ExtractSqlCommand(rawContent);

            //var rawContent = "```sql\nSELECT COUNT(*)\nFROM claim\nWHERE referralid = 'your_referralid_here';\n\nSELECT COUNT(*)\nFROM claim\nWHERE enrollid = your_enrollid_here;\n```";

            //return "select top 5 claimid, referralid, enrollid, affiliationid, facilitycode, memid, startdate, enddate from claim"; // Placeholder for actual implementation
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public async Task<DataTable> ExecuteSqlAsync(string sql)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        var dataTable = new DataTable();
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        dataTable.Load(reader);
        return dataTable;
    }

    private static string ExtractSqlCommand(string content)
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