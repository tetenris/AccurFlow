using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseRequests.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.PurchaseRequest;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseRequests.Handlers
{
    public class CreatePurchaseRequestCommandHandler : IRequestHandler<CreatePurchaseRequestCommand>
    {
        private readonly IRepository<PurchaseRequestEntity> _prRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePurchaseRequestCommandHandler(
            IRepository<PurchaseRequestEntity> prRepository,
            IRepository<UserEntity> userRepository,
            IUnitOfWork unitOfWork)
        {
            _prRepository = prRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreatePurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var pr = new PurchaseRequestEntity
            {
                PurchaseRequestId = Guid.NewGuid(),
                PurchaseRequestNumber = await GenerateNumberAsync(cancellationToken),
                RequestDate = r.RequestDate,
                RequiredDate = r.RequiredDate,
                RequestedBy = string.IsNullOrWhiteSpace(r.RequestedBy) ? await GetCurrentUserNameAsync(userId, cancellationToken) : r.RequestedBy,
                Department = r.Department,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };
            FillTotals(pr, r.Lines, userId);
            _prRepository.Add(pr);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private void FillTotals(PurchaseRequestEntity pr, IEnumerable<CreatePurchaseRequestLineRequest> lines, Guid userId)
        {
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                pr.Lines.Add(new PurchaseRequestLineEntity
                {
                    PurchaseRequestLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                pr.SubTotal += line.Quantity * line.UnitPrice;
                pr.TaxAmount += line.TaxAmount;
                pr.TotalAmount += lineTotal;
            }
        }

        private async Task<string> GetCurrentUserNameAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.Query()
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);
            return user != null && !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user?.UserName ?? userId.ToString();
        }

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _prRepository.Query()
                .Where(x => x.PurchaseRequestNumber.StartsWith("PR-"))
                .OrderByDescending(x => x.PurchaseRequestNumber)
                .Select(x => x.PurchaseRequestNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"PR-{next:D5}";
        }
    }
}