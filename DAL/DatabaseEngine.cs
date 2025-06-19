using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

public class DatabaseEngine
{
    private string _connString = "";
    IConfiguration _configuration;    

    public DatabaseEngine(IConfiguration configuration)
    { 
        _configuration = configuration;
    }

    public async Task<DataTable> ExecuteQueryAsync(string sqlQuery, string productName)
    {
        ProductService productService = new ProductService();
        try
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                return new DataTable();
            }

            _connString = _configuration.GetConnectionString($"{productName}_ConnString")
                           ?? throw new ArgumentNullException(nameof(_configuration), $"Connection string {productName}_ConnString is null.");

            var products = await productService.GetProductsAsync();

            IDbConnection connection = products.Where(p => p.ProductName == productName).FirstOrDefault().DBServerName == "MS-SQL"
                ? new SqlConnection(_connString)
                : new NpgsqlConnection(_connString);

            using (connection)
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = sqlQuery;

                var dataTable = new DataTable();
                await ((dynamic)connection).OpenAsync();
                using var reader = await ((dynamic)command).ExecuteReaderAsync();
                dataTable.Load(reader);
                return dataTable;
            }
        }
        catch (Exception ex)
        {
            // Log the exception (you can use any logging framework you prefer)
            Console.WriteLine($"Error executing query: {ex.Message}");
            return new DataTable();
        }
    }

}

