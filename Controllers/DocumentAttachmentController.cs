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
    }
}
