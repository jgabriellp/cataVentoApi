using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Net;

namespace CataVentoApi.Services.Service
{
    public class AzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public AzureStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        public async Task<string> Upload(IFormFile file)
        {
            // 1. Garante que o stream possa ser lido
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            // 2. Cria o Container Client
            // Assumindo que você está injetando IConfiguration no construtor da sua classe.
            //var connectionString = _configuration["Blob:ConnectionString"];
            //var containerName = _configuration["Blob:ContainerName"];

            var connectionString = Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING");
            var containerName = Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME");

            var container = new BlobContainerClient(connectionString, containerName);

            // 3. Cria o Blob Client (referência ao arquivo)
            // Usar um GUID garante que não haverá colisão de nomes.
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            BlobClient blobClient = container.GetBlobClient(fileName);

            // 4. Faz o upload (UploadAsync retorna um Response, mas você precisa do BlobClient para a URL)
            await blobClient.UploadAsync(stream);

            // 5. Retorna a URL pública
            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return false; // Nada para deletar
            }

            // Converte a URL em um objeto Uri para extrair informações
            if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out Uri uri))
            {
                return false; // URL inválida
            }

            // Obtém a referência ao container
            var containerClient = _blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("BLOB_CONTAINER_NAME"));
            //var containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["Blob:ContainerName"]    );

            // Extrai o nome do blob (o caminho do arquivo) da URL
            // Exemplo: de "https://suaconta/imagens-catavento/posts/guid.jpg" para "posts/guid.jpg"
            string blobName = uri.AbsolutePath.Substring(containerClient.Uri.AbsolutePath.Length + 1);

            // Obtém a referência ao arquivo
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Deleta o arquivo
            // O parâmetro DeleteSnapshotsOption.Include garante que quaisquer backups
            // de snapshot do blob sejam deletados junto.
            var response = await blobClient.DeleteIfExistsAsync();

            // Retorna true se o blob existia e foi deletado, ou false se não existia
            return response.Value;
        }
    }
}