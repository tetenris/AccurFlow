using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockBatches.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockBatches.Handlers
{
    public class ConsumeStockBatchCommandHandler : IRequestHandler<ConsumeStockBatchCommand>
    {
        private readonly IRepository<StockBatchEntity> _stockBatchRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConsumeStockBatchCommandHandler(
            IRepository<StockBatchEntity> stockBatchRepository,
            IUnitOfWork unitOfWork)
        {
            _stockBatchRepository = stockBatchRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ConsumeStockBatchCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            if (r.Quantity <= 0) throw new Exception("Quantity must be greater than zero");

            var batch = await _stockBatchRepository.Query()
                .FirstOrDefaultAsync(x => x.StockBatchId == r.StockBatchId && !x.IsDeleted && x.IsActive, cancellationToken);
            if (batch == null) throw new Exception("Batch not found");

            if (r.Quantity > batch.RemainingQuantity)
                throw new Exception($"Cannot consume more than remaining quantity ({batch.RemainingQuantity})");

            batch.RemainingQuantity -= r.Quantity;
            batch.UpdatedAt = DateTime.UtcNow;
            batch.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}