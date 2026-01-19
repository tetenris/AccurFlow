# 🚀 Quick Start Guide - Customer Management

## Step-by-Step Instructions

### 1️⃣ Fix Existing Audit Trail Data

**Run SQL Script:**
```sql
-- Open SQL Server Management Studio or Azure Data Studio
-- Connect to AccuFlow database
-- Execute: fix-audit-trail.sql
```

This will convert all User IDs to User Names in audit fields.

---

### 2️⃣ Start Application

```bash
# Stop any running instance first (Ctrl+C in terminal)
# Then run:
dotnet run
```

Application will:
- ✅ Apply migrations
- ✅ Seed Roles, Users, Chart of Accounts
- ✅ Seed Customers (7 sample customers)
- ✅ Seed Menus
- ✅ Start on https://localhost:54601

---

### 3️⃣ Login

**Default Credentials:**
- **Username**: `admin` or `accountant`
- **Password**: Check `UserSeed.cs` for password

---

### 4️⃣ Access Customer Management

**Navigate to:**
```
Master → Customers
```

---

### 5️⃣ Test Features

#### ✅ View Customer List
- Should see 7 customers (if seeded)
- DataTable with pagination

#### ✅ Add New Customer
1. Click **"Add Customer"** button
2. Fill form (5 tabs):
   - Basic Info (required)
   - Contact Info
   - Address
   - Financial
   - Additional
3. Click **"Save"**
4. Customer code auto-generated (CUST-00008)

#### ✅ View Detail
1. Click **eye icon (👁️)** on any customer
2. Modal opens with 6 tabs
3. Check **Audit tab** → Should show user name (not GUID)

#### ✅ Edit Customer
1. Click **pencil icon (✏️)**
2. Modify any field
3. Click **"Save"**
4. Check audit trail → UpdatedBy should show your name

#### ✅ Toggle Status
1. Click **toggle icon (🔄)**
2. Status changes (Active ↔ Inactive)
3. Inactive customers hidden from dropdowns

#### ✅ Delete Customer
1. Click **trash icon (🗑️)**
2. Confirm deletion
3. Customer soft-deleted (IsDeleted = true)

#### ✅ Search & Filter
1. Select **Customer Type** (Individual/Corporate/Government)
2. Select **Status** (Active/Inactive)
3. Type in **Search box**
4. Click **"Apply Filter"**

#### ✅ Export to Excel
1. Apply filters (optional)
2. Click **"Export Excel"** button
3. Excel file downloads with filtered data

---

### 6️⃣ Verify Audit Trail

**Check that audit trail shows user names:**

1. Create new customer
2. View detail → Audit tab
3. Should see:
   - ✅ Created By: "Admin User" (not GUID)
   - ✅ Created At: Date/time

4. Edit customer
5. View detail → Audit tab
6. Should see:
   - ✅ Updated By: "Admin User" (not GUID)
   - ✅ Updated At: Date/time

---

## 🐛 Troubleshooting

### Issue: Audit trail shows GUID or "00000000-0000-0000-0000-000000000000"

**Solution:**
```sql
-- Run: fix-audit-trail.sql
-- This converts existing GUIDs to user names
```

### Issue: No customers in list

**Solution:**
```sql
-- Option 1: Run seeder via web
Navigate to: Database Seeding → Click "Customers"

-- Option 2: Run SQL script
Execute: run-customer-seed.sql

-- Option 3: Restart application
dotnet run (seeder runs automatically)
```

### Issue: Port already in use

**Solution:**
```powershell
# Find and kill process
Get-Process -Name dotnet | Stop-Process -Force

# Or change port in launchSettings.json
```

### Issue: Cannot login

**Solution:**
```sql
-- Check if users exist
SELECT * FROM Users WHERE IsDeleted = 0;

-- If empty, run seeder
Navigate to: Database Seeding → Click "Users"
```

---

## 📋 Verification Checklist

After setup, verify:

- [ ] Application runs without errors
- [ ] Can login successfully
- [ ] Customer menu visible under Master
- [ ] Customer list loads (7 customers if seeded)
- [ ] Can add new customer
- [ ] Customer code auto-generates (CUST-00008)
- [ ] Can view customer detail (6 tabs)
- [ ] Audit trail shows user name (not GUID)
- [ ] Can edit customer
- [ ] Can toggle status
- [ ] Can delete customer
- [ ] Can search customers
- [ ] Can filter by type and status
- [ ] Can export to Excel
- [ ] All modals work properly
- [ ] No console errors in browser

---

## 🎯 Next Actions

### For Testing:
1. ✅ Follow steps above
2. ✅ Test all features
3. ✅ Verify audit trail
4. ✅ Check export functionality

### For Development:
1. Create **Suppliers** module (use Customer as template)
2. Create **Products** module
3. Create **Tax** module
4. Integrate with Invoice/PO modules

### For Production:
1. Review security settings
2. Configure production database
3. Set up backup strategy
4. Deploy to production server

---

## 📞 Need Help?

**Documentation Files:**
- `.kiro/BASE_TEMPLATE_ROLE.md` - Template & patterns
- `.kiro/PROGRESS_CUSTOMER.md` - Implementation details
- `.kiro/FINAL_SUMMARY.md` - Complete summary
- `.kiro/QUICK_START.md` - This file

**SQL Scripts:**
- `fix-audit-trail.sql` - Fix existing data
- `run-customer-seed.sql` - Manual seed
- `clear-and-reseed-customers.sql` - Fresh start

---

**Last Updated**: 2024-11-28  
**Status**: ✅ Ready to Use  
**Version**: 1.0

🎉 **Happy Coding!** 🎉
