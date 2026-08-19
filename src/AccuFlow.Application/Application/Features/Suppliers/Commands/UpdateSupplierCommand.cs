using AccuFlow.Models.Supplier;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Commands
{
    public record UpdateSupplierCommand(UpdateSupplierRequest Request, Guid UserId) : IRequest;
}