using AccuFlow.Models.DocumentAttachment;
using MediatR;

namespace AccuFlow.Application.Features.DocumentAttachments.Queries
{
    public record GetAttachmentsByDocumentQuery(string DocumentType, Guid DocumentId) : IRequest<List<DocumentAttachmentViewModel>>;
}