using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Queries
{
    public record ExportJournalQuery(DateTime? DateFrom, DateTime? DateTo, string? Status, Guid? AccountId) : IRequest<byte[]>;
}