using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class DocumentAttachmentEntity : BaseEntity
    {
        public Guid DocumentAttachmentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
    }
}
