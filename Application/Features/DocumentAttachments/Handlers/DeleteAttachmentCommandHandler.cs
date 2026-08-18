using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DocumentAttachments.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.DocumentAttachments.Handlers
{
    public class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand>
    {
        private readonly IRepository<DocumentAttachmentEntity> _attachmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public DeleteAttachmentCommandHandler(
            IRepository<DocumentAttachmentEntity> attachmentRepository,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment)
        {
            _attachmentRepository = attachmentRepository;
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
        {
            var attachment = await _attachmentRepository.FirstOrDefaultAsync(x => x.DocumentAttachmentId == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new Exception("Attachment not found");

            var filePath = Path.Combine(_environment.ContentRootPath, attachment.FilePath);
            if (File.Exists(filePath)) File.Delete(filePath);

            attachment.IsDeleted = true;
            attachment.DeletedBy = request.UserId.ToString();
            attachment.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}