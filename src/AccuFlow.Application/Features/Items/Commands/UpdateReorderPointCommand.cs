using AccuFlow.Models.Inventory;
using MediatR;

namespace AccuFlow.Application.Features.Items.Commands
{
    public record UpdateReorderPointCommand(UpdateReorderPointRequest Request, Guid UserId) : IRequest;
}