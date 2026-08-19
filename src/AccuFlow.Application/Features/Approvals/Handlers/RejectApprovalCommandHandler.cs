using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Approvals.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Approval;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Approvals.Handlers
{
    public class RejectApprovalCommandHandler : IRequestHandler<RejectApprovalCommand>
    {
        private readonly IRepository<ApprovalRequestEntity> _approvalRequestRepository;
        private readonly IRepository<ApprovalHistoryEntity> _approvalHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RejectApprovalCommandHandler(
            IRepository<ApprovalRequestEntity> approvalRequestRepository,
            IRepository<ApprovalHistoryEntity> approvalHistoryRepository,
            IUnitOfWork unitOfWork)
        {
            _approvalRequestRepository = approvalRequestRepository;
            _approvalHistoryRepository = approvalHistoryRepository;
            _unitOfWork = unitOfWork;
        }

        public Task Handle(RejectApprovalCommand request, CancellationToken cancellationToken)
            => ChangeStatus(request.Request, request.UserId, "Rejected", cancellationToken);

        private async Task ChangeStatus(ApprovalActionRequest request, Guid userId, string status, CancellationToken cancellationToken)
        {
            var approval = await _approvalRequestRepository.Query()
                .FirstOrDefaultAsync(x => x.ApprovalRequestId == request.ApprovalRequestId && !x.IsDeleted, cancellationToken);
            if (approval == null) throw new Exception("Approval request not found");
            approval.Status = status;
            approval.UpdatedBy = userId.ToString();
            _approvalHistoryRepository.Add(new ApprovalHistoryEntity
            {
                ApprovalHistoryId = Guid.NewGuid(),
                ApprovalRequestId = approval.ApprovalRequestId,
                ApproverId = userId,
                Action = status,
                ActionAt = DateTime.UtcNow,
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}