using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DocumentAttachments.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.DocumentAttachment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DocumentAttachments.Handlers
{
    public class GetAttachmentDatatableQueryHandler : IRequestHandler<GetAttachmentDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<DocumentAttachmentEntity> _attachmentRepository;

        public GetAttachmentDatatableQueryHandler(IRepository<DocumentAttachmentEntity> attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetAttachmentDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _attachmentRepository.Query().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.DocumentType)) query = query.Where(x => x.DocumentType == r.DocumentType);
            if (r.DocumentId.HasValue) query = query.Where(x => x.DocumentId == r.DocumentId);

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.CreatedAt).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new DocumentAttachmentViewModel
                {
                    DocumentAttachmentId = x.DocumentAttachmentId,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                }).ToListAsync(cancellationToken);
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}