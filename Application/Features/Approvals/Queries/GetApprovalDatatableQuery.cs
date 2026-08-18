using AccuFlow.Models.Approval;
using AccuFlow.Models.BaseModel;
using MediatR;

namespace AccuFlow.Application.Features.Approvals.Queries
{
    public record GetApprovalDatatableQuery(DataTableApprovalRequest Request) : IRequest<BaseDatatableResponse>;
}