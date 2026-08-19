using AccuFlow.Models.Customer;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Commands
{
    public record CreateCustomerCommand(CreateCustomerRequest Request, Guid UserId) : IRequest;
}