let currentTransferId = null;
let stItems = [];

$(document).ready(function () {
    initStockTransfer();
});

function initStockTransfer() {
    window.transferTable = $('#transfer_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/StockTransfer/Datatable',
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
            { data: 'stockTransferNumber' },
            { data: 'transferDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'fromWarehouseName' },
            { data: 'toWarehouseName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalQuantity', render: formatNumber },
            { data: null, orderable: false, render: transferActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.transferTable.ajax.reload());
    $('#btn-add-transfer').on('click', openCreateModal);
    $('#btn-save-transfer').on('click', saveTransfer);
    $('#btn-add-line').on('click', () => appendLineRow({}));
    $('#transfer-items-body').on('click', '.st-line-remove', removeLineRow);
    $('#transfer_datatable').on('click', '.btn-stpost', function () { postTransfer($(this).data('id')); });
    $('#transfer_datatable').on('click', '.btn-stdetail', function () { showDetail($(this).data('id')); });
    $('#transfer_datatable').on('click', '.btn-stedit', function () { openEditModal($(this).data('id')); });
    $('#transfer_datatable').on('click', '.btn-stdelete', function () { deleteTransfer($(this).data('id')); });

    loadLookups();
}

function transferActionButtons(row) {
    const id = row.stockTransferId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-danger btn-stdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-stpost" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-stdetail" data-id="${id}">Detail</button><button class="btn btn-sm btn-light-primary btn-stedit" data-id="${id}">Edit</button>${draft}</div>`;
}

function loadLookups() {
    $.get('/Inventory/GetActiveItems', data => { stItems = data || []; });
    $.get('/StockTransfer/GetWarehouses', data => {
        fillSelect('#transfer-from', data, 'warehouseId', x => `${x.warehouseCode} - ${x.warehouseName}`);
        fillSelect('#transfer-to', data, 'warehouseId', x => `${x.warehouseCode} - ${x.warehouseName}`);
    });
}

function openCreateModal() {
    currentTransferId = null;
    $('#transfer-date').val(new Date().toISOString().substring(0, 10));
    $('#transfer-from').val('');
    $('#transfer-to').val('');
    $('#transfer-notes').val('');
    $('#transfer-items-body').empty();
    appendLineRow({});
    $('#transfer_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/StockTransfer/GetById?id=${id}`, data => {
        currentTransferId = data.stockTransferId;
        $('#transfer-date').val(data.transferDate?.substring(0, 10));
        $('#transfer-from').val(data.fromWarehouseId);
        $('#transfer-to').val(data.toWarehouseId);
        $('#transfer-notes').val(data.notes || '');
        $('#transfer-items-body').empty();
        (data.lines || []).forEach(line => appendLineRow({ itemId: line.itemId, quantity: line.quantity }));
        $('#transfer_modal').modal('show');
    });
}

function appendLineRow(line) {
    const tr = $(`<tr class="st-line-row"></tr>`);
    tr.html(`
        <td><select class="form-select form-select-sm st-line-item"></select></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm st-line-qty text-end" value="${line.quantity || 1}" min="0" step="any" /></td>
        <td class="text-end"><button class="btn btn-sm btn-icon btn-light-danger st-line-remove" title="Remove"><i class="ki-duotone ki-trash fs-3"></i></button></td>`);
    fillItemSelect(tr.find('.st-line-item'), line.itemId);
    $('#transfer-items-body').append(tr);
}

function fillItemSelect(select, selected) {
    select.empty();
    select.append('<option value="">Select Item</option>');
    (stItems || []).forEach(item => {
        select.append(`<option value="${item.itemId}" ${item.itemId === selected ? 'selected' : ''}>${item.itemCode} - ${item.itemName}</option>`);
    });
}

function removeLineRow() {
    $(this).closest('tr').remove();
}

function saveTransfer() {
    const request = {
        transferDate: $('#transfer-date').val(),
        fromWarehouseId: $('#transfer-from').val(),
        toWarehouseId: $('#transfer-to').val(),
        notes: $('#transfer-notes').val(),
        lines: $('.st-line-row').map(function () {
            const tr = $(this);
            return { itemId: tr.find('.st-line-item').val(), quantity: Number(tr.find('.st-line-qty').val() || 0) };
        }).get().filter(l => l.itemId && l.quantity > 0)
    };
    if (currentTransferId) request.stockTransferId = currentTransferId;

    $.ajax({
        url: currentTransferId ? '/StockTransfer/Edit' : '/StockTransfer/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#transfer_modal').modal('hide'); window.transferTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save stock transfer')
    });
}

function postTransfer(id) {
    if (!confirm('Post this stock transfer? Stock will be moved between warehouses.')) return;
    $.ajax({ url: '/StockTransfer/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.transferTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to post stock transfer') });
}

function deleteTransfer(id) {
    if (!confirm('Delete this draft stock transfer?')) return;
    $.ajax({ url: '/StockTransfer/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.transferTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete stock transfer') });
}

function showDetail(id) {
    $.get(`/StockTransfer/GetById?id=${id}`, data => {
        const rows = (data.lines || []).map(line => `
            <tr>
                <td>${line.itemCode} - ${line.itemName}</td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
            </tr>`).join('');
        $('#transfer-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.stockTransferNumber}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.transferDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>From:</strong> ${data.fromWarehouseName}</div>
                <div class="col-md-4"><strong>To:</strong> ${data.toWarehouseName}</div>
                <div class="col-md-4"><strong>Total Qty:</strong> ${formatNumber(data.totalQuantity)}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item</th><th class="text-end">Qty</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="2" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>`);
        $('#transfer_detail_modal').modal('show');
    });
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function formatNumber(value) {
    return new Intl.NumberFormat('id-ID', { maximumFractionDigits: 2 }).format(value || 0);
}