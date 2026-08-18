using AccuFlow.Models.YearEndClosing;
using MediatR;

namespace AccuFlow.Application.Features.YearEndClosings.Commands
{
    public record CloseYearEndClosingCommand(YearEndClosingRequest Request, Guid UserId) : IRequest<YearEndClosingHistoryItem>;
}