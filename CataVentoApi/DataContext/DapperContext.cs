//using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;

namespace CataVentoApi.DataContext
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private string _connectionString; // Mantém a string bruta (URI ou Chave/Valor)

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("PostgresConnection");
        }

        public IDbConnection CreateConnection()
        {
            // O NpgsqlConnectionStringBuilder consegue parsear o formato URI (postgresql://...)
            // e convertê-lo internamente para o formato tradicional de pares chave-valor (Host=...).

            try
            {
                // Cria o builder com a string bruta (seja URI ou Chave/Valor)
                var builder = new NpgsqlConnectionStringBuilder(_connectionString);

                // Retorna uma nova conexão usando a string de pares chave-valor traduzida
                return new NpgsqlConnection(builder.ConnectionString);
            }
            catch (Exception ex)
            {
                // Em caso de falha de parsing, você pode logar ou relançar.
                // Aqui, apenas relançamos para manter a simplicidade.
                throw new InvalidOperationException("Falha ao processar a string de conexão Npgsql. Verifique o formato URI/Chave-Valor.", ex);
            }
        }
    }
}