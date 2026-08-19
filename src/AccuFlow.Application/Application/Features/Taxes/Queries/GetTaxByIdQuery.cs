using AccuFlow.Models.Tax;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Queries
{
    public record GetTaxByIdQuery(Guid TaxId) : IRequest<TaxViewModel?>;
}