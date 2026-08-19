using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class PostStockOpnameCommandHandler : IRequestHandler<PostStockOpnameCommand>
    {
        private readonly IRepository<StockOpnameEntity> _stockOpnameRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PostStockOpnameCommandHandler(
            IRepository<StockOpnameEntity> stockOpnameRepository,
            IRepository<ItemEntity> itemRepository,
            IRepository<StockMovementEntity> stockMovementRepository,
            IUnitOfWork unitOfWork)
        {
            _stockOpnameRepository = stockOpnameRepository;
            _itemRepository = itemRepository;
            _stockMovementRepository = stockMovementRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PostStockOpnameCommand request, CancellationToken cancellationToken)
        {
            var opname = await _stockOpnameRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.StockOpnameId == request.Id && !x.IsDeleted, cancellationToken);
            if (opname == null) throw new Exception("Stock opname not found");
            if (opname.Status == "Posted") throw new Exception("Stock opname is already posted");
            if (opname.Status != "Draft") throw new Exception("Only draft stock opname can be posted");

            var adjustments = opname.Lines.Where(l => l.DifferenceQuantity != 0).ToList();
            foreach (var line in adjustments)
            {
                var unitCost = await _itemRepository.Query()
                    .Where(x => x.ItemId == line.ItemId && !x.IsDeleted)
                    .Select(x => x.PurchasePrice)
                    .FirstOrDefaultAsync(cancellationToken);

                var difference = line.DifferenceQuantity;
                _stockMovementRepository.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = opname.OpnameDate,
                    ItemId = line.ItemId,
                    WarehouseId = opname.WarehouseId,
                    MovementType = "Opname Adjustment",
                    SourceDocumentType = "StockOpname",
                    SourceDocumentId = opname.StockOpnameId,
                    QuantityIn = difference > 0 ? difference : 0,
                    QuantityOut = difference < 0 ? -difference : 0,
                    UnitCost = unitCost,
                    Notes = string.IsNullOrWhiteSpace(line.Notes)
                        ? $"Stock opname {opname.StockOpnameNumber}"
                        : line.Notes,
                    CreatedBy = request.UserId.ToString()
                });
            }

            opname.Status = "Posted";
            opname.UpdatedAt = DateTime.UtcNow;
            opname.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}