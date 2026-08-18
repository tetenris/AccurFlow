using AccuFlow.Models.Tax;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Commands
{
    public record CreateTaxCommand(CreateTaxRequest Request, Guid UserId) : IRequest;
}