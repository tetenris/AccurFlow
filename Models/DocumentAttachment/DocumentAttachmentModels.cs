using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.DocumentAttachment
{
    public class DataTableDocumentAttachmentRequest : BaseDatatableRequest
    {
        public string? DocumentType { get; set; }
        public Guid? DocumentId { get; set; }
    }

    public class DocumentAttachmentViewModel
    {
        public Guid DocumentAttachmentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
