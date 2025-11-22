using System.Data;
using Microsoft.Data.SqlClient;

namespace CataVentoApi.DataContext
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqlConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
