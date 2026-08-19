let currentPrId = null;
let prItems = [];

$(document).ready(function () {
    initPurchaseRequest();
});

function initPurchaseRequest() {
    window.prTable = $('#pr_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/PurchaseRequest/Datatable',
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
            { data: 'purchaseRequestNumber' },
            { data: 'requestDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'requiredDate', render: data => data ? new Date(data).toLocaleDateString('en-GB') : '-' },
            { data: 'requestedBy' },
            { data: 'department', render: data => data || '-' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalAmount', render: formatCurrency },
            { data: 'purchaseOrderNumber', render: data => data || '-' },
            { data: null, orderable: false, render: prActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.prTable.ajax.reload());
    $('#btn-add-pr').on('click', openPrCreateModal);
    $('#btn-save-pr').on('click', savePr);
    $('#btn-add-pr-line').on('click', () => appendPrLineRow({}));
    $('#pr-items-body').on('click', '.pr-line-remove', removePrLineRow);
    $('#pr-items-body').on('change', '.pr-line-item', onPrLineItemChange);
    $('#pr-items-body').on('input', '.pr-line-input', updatePrLineTotal);
    $('#btn-confirm-convert').on('click', confirmConvert);
    $('#pr_datatable').on('click', '.btn-prapprove', function () { approvePr($(this).data('id')); });
    $('#pr_datatable').on('click', '.btn-prdetail', function () { showPrDetail($(this).data('id')); });
    $('#pr_datatable').on('click', '.btn-predit', function () { openPrEditModal($(this).data('id')); });
    $('#pr_datatable').on('click', '.btn-prdelete', function () { deletePr($(this).data('id')); });
    $('#pr_datatable').on('click', '.btn-prconvert', function () { openPrConvertModal($(this).data('id')); });

    loadPrLookups();
}

function prActionButtons(row) {
    const id = row.purchaseRequestId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-danger btn-prdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-prapprove" data-id="${id}">Approve</button>`
        : '';
    const convert = row.status === 'Approved' && !row.purchaseOrderNumber
        ? `<button class="btn btn-sm btn-light-warning btn-prconvert" data-id="${id}">Convert</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-prdetail" data-id="${id}">Detail</button><button class="btn btn-sm btn-light-primary btn-predit" data-id="${id}">Edit</button>${draft}${convert}</div>`;
}

function loadPrLookups() {
    $.get('/Inventory/GetActiveItems', data => { prItems = data || []; });
    $.get('/Supplier/GetActiveSuppliers', data => fillSelect('#convert-supplier-id', data, 'supplierId', x => `${x.supplierCode} - ${x.supplierName}`));
}

function openPrCreateModal() {
    currentPrId = null;
    $('#pr-date').val(new Date().toISOString().substring(0, 10));
    $('#pr-required-date').val('');
    $('#pr-requested-by').val('');
    $('#pr-department').val('');
    $('#pr-notes').val('');
    $('#pr-items-body').empty();
    appendPrLineRow({});
    $('#pr_modal').modal('show');
}

function openPrEditModal(id) {
    $.get(`/PurchaseRequest/GetById?id=${id}`, data => {
        currentPrId = data.purchaseRequestId;
        $('#pr-date').val(data.requestDate?.substring(0, 10));
        $('#pr-required-date').val(data.requiredDate?.substring(0, 10) || '');
        $('#pr-requested-by').val(data.requestedBy || '');
        $('#pr-department').val(data.department || '');
        $('#pr-notes').val(data.notes || '');
        $('#pr-items-body').empty();
        (data.lines || []).forEach(line => appendPrLineRow(line));
        $('#pr_modal').modal('show');
    });
}

function openPrConvertModal(id) {
    currentPrId = id;
    $('#convert-supplier-id').val('');
    $('#convert-expected-date').val('');
    $('#pr_convert_modal').modal('show');
}

function confirmConvert() {
    const supplierId = $('#convert-supplier-id').val();
    if (!supplierId) return alert('Choose a supplier first');
    $.ajax({
        url: '/PurchaseRequest/ConvertToPurchaseOrder',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            purchaseRequestId: currentPrId,
            supplierId: supplierId,
            expectedDate: $('#convert-expected-date').val() || null
        }),
        success: res => {
            $('#pr_convert_modal').modal('hide');
            window.prTable.ajax.reload();
            alert(res.message);
        },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to convert purchase request')
    });
}

function appendPrLineRow(line) {
    const tr = $(`<tr class="pr-line-row"></tr>`);
    tr.html(`
        <td><select class="form-select form-select-sm pr-line-item"></select></td>
        <td><input type="text" class="form-control form-control-sm pr-line-desc" value="${escapeHtml(line.description || '')}" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm pr-line-qty pr-line-input text-end" value="${line.quantity || 1}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm pr-line-price pr-line-input text-end" value="${line.unitPrice || 0}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm pr-line-tax pr-line-input text-end" value="${line.taxAmount || 0}" min="0" step="any" /></td>
        <td class="text-end fw-bold pr-line-total">0</td>
        <td class="text-end"><button class="btn btn-sm btn-icon btn-light-danger pr-line-remove" title="Remove"><i class="ki-duotone ki-trash fs-3"></i></button></td>`);
    fillItemSelect(tr.find('.pr-line-item'), line.itemId);
    $('#pr-items-body').append(tr);
    updatePrRowTotal(tr);
}

function fillItemSelect(select, selected) {
    select.empty();
    select.append('<option value="">Select Item</option>');
    (prItems || []).forEach(item => {
        select.append(`<option value="${item.itemId}" ${item.itemId === selected ? 'selected' : ''}>${item.itemCode} - ${item.itemName}</option>`);
    });
}

function onPrLineItemChange() {
    const tr = $(this).closest('tr');
    const item = (prItems || []).find(x => x.itemId === $(this).val());
    if (item) {
        tr.find('.pr-line-desc').val(`${item.itemCode} - ${item.itemName}`);
        tr.find('.pr-line-price').val(item.purchasePrice || 0);
        updatePrRowTotal(tr);
    }
}

function removePrLineRow() {
    $(this).closest('tr').remove();
}

function updatePrLineTotal() {
    updatePrRowTotal($(this).closest('tr'));
}

function updatePrRowTotal(tr) {
    const qty = Number(tr.find('.pr-line-qty').val() || 0);
    const price = Number(tr.find('.pr-line-price').val() || 0);
    const tax = Number(tr.find('.pr-line-tax').val() || 0);
    tr.find('.pr-line-total').text(formatNumber((qty * price) + tax));
}

function savePr() {
    const request = {
        requestDate: $('#pr-date').val(),
        requiredDate: $('#pr-required-date').val() || null,
        requestedBy: $('#pr-requested-by').val(),
        department: $('#pr-department').val(),
        notes: $('#pr-notes').val(),
        lines: $('.pr-line-row').map(function () {
            const tr = $(this);
            return {
                itemId: tr.find('.pr-line-item').val() || null,
                description: tr.find('.pr-line-desc').val(),
                quantity: Number(tr.find('.pr-line-qty').val() || 0),
                unitPrice: Number(tr.find('.pr-line-price').val() || 0),
                taxAmount: Number(tr.find('.pr-line-tax').val() || 0)
            };
        }).get().filter(l => l.quantity > 0)
    };
    if (currentPrId) request.purchaseRequestId = currentPrId;

    $.ajax({
        url: currentPrId ? '/PurchaseRequest/Edit' : '/PurchaseRequest/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#pr_modal').modal('hide'); window.prTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save purchase request')
    });
}

function approvePr(id) {
    if (!confirm('Approve this purchase request?')) return;
    $.ajax({ url: '/PurchaseRequest/Approve', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.prTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to approve purchase request') });
}

function deletePr(id) {
    if (!confirm('Delete this draft purchase request?')) return;
    $.ajax({ url: '/PurchaseRequest/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.prTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete purchase request') });
}

function showPrDetail(id) {
    $.get(`/PurchaseRequest/GetById?id=${id}`, data => {
        const rows = (data.lines || []).map(line => `
            <tr>
                <td>${line.itemCode || line.description}</td>
                <td>${line.description}</td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatCurrency(line.unitPrice)}</td>
                <td class="text-end">${formatCurrency(line.taxAmount)}</td>
                <td class="text-end">${formatCurrency(line.lineTotal)}</td>
            </tr>`).join('');
        $('#pr-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.purchaseRequestNumber}</div>
                <div class="col-md-4"><strong>Request Date:</strong> ${new Date(data.requestDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Required:</strong> ${data.requiredDate ? new Date(data.requiredDate).toLocaleDateString('en-GB') : '-'}</div>
                <div class="col-md-4"><strong>Requested By:</strong> ${data.requestedBy}</div>
                <div class="col-md-4"><strong>Department:</strong> ${data.department || '-'}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>Purchase Order:</strong> ${data.purchaseOrderNumber || '-'}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item</th><th>Description</th><th class="text-end">Qty</th><th class="text-end">Price</th><th class="text-end">Tax</th><th class="text-end">Total</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="6" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold">Total: ${formatCurrency(data.totalAmount)}</div>`);
        $('#pr_detail_modal').modal('show');
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

function escapeHtml(value) {
    return $('<div>').text(value || '').html();
}