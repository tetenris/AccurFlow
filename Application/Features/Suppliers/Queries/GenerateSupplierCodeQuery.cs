using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Queries
{
    public record GenerateSupplierCodeQuery() : IRequest<string>;
}