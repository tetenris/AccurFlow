using MediatR;

namespace AccuFlow.Application.Features.Customers.Queries
{
    public record ValidateCustomerCodeQuery(string Code, Guid? ExcludeId = null) : IRequest<bool>;
}