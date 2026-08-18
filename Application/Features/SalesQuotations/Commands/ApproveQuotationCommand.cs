using MediatR;

namespace AccuFlow.Application.Features.SalesQuotations.Commands
{
    public record ApproveQuotationCommand(Guid Id, Guid UserId) : IRequest;
}