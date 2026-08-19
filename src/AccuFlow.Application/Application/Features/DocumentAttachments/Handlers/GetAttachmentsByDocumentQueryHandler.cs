using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DocumentAttachments.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.DocumentAttachment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DocumentAttachments.Handlers
{
    public class GetAttachmentsByDocumentQueryHandler : IRequestHandler<GetAttachmentsByDocumentQuery, List<DocumentAttachmentViewModel>>
    {
        private readonly IRepository<DocumentAttachmentEntity> _attachmentRepository;

        public GetAttachmentsByDocumentQueryHandler(IRepository<DocumentAttachmentEntity> attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        public async Task<List<DocumentAttachmentViewModel>> Handle(GetAttachmentsByDocumentQuery request, CancellationToken cancellationToken)
        {
            return await _attachmentRepository.Query()
                .Where(x => x.DocumentType == request.DocumentType && x.DocumentId == request.DocumentId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
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
                })
                .ToListAsync(cancellationToken);
        }
    }
}