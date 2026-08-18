using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockTransfers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockTransfers.Handlers
{
    public class DeleteStockTransferCommandHandler : IRequestHandler<DeleteStockTransferCommand>
    {
        private readonly IRepository<StockTransferEntity> _stockTransferRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStockTransferCommandHandler(
            IRepository<StockTransferEntity> stockTransferRepository,
            IUnitOfWork unitOfWork)
        {
            _stockTransferRepository = stockTransferRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteStockTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _stockTransferRepository.Query()
                .FirstOrDefaultAsync(x => x.StockTransferId == request.Id && !x.IsDeleted, cancellationToken);
            if (transfer == null) throw new Exception("Stock transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft stock transfer can be deleted");
            transfer.IsDeleted = true; transfer.DeletedAt = DateTime.UtcNow; transfer.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}