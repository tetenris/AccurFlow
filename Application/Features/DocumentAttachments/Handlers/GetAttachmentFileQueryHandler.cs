using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DocumentAttachments.Queries;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.DocumentAttachments.Handlers
{
    public class GetAttachmentFileQueryHandler : IRequestHandler<GetAttachmentFileQuery, DocumentAttachmentEntity>
    {
        private readonly IRepository<DocumentAttachmentEntity> _attachmentRepository;

        public GetAttachmentFileQueryHandler(IRepository<DocumentAttachmentEntity> attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        public async Task<DocumentAttachmentEntity> Handle(GetAttachmentFileQuery request, CancellationToken cancellationToken)
        {
            var attachment = await _attachmentRepository.FirstOrDefaultAsync(x => x.DocumentAttachmentId == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new Exception("Attachment not found");
            return attachment;
        }
    }
}