using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Dto.ResponseDto;
using CataVentoApi.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly AzureStorageService _storageService;

        public ImageController(CloudinaryService cloudinaryService, AzureStorageService storageService)
        {
            _cloudinaryService = cloudinaryService;
            _storageService = storageService;
        }

        [HttpPost("upload")]
        //[Route("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadRequestDto uploadRequestDto)
        {
            if (uploadRequestDto.File == null || uploadRequestDto.File.Length == 0)
            {
                return BadRequest(new { Message = "Nenhum arquivo enviado ou arquivo vazio." });
            }

            try
            {
                string imageUrl = await _storageService.Upload(uploadRequestDto.File);

                return Ok(new { ImageUrl = imageUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro interno ao processar o upload.", Detail = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] ImageUrlToDelete imageUrlToDelete)
        {
            var isDeleted = await _storageService.DeleteFileAsync(imageUrlToDelete.ImageUrl);
            
            if (!isDeleted) return StatusCode(500, "Falha ao deletar a imagem no serviço de nuvem.");

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] UploadRequestDto uploadRequestDto)
        {
            var result = await _cloudinaryService.UploadImageAsync(uploadRequestDto.File);

                if (result.SecureUrl == null)
                {
                    // Erro inesperado no Cloudinary, já tratado no serviço
                    return StatusCode(500, "Falha ao processar o upload no Cloudinary.");
                }

                // Retorna a URL e o Public ID para o Front-end salvar no banco de dados
                var responseDto = new ImageUploadResponseDto
                {
                    SecureUrl = result.SecureUrl.ToString(),
                    PublicId = result.PublicId
                };

                return Ok(responseDto);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImage([FromBody] ImageDeleteRequestDto imageDeleteRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Chama o serviço para deletar no Cloudinary
            var isDeletedFromCloud = await _cloudinaryService.DeleteImageAsync(imageDeleteRequestDto.PublicId);

            if (!isDeletedFromCloud)
            {
                return StatusCode(500, "Falha ao deletar a imagem no serviço de nuvem.");
            }

            return NoContent();
        }

    }
}
