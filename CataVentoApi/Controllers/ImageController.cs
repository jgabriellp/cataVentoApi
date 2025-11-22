using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Dto.ResponseDto;
using CataVentoApi.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        public ImageController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
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
