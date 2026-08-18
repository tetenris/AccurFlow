using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Queries
{
    public record GenerateJournalNumberQuery(DateTime JournalDate, string? JournalType = null) : IRequest<string>;
}