using MediatR;

namespace AccuFlow.Application.Features.SalesQuotations.Commands
{
    public record DeleteQuotationCommand(Guid Id, Guid UserId) : IRequest;
}