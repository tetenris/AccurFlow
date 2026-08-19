using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Return;
using MediatR;

namespace AccuFlow.Application.Features.Returns.Queries
{
    public record GetReturnDatatableQuery(DataTableReturnRequest Request) : IRequest<BaseDatatableResponse>;
}