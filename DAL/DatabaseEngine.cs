using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

public class DatabaseEngine
{
    private string _sqlConnString = "";
    private string _postgreSqlConnString = "";

    public DatabaseEngine(IConfiguration configuration)
    {
        _sqlConnString = configuration.GetConnectionString($"{QueryLanguage.MSSQL}_ConnString")
                           ?? throw new ArgumentNullException(nameof(configuration), $"Connection string {QueryLanguage.MSSQL}_ConnString is null.");

        _postgreSqlConnString = configuration.GetConnectionString($"{QueryLanguage.PostgreSQL}_ConnString")
                           ?? throw new ArgumentNullException(nameof(configuration), $"Connection string {QueryLanguage.PostgreSQL}_ConnString is null.");
    }

    public QueryLanguage language { get; set; }

    public enum QueryLanguage
    {
        MSSQL,
        PostgreSQL
    }

    public async Task<DataTable> ExecuteQueryAsync(string sqlQuery)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                return new DataTable();
            }           
            using IDbConnection connection = language == QueryLanguage.MSSQL ? new SqlConnection(_sqlConnString) : new NpgsqlConnection(_postgreSqlConnString);
            using IDbCommand command = connection.CreateCommand();
            command.CommandText = sqlQuery;

            var dataTable = new DataTable();
            await ((dynamic)connection).OpenAsync();
            using var reader = await ((dynamic)command).ExecuteReaderAsync();
            dataTable.Load(reader);
            return dataTable;
        }
        catch (Exception ex)
        {
            // Log the exception (you can use any logging framework you prefer)
            Console.WriteLine($"Error executing query: {ex.Message}");
            return new DataTable();
        }
    }
}

