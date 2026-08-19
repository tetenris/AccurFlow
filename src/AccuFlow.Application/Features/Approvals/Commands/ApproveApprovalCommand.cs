using AccuFlow.Models.Approval;
using MediatR;

namespace AccuFlow.Application.Features.Approvals.Commands
{
    public record ApproveApprovalCommand(ApprovalActionRequest Request, Guid UserId) : IRequest;
}