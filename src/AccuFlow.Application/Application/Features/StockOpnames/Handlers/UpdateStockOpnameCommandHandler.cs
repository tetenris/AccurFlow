using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class UpdateStockOpnameCommandHandler : IRequestHandler<UpdateStockOpnameCommand>
    {
        private readonly IRepository<StockOpnameEntity> _stockOpnameRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStockOpnameCommandHandler(
            IRepository<StockOpnameEntity> stockOpnameRepository,
            IUnitOfWork unitOfWork)
        {
            _stockOpnameRepository = stockOpnameRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateStockOpnameCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var opname = await _stockOpnameRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.StockOpnameId == r.StockOpnameId && !x.IsDeleted, cancellationToken);
            if (opname == null) throw new Exception("Stock opname not found");
            if (opname.Status != "Draft") throw new Exception("Only draft stock opname can be edited");

            opname.WarehouseId = r.WarehouseId;
            opname.OpnameDate = r.OpnameDate;
            opname.Notes = r.Notes;
            opname.UpdatedAt = DateTime.UtcNow;
            opname.UpdatedBy = userId.ToString();

            foreach (var existing in opname.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

            foreach (var line in r.Lines)
            {
                opname.Lines.Add(new StockOpnameLineEntity
                {
                    StockOpnameLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    SystemQuantity = line.SystemQuantity,
                    ActualQuantity = line.ActualQuantity,
                    DifferenceQuantity = line.ActualQuantity - line.SystemQuantity,
                    Notes = line.Notes,
                    CreatedBy = userId.ToString()
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}