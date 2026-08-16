$(document).ready(function () {
    initReturn();
});

let currentReturnId = null;
let returnExistingLines = {};

function initReturn() {
    window.returnTable = $('#return_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Return/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({
                draw: d.draw,
                search: d.search.value || '',
                page: (d.start / d.length) + 1,
                size: d.length,
                returnType: $('#filter-type').val() || null,
                status: $('#filter-status').val() || null
            })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'goodsReturnNumber' },
            { data: 'returnType', render: data => `<span class="badge ${data === 'Sales' ? 'badge-light-warning' : 'badge-light-info'}">${data}</span>` },
            { data: 'returnDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'invoiceNumber' },
            { data: 'partnerName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalAmount', render: formatCurrency },
            { data: null, orderable: false, render: returnActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.returnTable.ajax.reload());
    $('#btn-add-return').on('click', openReturnCreateModal);
    $('#btn-save-return').on('click', saveReturn);
    $('#return-type').on('change', loadReturnInvoices);
    $('#return-invoice-id').on('change', loadReturnInvoiceLines);
    $('#return-items-body').on('input', '.return-qty', updateReturnLineTotal);
    $('#return_datatable').on('click', '.btn-rpost', function () { postReturn($(this).data('id')); });
    $('#return_datatable').on('click', '.btn-rdetail', function () { showReturnDetail($(this).data('id')); });
    $('#return_datatable').on('click', '.btn-redit', function () { openReturnEditModal($(this).data('id')); });
    $('#return_datatable').on('click', '.btn-rdelete', function () { deleteReturn($(this).data('id')); });

    loadReturnWarehouses();
}

function returnActionButtons(row) {
    const id = row.goodsReturnId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-redit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-rdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-rpost" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-rdetail" data-id="${id}">Detail</button>${draft}</div>`;
}

function loadReturnWarehouses() {
    $.get('/Return/GetWarehouses', data => fillSelect('#return-warehouse-id', data, 'warehouseId', x => `${x.warehouseCode} - ${x.warehouseName}`));
}

function loadReturnInvoices() {
    const type = $('#return-type').val() || 'Sales';
    const select = $('#return-invoice-id');
    select.empty();
    select.append('<option value="">Select</option>');
    $.get(`/Return/GetInvoices?returnType=${type}`, data => {
        (data || []).forEach(item => select.append(`<option value="${item.invoiceId}">${item.invoiceNumber} - ${item.partnerName}</option>`));
    });
}

function openReturnCreateModal() {
    currentReturnId = null;
    returnExistingLines = {};
    $('#return-date').val(new Date().toISOString().substring(0, 10));
    $('#return-notes').val('');
    $('#return-warehouse-id').val('');
    $('#return-type').val('Sales');
    $('#return-invoice-id').val('');
    renderReturnLines([]);
    $('#return_modal').modal('show');
    loadReturnInvoices();
}

function openReturnEditModal(id) {
    $.get(`/Return/GetById?id=${id}`, data => {
        currentReturnId = data.goodsReturnId;
        $('#return-date').val(data.returnDate?.substring(0, 10));
        $('#return-notes').val(data.notes || '');
        $('#return-warehouse-id').val(data.warehouseId);
        $('#return-type').val(data.returnType);
        $('#return_modal').modal('show');
        $('#return-invoice-id').val(data.invoiceId);
        loadReturnInvoices();
        returnExistingLines = {};
        (data.lines || []).forEach(line => { returnExistingLines[line.invoiceLineId] = line; });
        setTimeout(() => { $('#return-invoice-id').val(data.invoiceId); loadReturnInvoiceLines(); }, 100);
    });
}

function loadReturnInvoiceLines() {
    const invoiceId = $('#return-invoice-id').val();
    if (!invoiceId) {
        renderReturnLines([]);
        return;
    }
    $.get(`/Return/GetInvoiceLines?invoiceId=${invoiceId}`, data => renderReturnLines(data.lines || []));
}

function renderReturnLines(lines) {
    const body = $('#return-items-body');
    if (!lines.length) {
        body.html('<tr id="return-items-empty"><td colspan="8" class="text-center text-muted py-5">No items left to return for this invoice</td></tr>');
        return;
    }
    body.empty();
    (lines || []).forEach(line => {
        const existing = returnExistingLines[line.invoiceLineId];
        const qty = existing ? existing.quantity : line.remainingQuantity;
        const unitPrice = existing ? existing.unitPrice : line.unitPrice;
        const tax = existing ? existing.taxAmount : line.taxAmount;
        const itemLabel = line.itemCode
            ? `${line.itemCode} - ${line.itemName}`
            : line.description;
        body.append(`
            <tr class="return-item-row" data-invoice-line-id="${line.invoiceLineId}" data-item-id="${line.itemId || ''}">
                <td>${itemLabel}</td>
                <td><input type="text" class="form-control form-control-sm return-desc" value="${line.description}" /></td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatNumber(line.remainingQuantity)}</td>
                <td class="text-end"><input type="number" class="form-control form-control-sm return-qty text-end" value="${qty}" min="0" max="${line.remainingQuantity}" /></td>
                <td class="text-end"><span class="return-price">${formatNumber(unitPrice)}</span></td>
                <td class="text-end"><span class="return-tax">${formatNumber(tax)}</span></td>
                <td class="text-end fw-bold return-total">${formatCurrency(0)}</td>
            </tr>`);
        updateReturnLineTotal.call(body.children().last().find('.return-qty')[0]);
    });
}

function updateReturnLineTotal() {
    const row = $(this).closest('tr');
    const qty = Number(row.find('.return-qty').val() || 0);
    const priceStr = (row.find('.return-price').text() || '0').replace(/,/g, '');
    const taxStr = (row.find('.return-tax').text() || '0').replace(/,/g, '');
    const price = Number(priceStr || 0);
    const tax = Number(taxStr || 0);
    row.find('.return-total').text(formatCurrency((qty * price) + tax));
}

function saveReturn() {
    const request = {
        returnType: $('#return-type').val(),
        returnDate: $('#return-date').val(),
        invoiceId: $('#return-invoice-id').val(),
        warehouseId: $('#return-warehouse-id').val(),
        notes: $('#return-notes').val(),
        lines: $('#return-items-body tr.return-item-row').map(function () {
            const row = $(this);
            const qty = Number(row.find('.return-qty').val() || 0);
            const desc = row.find('.return-desc').val();
            const priceStr = (row.find('.return-price').text() || '0').replace(/,/g, '');
            const taxStr = (row.find('.return-tax').text() || '0').replace(/,/g, '');
            return {
                invoiceLineId: row.data('invoice-line-id'),
                itemId: row.data('item-id') || null,
                description: desc,
                quantity: qty,
                unitPrice: Number(priceStr || 0),
                taxAmount: Number(taxStr || 0)
            };
        }).get().filter(line => line.quantity > 0)
    };
    if (currentReturnId) request.goodsReturnId = currentReturnId;

    $.ajax({
        url: currentReturnId ? '/Return/Edit' : '/Return/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#return_modal').modal('hide'); window.returnTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save return')
    });
}

function postReturn(id) {
    if (!confirm('Post this return? Stock will be decreased and a journal will be posted.')) return;
    $.ajax({ url: '/Return/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.returnTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to post return') });
}

function deleteReturn(id) {
    if (!confirm('Delete this draft return?')) return;
    $.ajax({ url: '/Return/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.returnTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete return') });
}

function showReturnDetail(id) {
    $.get(`/Return/GetById?id=${id}`, data => {
        const rows = (data.lines || []).map(line => `
            <tr>
                <td>${line.itemCode || line.description}</td>
                <td>${line.description}</td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatCurrency(line.unitPrice)}</td>
                <td class="text-end">${formatCurrency(line.taxAmount)}</td>
                <td class="text-end">${formatCurrency(line.lineTotal)}</td>
            </tr>`).join('');
        $('#return-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.goodsReturnNumber}</div>
                <div class="col-md-4"><strong>Type:</strong> ${data.returnType}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.returnDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>Invoice:</strong> ${data.invoiceNumber}</div>
                <div class="col-md-4"><strong>Partner:</strong> ${data.partnerName}</div>
                <div class="col-md-4"><strong>Warehouse:</strong> ${data.warehouseName}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item</th><th>Description</th><th class="text-end">Qty</th><th class="text-end">Price</th><th class="text-end">Tax</th><th class="text-end">Total</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="6" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold">Total: ${formatCurrency(data.totalAmount)}</div>`);
        $('#return_detail_modal').modal('show');
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