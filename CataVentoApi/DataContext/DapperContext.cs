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
        // APAGAR
        private readonly IConfiguration _configuration;

        public DapperContext(IConfiguration configuration)
        {
            // APAGAR AS PRÓXIMAS DUAS LINHAS
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("PostgresConnection");

            //// Tenta ler a variável simples e direta do Railway (POSTGRES_URI)
            //string railwayUri = Environment.GetEnvironmentVariable("POSTGRES_URI");

            //if (!string.IsNullOrEmpty(railwayUri))
            //{
            //    // Prioriza o URI do Railway
            //    _connectionString = railwayUri;
            //}
            //else
            //{
            //    // Se não estiver no Railway, usa a string do appsettings.
            //    _connectionString = configuration.GetConnectionString("PostgresConnection");
            //}

            //// Verificação crítica para o log: se a string é nula, lançamos uma exceção clara.
            //if (string.IsNullOrEmpty(_connectionString))
            //{
            //    throw new InvalidOperationException("ERRO: A Connection String está VAZIA. Verifique as variáveis de ambiente POSTGRES_URI e/ou appsettings.json.");
            //}
        }

        public IDbConnection CreateConnection()
        {
            // APAGAR ESTE CÓDIGO COMENTADO E DESCOMENTAR O CÓDIGO ABAIXO DEPOIS DE TESTAR NO RAILWAY
            return new NpgsqlConnection(_connectionString);

            try
            {
                // 1. Cria um objeto Uri com a string lida. Isso remove sujeira e valida o formato.
                var connectionUri = new Uri(_connectionString);

                // 2. Cria o Npgsql builder USANDO O URI. Este é o método mais robusto.
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = connectionUri.Host,
                    Port = connectionUri.Port,
                    Database = connectionUri.AbsolutePath.TrimStart('/'),
                    Username = connectionUri.UserInfo.Split(':')[0],
                    Password = connectionUri.UserInfo.Split(':')[1],
                    // Adicione a opção "TargetSessionAttrs" se o erro de autenticação persistir
                    // TargetSessionAttributes = "read-write", 
                    Pooling = true
                };

                // 3. Retorna a conexão com a string traduzida.
                return new NpgsqlConnection(builder.ConnectionString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Falha crítica ao criar a conexão Npgsql. String usada: '{_connectionString}'.", ex);
            }
        }
    }
}