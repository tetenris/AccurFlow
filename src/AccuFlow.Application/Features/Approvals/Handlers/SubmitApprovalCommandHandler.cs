using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Approvals.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Approvals.Handlers
{
    public class SubmitApprovalCommandHandler : IRequestHandler<SubmitApprovalCommand>
    {
        private readonly IRepository<ApprovalRequestEntity> _approvalRequestRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitApprovalCommandHandler(
            IRepository<ApprovalRequestEntity> approvalRequestRepository,
            IUnitOfWork unitOfWork)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(SubmitApprovalCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            _approvalRequestRepository.Add(new ApprovalRequestEntity
            {
                ApprovalRequestId = Guid.NewGuid(),
                DocumentType = request.Request.DocumentType,
                DocumentId = request.Request.DocumentId,
                Status = "Pending",
                RequestedBy = userId,
                RequestedAt = DateTime.UtcNow,
                CurrentApproverId = request.Request.CurrentApproverId,
                Notes = request.Request.Notes,
                CreatedBy = userId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}