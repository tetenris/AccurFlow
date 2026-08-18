using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Tax;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Queries
{
    public record GetVatReportQuery(VatReportRequest Request) : IRequest<BaseDatatableResponse>;
}