# BUSINESS LOGIC & WORKFLOWS

## 1. PURCHASE WORKFLOW (Pembelian)

### Entry Point: AddBeliKidung.vb

### Step-by-Step Process:

#### 1. Form Load
- Load Supply Points (dropdown)
- Load Cars (dropdown)
- Load Drivers (dropdown)
- Load existing temp data (TEMPBELIKIDUNG)

#### 2. Data Entry
- User selects Supply Point → enables form
- User selects Driver
- User selects Car
- User enters Quantity
- System calculates: Total = HargaBeli × Qty
- System updates stock: Stock = Stock + Qty
- Data saved to TEMPBELIKIDUNG (temporary table)

#### 3. Save Transaction (bsimpan_Click)
- Validate: TSold, TShip, TSA, CSP must be filled
- Generate Invoice Number: CekNoFak()
  - Format: KDBUOM-00000001 (incremental)
  - Count existing records + 1
- Save to BELIKIDUNG (from TEMPBELIKIDUNG)
- Save to FAKTURBELIKIDUNG (invoice header)
- Update BELIKIDUNG.FakturBeliId
- Delete TEMPBELIKIDUNG records
- Show success message


### Key Business Rules (Purchase):
1. **Invoice Numbering**: Always increment, no duplication
2. **Stock Management**: Purchase increases stock (Stock + Qty)
3. **Multi-line Support**: Can add multiple items before saving
4. **Soft Delete**: Uses DeletedBy/DeletedDate instead of hard delete
5. **Audit Trail**: Tracks CreatedBy, CreatedDate for all transactions

## 2. SALES WORKFLOW (Penjualan)

### Entry Point: AddJualKidung.vb

### Step-by-Step Process:

#### 1. Form Load
- Load Delivery Points/Customers (dropdown)
- Load Cars (dropdown)
- Load Drivers (dropdown)
- Default Qty = 200

#### 2. Data Entry
- User selects Delivery Point → enables date picker
- User selects Driver → enables Qty input
- User selects Car
- User enters Quantity
- System validates stock availability

#### 3. Save Transaction (bsimpan_Click)
- Validate: CSP, CDriver, CCar must be filled
- Generate Faktur Number: CekNoFak()
  - Format: KDJUOM-00000001
- Generate Invoice Number: CekNoInv()
  - Format: INVJKDUOM-00000001
- Check stock availability
  - If insufficient: Show error, exit
  - If sufficient: Continue
- Update stock: Stock = Stock - Qty
- Save to FAKTURKIDUNG (invoice header)
- Save to JUALKIDUNG (transaction detail)
- Generate & print report (Cetak)
- Show success message


### Key Business Rules (Sales):
1. **Stock Validation**: Must check stock before selling
2. **Stock Deduction**: Sales decreases stock (Stock - Qty)
3. **Dual Numbering**: Faktur (KDJUOM) + Invoice (INVJKDUOM)
4. **Profit Tracking**: Stores both HargaBeli and HargaJual
5. **Auto Print**: Automatically generates report after save
6. **Payment Type**: Defaults to "Cash" (typeTunai)

## 3. STOCK MANAGEMENT

### Stock Calculation Logic:
```
Initial Stock = 0
After Purchase: Stock = Stock + PurchaseQty
After Sales: Stock = Stock - SalesQty
Current Stock = Initial + Total Purchases - Total Sales
```

### Stock History Query (getHistoryStockKidung):
```sql
SELECT 
    X.BarangId, X.Nama, X.Faktur,
    X.QtyBeli, X.QtyJual, X.CreatedDate,
    SUM(X.QtyBeli - X.QtyJual) 
        OVER (PARTITION BY X.BarangId 
              ORDER BY X.CreatedDate 
              ROWS UNBOUNDED PRECEDING) AS Stock
FROM (
    SELECT BarangId, Nama, FakturBeliId AS Faktur,
           Qty AS QtyBeli, 0 AS QtyJual, CreatedDate
    FROM BELIKIDUNG
    UNION ALL
    SELECT BarangId, Nama, FakturJualId AS Faktur,
           0 AS QtyBeli, Qty AS QtyJual, CreatedDate
    FROM JUALKIDUNG
) X
ORDER BY X.BarangId, X.CreatedDate
```

