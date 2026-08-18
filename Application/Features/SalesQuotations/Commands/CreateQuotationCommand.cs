using AccuFlow.Models.Quotation;
using MediatR;

namespace AccuFlow.Application.Features.SalesQuotations.Commands
{
    public record CreateQuotationCommand(CreateQuotationRequest Request, Guid UserId) : IRequest;
}