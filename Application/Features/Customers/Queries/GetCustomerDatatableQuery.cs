using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Customer;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Queries
{
    public record GetCustomerDatatableQuery(DataTableCustomerRequest Request) : IRequest<BaseDatatableResponse>;
}