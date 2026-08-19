using AccuFlow.Models.Tax;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Commands
{
    public record UpdateTaxCommand(UpdateTaxRequest Request, Guid UserId) : IRequest;
}