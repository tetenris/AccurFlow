using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.CashBankTransfers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.CashBankTransfers.Handlers
{
    public class DeleteTransferCommandHandler : IRequestHandler<DeleteTransferCommand>
    {
        private readonly IRepository<CashBankTransferEntity> _transferRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTransferCommandHandler(
            IRepository<CashBankTransferEntity> transferRepository,
            IUnitOfWork unitOfWork)
        {
            _transferRepository = transferRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _transferRepository.Query()
                .FirstOrDefaultAsync(x => x.TransferId == request.TransferId && !x.IsDeleted, cancellationToken);
            if (transfer == null) throw new Exception("Transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft transfers can be deleted");

            transfer.IsDeleted = true;
            transfer.DeletedAt = DateTime.UtcNow;
            transfer.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}