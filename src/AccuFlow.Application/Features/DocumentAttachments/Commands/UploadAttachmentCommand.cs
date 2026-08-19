using AccuFlow.Domain.Entities;
using AccuFlow.Models.DocumentAttachment;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AccuFlow.Application.Features.DocumentAttachments.Commands
{
    public record UploadAttachmentCommand(UploadDocumentAttachmentRequest Request, IFormFile File, Guid UserId) : IRequest<DocumentAttachmentEntity>;
}