//using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;

namespace CataVentoApi.DataContext
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("PostgresConnection");
        }

        public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    }
}
