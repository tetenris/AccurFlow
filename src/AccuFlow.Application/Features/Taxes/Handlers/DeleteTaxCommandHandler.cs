using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Taxes.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Handlers
{
    public class DeleteTaxCommandHandler : IRequestHandler<DeleteTaxCommand>
    {
        private readonly IRepository<TaxEntity> _taxRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTaxCommandHandler(
            IRepository<TaxEntity> taxRepository,
            IUnitOfWork unitOfWork)
        {
            _taxRepository = taxRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteTaxCommand request, CancellationToken cancellationToken)
        {
            var tax = await _taxRepository.FirstOrDefaultAsync(x => x.TaxId == request.TaxId && !x.IsDeleted, cancellationToken);
            if (tax == null) throw new Exception("Tax not found");
            tax.IsDeleted = true;
            tax.DeletedAt = DateTime.UtcNow;
            tax.DeletedBy = request.UserId.ToString();
            _taxRepository.Update(tax);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}