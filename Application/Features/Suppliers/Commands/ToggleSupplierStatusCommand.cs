using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Commands
{
    public record ToggleSupplierStatusCommand(Guid SupplierId, Guid UserId) : IRequest;
}