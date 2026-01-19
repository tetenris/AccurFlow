# CODE EXAMPLES - WEB MIGRATION

## Backend Examples (ASP.NET Core)

### 1. Purchase Controller
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    
    [HttpPost]
    public async Task<ActionResult<PurchaseResponse>> CreatePurchase(
        [FromBody] CreatePurchaseRequest request)
    {
        try
        {
            var result = await _purchaseService.CreatePurchaseAsync(request);
            return Ok(new ApiResponse<PurchaseResponse>
            {
                Success = true,
                Data = result,
                Message = "Purchase created successfully"
            });
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Error = new ErrorDetail
                {
                    Code = "INSUFFICIENT_STOCK",
                    Message = ex.Message
                }
            });
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<PurchaseDto>>> GetPurchases(
        [FromQuery] PurchaseFilter filter)
    {
        var result = await _purchaseService.GetPurchasesAsync(filter);
        return Ok(result);
    }
}
```


### 2. Purchase Service
```csharp
public class PurchaseService : IPurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly IInvoiceNumberGenerator _invoiceGenerator;
    private readonly IStockService _stockService;
    
    public async Task<PurchaseResponse> CreatePurchaseAsync(
        CreatePurchaseRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Generate invoice number
            var invoiceNumber = await _invoiceGenerator
                .GenerateAsync("PURCHASE");
            
            // Create purchase header
            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                SupplyPointId = request.SupplyPointId,
                TransactionDate = request.TransactionDate,
                CreatedBy = GetCurrentUser(),
                CreatedDate = DateTime.UtcNow
            };
            
            // Create purchase details
            foreach (var item in request.Items)
            {
                var detail = new PurchaseDetail
                {
                    Id = Guid.NewGuid(),
                    PurchaseId = purchase.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = item.Quantity * item.UnitPrice
                };
                
                purchase.Details.Add(detail);
                
                // Update stock
                await _stockService.IncreaseStockAsync(
                    item.ProductId, 
                    item.Quantity);
            }
            
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return MapToResponse(purchase);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```


### 3. Invoice Number Generator
```csharp
public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly ApplicationDbContext _context;
    
    public async Task<string> GenerateAsync(string type)
    {
        // Use database sequence or stored procedure
        var counter = await _context.InvoiceCounters
            .FirstOrDefaultAsync(c => c.Type == type);
            
        if (counter == null)
        {
            counter = new InvoiceCounter
            {
                Type = type,
                CurrentNumber = 0,
                Prefix = GetPrefix(type)
            };
            _context.InvoiceCounters.Add(counter);
        }
        
        counter.CurrentNumber++;
        await _context.SaveChangesAsync();
        
        return $"{counter.Prefix}{counter.CurrentNumber:D8}";
    }
    
    private string GetPrefix(string type)
    {
        return type switch
        {
            "PURCHASE" => "KDBUOM-",
            "SALES_INVOICE" => "KDJUOM-",
            "SALES_FAKTUR" => "INVJKDUOM-",
            _ => "INV-"
        };
    }
}
```

## Frontend Examples (React + TypeScript)

### 1. Purchase Form Component
```typescript
import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQuery } from '@tanstack/react-query';
import { purchaseApi } from '@/api/purchase';

interface PurchaseFormData {
  supplyPointId: string;
  driverId: string;
  carId: string;
  transactionDate: Date;
  items: PurchaseItem[];
}

export const PurchaseForm: React.FC = () => {
  const { register, handleSubmit, formState: { errors } } = 
    useForm<PurchaseFormData>();
  
  const createPurchase = useMutation({
    mutationFn: purchaseApi.create,
    onSuccess: () => {
      toast.success('Purchase created successfully');
      navigate('/purchases');
    },
    onError: (error) => {
      toast.error(error.message);
    }
  });
  
  const onSubmit = (data: PurchaseFormData) => {
    createPurchase.mutate(data);
  };
  
  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <FormControl error={!!errors.supplyPointId}>
        <InputLabel>Supply Point</InputLabel>
        <Select {...register('supplyPointId', { required: true })}>
          {/* Options */}
        </Select>
      </FormControl>
      
      {/* More fields */}
      
      <Button type="submit" disabled={createPurchase.isPending}>
        {createPurchase.isPending ? 'Saving...' : 'Save Purchase'}
      </Button>
    </form>
  );
};
```

