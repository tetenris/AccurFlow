using AccuFlow.Models.Supplier;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Queries
{
    public record GetSupplierActiveQuery() : IRequest<List<SupplierDropdownViewModel>>;
}