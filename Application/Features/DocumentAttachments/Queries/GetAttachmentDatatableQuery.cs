using AccuFlow.Models.BaseModel;
using AccuFlow.Models.DocumentAttachment;
using MediatR;

namespace AccuFlow.Application.Features.DocumentAttachments.Queries
{
    public record GetAttachmentDatatableQuery(DataTableDocumentAttachmentRequest Request) : IRequest<BaseDatatableResponse>;
}