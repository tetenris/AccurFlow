using AccuFlow.Models.YearEndClosing;
using MediatR;

namespace AccuFlow.Application.Features.YearEndClosings.Queries
{
    public record GetYearEndClosingPreviewQuery(YearEndClosingRequest Request) : IRequest<YearEndClosingPreview>;
}