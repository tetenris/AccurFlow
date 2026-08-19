using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Queries
{
    public record ValidateSupplierCodeQuery(string Code, Guid? ExcludeId = null) : IRequest<bool>;
}