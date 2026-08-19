using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Taxes.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Taxes.Handlers
{
    public class CreateTaxCommandHandler : IRequestHandler<CreateTaxCommand>
    {
        private readonly IRepository<TaxEntity> _taxRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTaxCommandHandler(
            IRepository<TaxEntity> taxRepository,
            IUnitOfWork unitOfWork)
        {
            _taxRepository = taxRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateTaxCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var exists = await _taxRepository.AnyAsync(x => x.TaxCode == r.TaxCode && !x.IsDeleted, cancellationToken);
            if (exists) throw new Exception("Tax code already exists");
            _taxRepository.Add(new TaxEntity
            {
                TaxId = Guid.NewGuid(),
                TaxCode = r.TaxCode,
                TaxName = r.TaxName,
                Rate = r.Rate,
                TaxType = r.TaxType,
                IsActive = true,
                CreatedBy = userId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}