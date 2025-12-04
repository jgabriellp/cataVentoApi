// CataVentoApi/DataContext/DapperContext.cs

using Npgsql;
using System.Data;
using System;
using Microsoft.Extensions.Configuration;

namespace CataVentoApi.DataContext
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            // Tenta ler a variável simples e direta do Railway (POSTGRES_URI)
            string railwayUri = Environment.GetEnvironmentVariable("POSTGRES_URI");

            if (!string.IsNullOrEmpty(railwayUri))
            {
                // Prioriza o URI do Railway
                _connectionString = railwayUri;
            }
            else
            {
                // Se não estiver no Railway, usa a string do appsettings.
                _connectionString = configuration.GetConnectionString("PostgresConnection");
            }

            // Verificação crítica para o log: se a string é nula, lançamos uma exceção clara.
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("ERRO: A Connection String está VAZIA. Verifique as variáveis de ambiente POSTGRES_URI e/ou appsettings.json.");
            }
        }

        public IDbConnection CreateConnection()
        {
            try
            {
                // O NpgsqlConnectionStringBuilder deve aceitar o formato URI.
                // Se falhar aqui, o valor LITERAL está incorreto.
                var builder = new NpgsqlConnectionStringBuilder(_connectionString);

                return new NpgsqlConnection(builder.ConnectionString);
            }
            catch (Exception ex)
            {
                // Lançamos uma exceção que mostra o valor que falhou no parsing
                throw new InvalidOperationException(
                    $"Falha crítica no parsing da string Npgsql. Valor lido: '{_connectionString}'. Verifique a sintaxe da URL URI.", ex);
            }
        }
    }
}