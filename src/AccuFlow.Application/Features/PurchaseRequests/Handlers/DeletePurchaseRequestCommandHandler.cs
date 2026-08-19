using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseRequests.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseRequests.Handlers
{
    public class DeletePurchaseRequestCommandHandler : IRequestHandler<DeletePurchaseRequestCommand>
    {
        private readonly IRepository<PurchaseRequestEntity> _prRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePurchaseRequestCommandHandler(
            IRepository<PurchaseRequestEntity> prRepository,
            IUnitOfWork unitOfWork)
        {
            _prRepository = prRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeletePurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var pr = await _prRepository.Query()
                .FirstOrDefaultAsync(x => x.PurchaseRequestId == request.Id && !x.IsDeleted, cancellationToken);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Draft") throw new Exception("Only draft purchase request can be deleted");
            pr.IsDeleted = true; pr.DeletedAt = DateTime.UtcNow; pr.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}