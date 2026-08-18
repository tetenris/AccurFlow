using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Approvals.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Approval;
using AccuFlow.Models.BaseModel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Approvals.Handlers
{
    public class GetApprovalDatatableQueryHandler : IRequestHandler<GetApprovalDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<ApprovalRequestEntity> _approvalRequestRepository;

        public GetApprovalDatatableQueryHandler(IRepository<ApprovalRequestEntity> approvalRequestRepository)
        {
            _approvalRequestRepository = approvalRequestRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetApprovalDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _approvalRequestRepository.Query()
                .Include(x => x.RequestedByUser)
                .Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.DocumentType)) query = query.Where(x => x.DocumentType == r.DocumentType);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.RequestedAt).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new ApprovalRequestViewModel
                {
                    ApprovalRequestId = x.ApprovalRequestId,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    Status = x.Status,
                    RequestedByName = x.RequestedByUser.FullName,
                    RequestedAt = x.RequestedAt,
                    Notes = x.Notes
                }).ToListAsync(cancellationToken);
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}