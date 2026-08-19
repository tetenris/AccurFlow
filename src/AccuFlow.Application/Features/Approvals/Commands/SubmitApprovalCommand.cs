using AccuFlow.Models.Approval;
using MediatR;

namespace AccuFlow.Application.Features.Approvals.Commands
{
    public record SubmitApprovalCommand(SubmitApprovalRequest Request, Guid UserId) : IRequest;
}