using AccuFlow.Models.Supplier;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Commands
{
    public record CreateSupplierCommand(CreateSupplierRequest Request, Guid UserId) : IRequest;
}