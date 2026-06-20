using AccuFlow.Models.DocumentAttachment;
using AccuFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    [Authorize]
    public class DocumentAttachmentController : BaseController
    {
        private readonly IDocumentAttachmentService _documentAttachmentService;

        public DocumentAttachmentController(IDocumentAttachmentService documentAttachmentService) : base(documentAttachmentService)
        {
            _documentAttachmentService = documentAttachmentService;
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableDocumentAttachmentRequest request) => Json(await _documentAttachmentService.Datatable(request));

        [HttpGet]
        public async Task<IActionResult> GetByDocument(string documentType, Guid documentId)
        {
            var result = await _documentAttachmentService.GetByDocument(documentType, documentId);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentAttachmentRequest request, IFormFile file)
        {
            try
            {
                var result = await _documentAttachmentService.Upload(request, file, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Attachment uploaded successfully", id = result.DocumentAttachmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var attachment = await _documentAttachmentService.GetFile(id);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), attachment.FilePath);
                if (!System.IO.File.Exists(filePath)) return NotFound();

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, attachment.ContentType, attachment.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Guid id)
        {
            try
            {
                await _documentAttachmentService.Delete(id, _currentUserService!.UserId);
                return Ok(new { success = true, message = "Attachment deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
