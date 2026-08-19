using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Quotation;
using MediatR;

namespace AccuFlow.Application.Features.SalesQuotations.Queries
{
    public record GetQuotationDatatableQuery(DataTableQuotationRequest Request) : IRequest<BaseDatatableResponse>;
}