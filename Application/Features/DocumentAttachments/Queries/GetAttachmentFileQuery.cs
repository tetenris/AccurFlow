using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.DocumentAttachments.Queries
{
    public record GetAttachmentFileQuery(Guid Id) : IRequest<DocumentAttachmentEntity>;
}