using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class CreateStockOpnameCommandHandler : IRequestHandler<CreateStockOpnameCommand>
    {
        private readonly IRepository<StockOpnameEntity> _stockOpnameRepository;
        private readonly IRepository<WarehouseEntity> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateStockOpnameCommandHandler(
            IRepository<StockOpnameEntity> stockOpnameRepository,
            IRepository<WarehouseEntity> warehouseRepository,
            IUnitOfWork unitOfWork)
        {
            _stockOpnameRepository = stockOpnameRepository;
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateStockOpnameCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var warehouse = await _warehouseRepository.Query()
                .FirstOrDefaultAsync(x => x.WarehouseId == r.WarehouseId && !x.IsDeleted, cancellationToken);
            if (warehouse == null) throw new Exception("Warehouse not found");

            var opname = new StockOpnameEntity
            {
                StockOpnameId = Guid.NewGuid(),
                StockOpnameNumber = await GenerateNumberAsync(cancellationToken),
                OpnameDate = r.OpnameDate,
                WarehouseId = r.WarehouseId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };

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

            _stockOpnameRepository.Add(opname);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _stockOpnameRepository.Query()
                .Where(x => x.StockOpnameNumber.StartsWith("OPN-"))
                .OrderByDescending(x => x.StockOpnameNumber)
                .Select(x => x.StockOpnameNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"OPN-{next:D5}";
        }
    }
}