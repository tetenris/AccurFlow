using AccuFlow.Models.Quotation;
using MediatR;

namespace AccuFlow.Application.Features.SalesQuotations.Commands
{
    public record UpdateQuotationCommand(UpdateQuotationRequest Request, Guid UserId) : IRequest;
}