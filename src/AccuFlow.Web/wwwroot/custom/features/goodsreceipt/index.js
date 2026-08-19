$(document).ready(function () {
    initGoodsReceipt();
});

let currentGrnId = null;
let grnExistingLines = {};

function initGoodsReceipt() {
    window.grnTable = $('#grn_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/GoodsReceipt/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({
                draw: d.draw,
                search: d.search.value || '',
                page: (d.start / d.length) + 1,
                size: d.length,
                status: $('#filter-status').val() || null
            })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'goodsReceiptNumber' },
            { data: 'receiptDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'purchaseOrderNumber' },
            { data: 'supplierName' },
            { data: 'warehouseName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalAmount', render: formatCurrency },
            { data: null, orderable: false, render: grnActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.grnTable.ajax.reload());
    $('#btn-add-grn').on('click', openGrnCreateModal);
    $('#btn-save-grn').on('click', saveGrn);
    $('#grn-po-id').on('change', loadGrnPoLines);
    $('#grn-items-body').on('input', '.grn-qty', updateGrnLineTotal);
    $('#grn_datatable').on('click', '.btn-gpost', function () { postGrn($(this).data('id')); });
    $('#grn_datatable').on('click', '.btn-gdetail', function () { showGrnDetail($(this).data('id')); });
    $('#grn_datatable').on('click', '.btn-gedit', function () { openGrnEditModal($(this).data('id')); });
    $('#grn_datatable').on('click', '.btn-gdelete', function () { deleteGrn($(this).data('id')); });

    loadGrnLookups();
}

function grnActionButtons(row) {
    const id = row.goodsReceiptId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-gedit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-gdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-gpost" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-gdetail" data-id="${id}">Detail</button>${draft}</div>`;
}

function loadGrnLookups() {
    $.get('/GoodsReceipt/GetWarehouses', data => fillSelect('#grn-warehouse-id', data, 'warehouseId', x => `${x.warehouseCode} - ${x.warehouseName}`));
    $.get('/GoodsReceipt/GetPurchaseOrders', data => fillSelect('#grn-po-id', data, 'purchaseOrderId', x => `${x.purchaseOrderNumber} - ${x.supplierName}`));
}

function openGrnCreateModal() {
    currentGrnId = null;
    grnExistingLines = {};
    $('#grn-date').val(new Date().toISOString().substring(0, 10));
    $('#grn-notes').val('');
    $('#grn-warehouse-id').val('');
    $('#grn-po-id').val('');
    renderGrnLines([]);
    $('#grn_modal').modal('show');
}

function openGrnEditModal(id) {
    $.get(`/GoodsReceipt/GetById?id=${id}`, data => {
        currentGrnId = data.goodsReceiptId;
        $('#grn-date').val(data.receiptDate?.substring(0, 10));
        $('#grn-notes').val(data.notes || '');
        $('#grn-warehouse-id').val(data.warehouseId);
        $('#grn-po-id').val(data.purchaseOrderId);
        $('#grn_modal').modal('show');
        grnExistingLines = {};
        (data.lines || []).forEach(line => { grnExistingLines[line.purchaseOrderLineId] = line; });
        loadGrnPoLines();
    });
}

function loadGrnPoLines() {
    const poId = $('#grn-po-id').val();
    if (!poId) {
        renderGrnLines([]);
        return;
    }
    $.get(`/GoodsReceipt/GetPurchaseOrderLines?purchaseOrderId=${poId}`, data => renderGrnLines(data.lines || []));
}

function renderGrnLines(lines) {
    const body = $('#grn-items-body');
    if (!lines.length) {
        body.html('<tr id="grn-items-empty"><td colspan="8" class="text-center text-muted py-5">No items left to receive for this purchase order</td></tr>');
        return;
    }
    body.empty();
    (lines || []).forEach(line => {
        const existing = grnExistingLines[line.purchaseOrderLineId];
        const qty = existing ? existing.quantity : line.remainingQuantity;
        const unitPrice = existing ? existing.unitPrice : line.unitPrice;
        const tax = existing ? existing.taxAmount : line.taxAmount;
        const itemLabel = line.itemCode
            ? `${line.itemCode} - ${line.itemName}`
            : line.description;
        body.append(`
            <tr class="grn-item-row" data-po-line-id="${line.purchaseOrderLineId}" data-item-id="${line.itemId || ''}">
                <td>${itemLabel}</td>
                <td><input type="text" class="form-control form-control-sm grn-desc" value="${line.description}" /></td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatNumber(line.remainingQuantity)}</td>
                <td class="text-end"><input type="number" class="form-control form-control-sm grn-qty text-end" value="${qty}" min="0" max="${line.remainingQuantity}" /></td>
                <td class="text-end"><span class="grn-price">${formatNumber(unitPrice)}</span></td>
                <td class="text-end"><span class="grn-tax">${formatNumber(tax)}</span></td>
                <td class="text-end fw-bold grn-total">${formatCurrency(0)}</td>
            </tr>`);
        updateGrnLineTotal.call(body.children().last().find('.grn-qty')[0]);
    });
}

function updateGrnLineTotal() {
    const row = $(this).closest('tr');
    const qty = Number(row.find('.grn-qty').val() || 0);
    const priceStr = (row.find('.grn-price').text() || '0').replace(/,/g, '');
    const taxStr = (row.find('.grn-tax').text() || '0').replace(/,/g, '');
    const price = Number(priceStr || 0);
    const tax = Number(taxStr || 0);
    row.find('.grn-total').text(formatCurrency((qty * price) + tax));
}

function saveGrn() {
    const request = {
        receiptDate: $('#grn-date').val(),
        purchaseOrderId: $('#grn-po-id').val(),
        warehouseId: $('#grn-warehouse-id').val(),
        notes: $('#grn-notes').val(),
        lines: $('#grn-items-body tr.grn-item-row').map(function () {
            const row = $(this);
            const qty = Number(row.find('.grn-qty').val() || 0);
            const desc = row.find('.grn-desc').val();
            const priceStr = (row.find('.grn-price').text() || '0').replace(/,/g, '');
            const taxStr = (row.find('.grn-tax').text() || '0').replace(/,/g, '');
            return {
                purchaseOrderLineId: row.data('po-line-id'),
                itemId: row.data('item-id') || null,
                description: desc,
                quantity: qty,
                unitPrice: Number(priceStr || 0),
                taxAmount: Number(taxStr || 0)
            };
        }).get().filter(line => line.quantity > 0)
    };
    if (currentGrnId) request.goodsReceiptId = currentGrnId;

    $.ajax({
        url: currentGrnId ? '/GoodsReceipt/Edit' : '/GoodsReceipt/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#grn_modal').modal('hide'); window.grnTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save goods receipt')
    });
}

function postGrn(id) {
    if (!confirm('Post this goods receipt? Stock will be increased and an inventory journal posted.')) return;
    $.ajax({ url: '/GoodsReceipt/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.grnTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to post goods receipt') });
}

function deleteGrn(id) {
    if (!confirm('Delete this draft goods receipt?')) return;
    $.ajax({ url: '/GoodsReceipt/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.grnTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete goods receipt') });
}

function showGrnDetail(id) {
    $.get(`/GoodsReceipt/GetById?id=${id}`, data => {
        const rows = (data.lines || []).map(line => `
            <tr>
                <td>${line.itemCode || line.description}</td>
                <td>${line.description}</td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatCurrency(line.unitPrice)}</td>
                <td class="text-end">${formatCurrency(line.taxAmount)}</td>
                <td class="text-end">${formatCurrency(line.lineTotal)}</td>
            </tr>`).join('');
        $('#grn-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.goodsReceiptNumber}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.receiptDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>PO:</strong> ${data.purchaseOrderNumber}</div>
                <div class="col-md-4"><strong>Supplier:</strong> ${data.supplierName}</div>
                <div class="col-md-4"><strong>Warehouse:</strong> ${data.warehouseName}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item</th><th>Description</th><th class="text-end">Qty</th><th class="text-end">Price</th><th class="text-end">Tax</th><th class="text-end">Total</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="6" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold">Total: ${formatCurrency(data.totalAmount)}</div>`);
        $('#grn_detail_modal').modal('show');
    });
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function formatCurrency(value) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(value || 0);
}

function formatNumber(value) {
    return new Intl.NumberFormat('id-ID', { maximumFractionDigits: 2 }).format(value || 0);
}