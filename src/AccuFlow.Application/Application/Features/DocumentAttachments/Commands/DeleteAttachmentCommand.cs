using MediatR;

namespace AccuFlow.Application.Features.DocumentAttachments.Commands
{
    public record DeleteAttachmentCommand(Guid Id, Guid UserId) : IRequest;
}