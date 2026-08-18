using MediatR;

namespace AccuFlow.Application.Features.Customers.Commands
{
    public record DeleteCustomerCommand(Guid CustomerId, Guid UserId) : IRequest;
}