using MediatR;

namespace AccuFlow.Application.Features.Customers.Commands
{
    public record ToggleCustomerStatusCommand(Guid CustomerId, Guid UserId) : IRequest;
}