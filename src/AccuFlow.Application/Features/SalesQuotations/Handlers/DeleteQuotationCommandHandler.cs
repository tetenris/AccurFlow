using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesQuotations.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesQuotations.Handlers
{
    public class DeleteQuotationCommandHandler : IRequestHandler<DeleteQuotationCommand>
    {
        private readonly IRepository<SalesQuotationEntity> _quotationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteQuotationCommandHandler(
            IRepository<SalesQuotationEntity> quotationRepository,
            IUnitOfWork unitOfWork)
        {
            _quotationRepository = quotationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteQuotationCommand request, CancellationToken cancellationToken)
        {
            var quote = await _quotationRepository.Query()
                .FirstOrDefaultAsync(x => x.SalesQuotationId == request.Id && !x.IsDeleted, cancellationToken);
            if (quote == null) throw new Exception("Quotation not found");
            if (quote.Status != "Draft") throw new Exception("Only draft quotation can be deleted");
            quote.IsDeleted = true; quote.DeletedAt = DateTime.UtcNow; quote.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}