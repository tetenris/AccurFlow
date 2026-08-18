using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockTransfers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockTransfers.Handlers
{
    public class CreateStockTransferCommandHandler : IRequestHandler<CreateStockTransferCommand>
    {
        private readonly IRepository<StockTransferEntity> _stockTransferRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateStockTransferCommandHandler(
            IRepository<StockTransferEntity> stockTransferRepository,
            IUnitOfWork unitOfWork)
        {
            _stockTransferRepository = stockTransferRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.FromWarehouseId == Guid.Empty) throw new Exception("From warehouse is required");
            if (r.ToWarehouseId == Guid.Empty) throw new Exception("To warehouse is required");
            if (r.FromWarehouseId == r.ToWarehouseId) throw new Exception("From and to warehouse must be different");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var transfer = new StockTransferEntity
            {
                StockTransferId = Guid.NewGuid(),
                StockTransferNumber = await GenerateNumberAsync(cancellationToken),
                TransferDate = r.TransferDate,
                FromWarehouseId = r.FromWarehouseId,
                ToWarehouseId = r.ToWarehouseId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };
            foreach (var line in r.Lines)
            {
                if (line.Quantity <= 0) continue;
                transfer.Lines.Add(new StockTransferLineEntity
                {
                    StockTransferLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    CreatedBy = userId.ToString()
                });
            }
            if (transfer.Lines.Count == 0) throw new Exception("At least one item line with quantity above zero is required");
            _stockTransferRepository.Add(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _stockTransferRepository.Query()
                .Where(x => x.StockTransferNumber.StartsWith("ST-"))
                .OrderByDescending(x => x.StockTransferNumber)
                .Select(x => x.StockTransferNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"ST-{next:D5}";
        }
    }
}