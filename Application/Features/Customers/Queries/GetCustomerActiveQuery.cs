using AccuFlow.Models.Customer;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Queries
{
    public record GetCustomerActiveQuery() : IRequest<List<CustomerDropdownViewModel>>;
}