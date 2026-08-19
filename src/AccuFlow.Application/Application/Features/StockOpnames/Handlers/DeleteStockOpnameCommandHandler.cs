using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class DeleteStockOpnameCommandHandler : IRequestHandler<DeleteStockOpnameCommand>
    {
        private readonly IRepository<StockOpnameEntity> _stockOpnameRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStockOpnameCommandHandler(
            IRepository<StockOpnameEntity> stockOpnameRepository,
            IUnitOfWork unitOfWork)
        {
            _stockOpnameRepository = stockOpnameRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteStockOpnameCommand request, CancellationToken cancellationToken)
        {
            var opname = await _stockOpnameRepository.Query()
                .FirstOrDefaultAsync(x => x.StockOpnameId == request.Id && !x.IsDeleted, cancellationToken);
            if (opname == null) throw new Exception("Stock opname not found");
            if (opname.Status != "Draft") throw new Exception("Only draft stock opname can be deleted");

            opname.IsDeleted = true;
            opname.DeletedAt = DateTime.UtcNow;
            opname.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}