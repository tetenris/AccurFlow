using MediatR;

namespace AccuFlow.Application.Features.Customers.Queries
{
    public record GenerateCustomerCodeQuery() : IRequest<string>;
}