using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockTransfers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockTransfers.Handlers
{
    public class UpdateStockTransferCommandHandler : IRequestHandler<UpdateStockTransferCommand>
    {
        private readonly IRepository<StockTransferEntity> _stockTransferRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStockTransferCommandHandler(
            IRepository<StockTransferEntity> stockTransferRepository,
            IUnitOfWork unitOfWork)
        {
            _stockTransferRepository = stockTransferRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateStockTransferCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var transfer = await _stockTransferRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.StockTransferId == r.StockTransferId && !x.IsDeleted, cancellationToken);
            if (transfer == null) throw new Exception("Stock transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft stock transfer can be edited");
            if (r.FromWarehouseId == r.ToWarehouseId) throw new Exception("From and to warehouse must be different");

            transfer.TransferDate = r.TransferDate;
            transfer.FromWarehouseId = r.FromWarehouseId;
            transfer.ToWarehouseId = r.ToWarehouseId;
            transfer.Notes = r.Notes;
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = userId.ToString();
            foreach (var existing in transfer.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
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
            if (!transfer.Lines.Any(l => !l.IsDeleted)) throw new Exception("At least one item line is required");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}