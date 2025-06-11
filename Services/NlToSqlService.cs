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
                new { role = "system", content = string.Format("As a professional sql developer, only sql queries should be in response without extra things or descriptions because this will be input for SQL playground. Assume I have a DB with this structure:{0}" ,sqlSchema) },
                new { role = "user", content = naturalLanguage.ToString() }
            },
                stream = false,
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "http://10.143.62.231:11434/api/chat");
            req.Content = JsonContent.Create(request);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(req);

            var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
            return result?.Message?.content ?? string.Empty;
            //return "select top 5 claimid,referralid,enrollid,affiliationid,facilitycode,memid,startdate,enddate from claim"; // Placeholder for actual implementation
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