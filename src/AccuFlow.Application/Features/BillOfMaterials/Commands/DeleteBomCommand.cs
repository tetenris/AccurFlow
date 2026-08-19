using MediatR;

namespace AccuFlow.Application.Features.BillOfMaterials.Commands
{
    public record DeleteBomCommand(Guid BomId, Guid UserId) : IRequest;
}