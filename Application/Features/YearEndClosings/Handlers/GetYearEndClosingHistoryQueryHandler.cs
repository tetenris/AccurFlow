using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.YearEndClosings.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.YearEndClosing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.YearEndClosings.Handlers
{
    public class GetYearEndClosingHistoryQueryHandler : IRequestHandler<GetYearEndClosingHistoryQuery, List<YearEndClosingHistoryItem>>
    {
        private readonly IRepository<YearEndClosingEntity> _yearEndClosingRepository;

        public GetYearEndClosingHistoryQueryHandler(IRepository<YearEndClosingEntity> yearEndClosingRepository)
        {
            _yearEndClosingRepository = yearEndClosingRepository;
        }

        public async Task<List<YearEndClosingHistoryItem>> Handle(GetYearEndClosingHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _yearEndClosingRepository.Query()
                .Include(x => x.ClosingJournal)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.FiscalYear)
                .Select(x => new YearEndClosingHistoryItem
                {
                    ClosingId = x.ClosingId,
                    FiscalYear = x.FiscalYear,
                    ClosingDate = x.ClosingDate,
                    ClosingJournalId = x.ClosingJournalId,
                    JournalNumber = x.ClosingJournal != null ? x.ClosingJournal.JournalNumber : null
                }).ToListAsync(cancellationToken);
        }
    }
}