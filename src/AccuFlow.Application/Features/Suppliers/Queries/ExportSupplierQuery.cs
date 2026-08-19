using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Queries
{
    public record ExportSupplierQuery(string? SupplierType, bool? IsActive) : IRequest<byte[]>;
}