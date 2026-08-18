using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Taxes.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Handlers
{
    public class UpdateTaxCommandHandler : IRequestHandler<UpdateTaxCommand>
    {
        private readonly IRepository<TaxEntity> _taxRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTaxCommandHandler(
            IRepository<TaxEntity> taxRepository,
            IUnitOfWork unitOfWork)
        {
            _taxRepository = taxRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateTaxCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var tax = await _taxRepository.FirstOrDefaultAsync(x => x.TaxId == r.TaxId && !x.IsDeleted, cancellationToken);
            if (tax == null) throw new Exception("Tax not found");
            tax.TaxCode = r.TaxCode;
            tax.TaxName = r.TaxName;
            tax.Rate = r.Rate;
            tax.TaxType = r.TaxType;
            tax.IsActive = r.IsActive;
            tax.UpdatedAt = DateTime.UtcNow;
            tax.UpdatedBy = userId.ToString();
            _taxRepository.Update(tax);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}