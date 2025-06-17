using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

public class DatabaseEngine
{
    private string _claimsConnString = "";
    private string _referenceConnString = "";

    public DatabaseEngine(IConfiguration configuration)
    {
        _claimsConnString = configuration.GetConnectionString($"{DatabaseProduct.Claims}_ConnString")
                           ?? throw new ArgumentNullException(nameof(configuration), $"Connection string {DatabaseProduct.Claims}_ConnString is null.");

        _referenceConnString = configuration.GetConnectionString($"{DatabaseProduct.Reference}_ConnString")
                           ?? throw new ArgumentNullException(nameof(configuration), $"Connection string {DatabaseProduct.Reference}_ConnString is null.");
    }

    public DatabaseProduct product { get; set; }

    public enum DatabaseProduct
    {
        Claims,
        Reference
    }

    public async Task<DataTable> ExecuteQueryAsync(string sqlQuery)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                return new DataTable();
            }           
            using IDbConnection connection = product == DatabaseProduct.Claims ? new SqlConnection(_claimsConnString) : new NpgsqlConnection(_referenceConnString);
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

