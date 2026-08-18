using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Commands
{
    public record DeleteSupplierCommand(Guid SupplierId, Guid UserId) : IRequest;
}