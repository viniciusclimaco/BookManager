using System.Data;
using Microsoft.Data.SqlClient;

namespace BookManager.Infrastructure.Data;

/// <summary>
/// Factory para criar conexões com SQL Server
/// </summary>
public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
