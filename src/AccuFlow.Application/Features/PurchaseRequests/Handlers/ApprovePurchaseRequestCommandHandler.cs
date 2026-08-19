using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseRequests.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseRequests.Handlers
{
    public class ApprovePurchaseRequestCommandHandler : IRequestHandler<ApprovePurchaseRequestCommand>
    {
        private readonly IRepository<PurchaseRequestEntity> _prRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApprovePurchaseRequestCommandHandler(
            IRepository<PurchaseRequestEntity> prRepository,
            IUnitOfWork unitOfWork)
        {
            _prRepository = prRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ApprovePurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var pr = await _prRepository.Query()
                .FirstOrDefaultAsync(x => x.PurchaseRequestId == request.Id && !x.IsDeleted, cancellationToken);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Draft") throw new Exception("Only draft purchase request can be approved");
            pr.Status = "Approved";
            pr.UpdatedAt = DateTime.UtcNow;
            pr.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}