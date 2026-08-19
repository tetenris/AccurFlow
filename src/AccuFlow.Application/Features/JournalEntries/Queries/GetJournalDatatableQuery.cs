using AccuFlow.Models.BaseModel;
using AccuFlow.Models.JournalEntry;
using MediatR;

namespace AccuFlow.Application.Features.JournalEntries.Queries
{
    public record GetJournalDatatableQuery(DataTableJournalEntryRequest Request) : IRequest<BaseDatatableResponse>;
}