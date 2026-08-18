using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Application.Features.JournalEntries.Handlers
{
    public class GetJournalDatatableQueryHandler : IRequestHandler<GetJournalDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<JournalEntryEntity> _journalRepository;

        public GetJournalDatatableQueryHandler(IRepository<JournalEntryEntity> journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetJournalDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            IQueryable<JournalEntryEntity> query = _journalRepository.Query()
                .Where(x => !x.IsDeleted)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PostedByUser);

            if (r.DateFrom.HasValue)
            {
                query = query.Where(x => x.JournalDate >= r.DateFrom.Value);
            }

            if (r.DateTo.HasValue)
            {
                query = query.Where(x => x.JournalDate <= r.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(r.Status))
            {
                query = query.Where(x => x.Status == r.Status);
            }

            if (!string.IsNullOrEmpty(r.JournalType))
            {
                query = query.Where(x => x.JournalType == r.JournalType);
            }

            if (r.AccountId.HasValue)
            {
                query = query.Where(x => x.JournalLines.Any(l => l.AccountId == r.AccountId.Value && !l.IsDeleted));
            }

            if (!string.IsNullOrEmpty(r.Search))
            {
                query = query.Where(x => x.JournalNumber.Contains(r.Search) || x.Description.Contains(r.Search));
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrEmpty(r.OrderBy))
            {
                query = query.OrderBy($"{r.OrderBy} {r.OrderType}");
            }
            else
            {
                query = query.OrderByDescending(x => x.JournalDate).ThenByDescending(x => x.JournalNumber);
            }

            var data = await query
                .Skip((r.Page - 1) * r.Size)
                .Take(r.Size)
                .Select(x => new JournalEntryViewModel
                {
                    JournalId = x.JournalId,
                    JournalNumber = x.JournalNumber,
                    JournalDate = x.JournalDate,
                    Description = x.Description,
                    JournalType = x.JournalType ?? "General",
                    Status = x.Status,
                    TotalDebit = x.TotalDebit,
                    TotalCredit = x.TotalCredit,
                    IsBalanced = x.IsBalanced,
                    PostedDate = x.PostedDate,
                    PostedBy = x.PostedByUser != null ? x.PostedByUser.FullName : null,
                    CreatedBy = x.CreatedByUser != null ? x.CreatedByUser.FullName : "",
                    CreatedAt = x.CreatedAt,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    CanPost = x.CanPost,
                    CanReverse = x.CanReverse
                })
                .ToListAsync(cancellationToken);

            return new BaseDatatableResponse
            {
                Draw = r.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = totalRecords,
                Data = data
            };
        }
    }
}