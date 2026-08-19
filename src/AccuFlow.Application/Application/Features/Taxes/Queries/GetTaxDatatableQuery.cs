using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Tax;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Queries
{
    public record GetTaxDatatableQuery(DataTableTaxRequest Request) : IRequest<BaseDatatableResponse>;
}