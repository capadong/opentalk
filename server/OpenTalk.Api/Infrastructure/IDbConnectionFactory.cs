using System.Data;
using MySqlConnector;

namespace OpenTalk.Infrastructure;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create()
    {
        return new MySqlConnection(_connectionString);
    }
}
