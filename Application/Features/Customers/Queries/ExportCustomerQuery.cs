using MediatR;

namespace AccuFlow.Application.Features.Customers.Queries
{
    public record ExportCustomerQuery(string? CustomerType, bool? IsActive) : IRequest<byte[]>;
}