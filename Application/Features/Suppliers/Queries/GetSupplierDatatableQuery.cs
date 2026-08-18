using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Supplier;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Queries
{
    public record GetSupplierDatatableQuery(DataTableSupplierRequest Request) : IRequest<BaseDatatableResponse>;
}