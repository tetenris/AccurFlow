using MediatR;

namespace AccuFlow.Application.Features.Taxes.Commands
{
    public record DeleteTaxCommand(Guid TaxId, Guid UserId) : IRequest;
}