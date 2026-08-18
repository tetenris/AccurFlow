using AccuFlow.Models.Customer;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Commands
{
    public record UpdateCustomerCommand(UpdateCustomerRequest Request, Guid UserId) : IRequest;
}