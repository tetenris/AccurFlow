using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Dashboard.Dtos;
using AccuFlow.Application.Features.Dashboard.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Dashboard.Handlers
{
    public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IRepository<PurchaseOrderEntity> _purchaseOrderRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;

        public GetDashboardSummaryQueryHandler(
            IRepository<UserEntity> userRepository,
            IRepository<RoleEntity> roleRepository,
            IRepository<JournalEntryEntity> journalRepository,
            IRepository<InvoiceEntity> invoiceRepository,
            IRepository<PurchaseOrderEntity> purchaseOrderRepository,
            IRepository<PaymentEntity> paymentRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _journalRepository = journalRepository;
            _invoiceRepository = invoiceRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var salesInvoices = _invoiceRepository.Query()
                .Where(x => !x.IsDeleted && x.InvoiceType == "Sales" && x.Status != "Cancelled");
            var purchaseInvoices = _invoiceRepository.Query()
                .Where(x => !x.IsDeleted && x.InvoiceType == "Purchase" && x.Status != "Cancelled");

            var cashMovementsThisMonthTask = _paymentRepository.Query()
                .Where(x => !x.IsDeleted && x.Status != "Cancelled" && x.PaymentDate >= monthStart)
                .SumAsync(x => x.TotalAmount, cancellationToken);

            var activeUsersTask = _userRepository.CountAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);
            var activeRolesTask = _roleRepository.CountAsync(x => !x.IsDeleted && x.IsActive, cancellationToken);
            var draftJournalsTask = _journalRepository.CountAsync(x => !x.IsDeleted && x.Status == "Draft", cancellationToken);
            var postedJournalsTask = _journalRepository.CountAsync(x => !x.IsDeleted && x.Status == "Posted", cancellationToken);
            var openInvoicesTask = salesInvoices.CountAsync(x => x.TotalAmount > x.PaidAmount, cancellationToken);
            var overdueInvoicesTask = salesInvoices.CountAsync(x => x.TotalAmount > x.PaidAmount && x.DueDate.Date < today, cancellationToken);
            var pendingPurchaseOrdersTask = _purchaseOrderRepository.CountAsync(
                x => !x.IsDeleted && x.Status != "Completed" && x.Status != "Cancelled", cancellationToken);
            var receivablesTask = salesInvoices.SumAsync(x => x.TotalAmount - x.PaidAmount, cancellationToken);
            var payablesTask = purchaseInvoices.SumAsync(x => x.TotalAmount - x.PaidAmount, cancellationToken);

            await Task.WhenAll(
                cashMovementsThisMonthTask,
                activeUsersTask,
                activeRolesTask,
                draftJournalsTask,
                postedJournalsTask,
                openInvoicesTask,
                overdueInvoicesTask,
                pendingPurchaseOrdersTask,
                receivablesTask,
                payablesTask);

            var recentActivities = await GetRecentActivitiesAsync(cancellationToken);

            return new DashboardSummaryDto
            {
                ActiveUsers = await activeUsersTask,
                ActiveRoles = await activeRolesTask,
                DraftJournals = await draftJournalsTask,
                PostedJournals = await postedJournalsTask,
                OpenInvoices = await openInvoicesTask,
                OverdueInvoices = await overdueInvoicesTask,
                PendingPurchaseOrders = await pendingPurchaseOrdersTask,
                Receivables = await receivablesTask,
                Payables = await payablesTask,
                CashMovementsThisMonth = await cashMovementsThisMonthTask,
                RecentActivities = recentActivities
            };
        }

        private async Task<List<DashboardActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken)
        {
            var recentJournals = await _journalRepository.Query()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(4)
                .Select(x => new DashboardActivityDto
                {
                    Title = x.JournalNumber,
                    Description = x.Description,
                    Category = "Journal",
                    Date = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var recentInvoices = await _invoiceRepository.Query()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(4)
                .Select(x => new DashboardActivityDto
                {
                    Title = x.InvoiceNumber,
                    Description = x.InvoiceType + " invoice - " + x.Status,
                    Category = "Invoice",
                    Date = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return recentJournals
                .Concat(recentInvoices)
                .OrderByDescending(x => x.Date)
                .Take(6)
                .ToList();
        }
    }
}