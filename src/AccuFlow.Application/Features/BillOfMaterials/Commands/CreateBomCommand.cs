using AccuFlow.Models.Production;
using MediatR;

namespace AccuFlow.Application.Features.BillOfMaterials.Commands
{
    public record CreateBomCommand(BomRequest Request, Guid UserId) : IRequest;
}