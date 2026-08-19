using AccuFlow.Models.Return;
using MediatR;

namespace AccuFlow.Application.Features.Returns.Queries
{
    public record GetReturnInvoicesQuery(string ReturnType) : IRequest<List<ReturnInvoiceOptionViewModel>>;
}