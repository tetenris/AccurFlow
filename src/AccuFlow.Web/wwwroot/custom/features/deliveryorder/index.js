let currentDeliveryId = null;
let deliveryExisting = {};

$(document).ready(function () {
    initDeliveryOrder();
});

function initDeliveryOrder() {
    window.deliveryTable = $('#delivery_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/DeliveryOrder/Datatable',
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
            { data: 'deliveryNumber' },
            { data: 'deliveryDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'orderNumber' },
            { data: 'customerName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalAmount', render: formatCurrency },
            { data: null, orderable: false, render: deliveryActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.deliveryTable.ajax.reload());
    $('#btn-add-delivery').on('click', openDeliveryCreateModal);
    $('#btn-save-delivery').on('click', saveDelivery);
    $('#delivery-so-id').on('change', loadDeliverySoLines);
    $('#delivery_datatable').on('click', '.btn-dpost', function () { postDelivery($(this).data('id')); });
    $('#delivery_datatable').on('click', '.btn-ddetail', function () { showDeliveryDetail($(this).data('id')); });
    $('#delivery_datatable').on('click', '.btn-dedit', function () { openDeliveryEditModal($(this).data('id')); });
    $('#delivery_datatable').on('click', '.btn-ddelete', function () { deleteDelivery($(this).data('id')); });
    $('#delivery_datatable').on('click', '.btn-dinvoice', function () { convertToInvoice($(this).data('id')); });

   $.get('/DeliveryOrder/GetOrders', data => fillSelect('#delivery-so-id', data, 'salesOrderId', x => `${x.orderNumber} - ${x.customerName}`));
}

function deliveryActionButtons(row) {
    const id = row.deliveryOrderId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-danger btn-ddelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-dpost" data-id="${id}">Post</button>`
        : '';
    const invoice = row.status === 'Posted' ? `<button class="btn btn-sm btn-light-warning btn-dinvoice" data-id="${id}">To Invoice</button>` : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-ddetail" data-id="${id}">Detail</button><button class="btn btn-sm btn-light-primary btn-dedit" data-id="${id}">Edit</button>${draft}${invoice}</div>`;
}

function openDeliveryCreateModal() {
    currentDeliveryId = null;
    deliveryExisting = {};
    $('#delivery-date').val(new Date().toISOString().substring(0, 10));
    $('#delivery-so-id').val('');
    $('#delivery-notes').val('');
    renderDeliveryLines([]);
    $('#delivery_modal').modal('show');
}

function openDeliveryEditModal(id) {
    $.get(`/DeliveryOrder/GetById?id=${id}`, data => {
        currentDeliveryId = data.deliveryOrderId;
        $('#delivery-date').val(data.deliveryDate?.substring(0, 10));
        $('#delivery-notes').val(data.notes || '');
        $('#delivery-so-id').val(data.salesOrderId);
        $('#delivery_modal').modal('show');
        deliveryExisting = {};
        (data.lines || []).forEach(line => { deliveryExisting[line.salesOrderLineId] = line; });
        loadDeliverySoLines();
    });
}

function loadDeliverySoLines() {
    const soId = $('#delivery-so-id').val();
    if (!soId) {
        renderDeliveryLines([]);
        return;
    }
    $.get(`/DeliveryOrder/GetOrderLines?salesOrderId=${soId}`, data => renderDeliveryLines(data.lines || []));
}

function renderDeliveryLines(lines) {
    const body = $('#delivery-items-body');
    if (!lines.length) {
        body.html('<tr id="delivery-items-empty"><td colspan="7" class="text-center text-muted py-5">No items left to deliver for this sales order</td></tr>');
        return;
    }
    body.empty();
    (lines || []).forEach(line => {
        const existing = deliveryExisting[line.salesOrderLineId];
        const qty = existing ? existing.quantity : line.remainingQuantity;
        const unitPrice = existing ? existing.unitPrice : line.unitPrice;
        const itemLabel = line.itemCode ? `${line.itemCode} - ${line.itemName}` : line.description;
        body.append(`
            <tr class="delivery-item-row" data-so-line-id="${line.salesOrderLineId}" data-item-id="${line.itemId || ''}" data-desc="${escapeHtml(line.description)}">
                <td>${itemLabel}</td>
                <td>${line.description}</td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatNumber(line.remainingQuantity)}</td>
                <td class="text-end"><input type="number" class="form-control form-control-sm delivery-qty text-end" value="${qty}" min="0" max="${line.remainingQuantity}" /></td>
                <td class="text-end"><span class="delivery-price">${formatNumber(unitPrice)}</span></td>
                <td class="text-end fw-bold delivery-total">0</td>
            </tr>`);
        updateDeliveryRowTotal(body.children().last());
    });
}

function updateDeliveryRowTotal(tr) {
    const qty = Number(tr.find('.delivery-qty').val() || 0);
    const priceStr = (tr.find('.delivery-price').text() || '0').replace(/,/g, '');
    tr.find('.delivery-total').text(formatCurrency(qty * Number(priceStr || 0)));
}

function saveDelivery() {
    const request = {
        deliveryDate: $('#delivery-date').val(),
        salesOrderId: $('#delivery-so-id').val(),
        notes: $('#delivery-notes').val(),
        lines: $('#delivery-items-body tr.delivery-item-row').map(function () {
            const row = $(this);
            const qty = Number(row.find('.delivery-qty').val() || 0);
            const priceStr = (row.find('.delivery-price').text() || '0').replace(/,/g, '');
            return {
                salesOrderLineId: row.data('so-line-id'),
                itemId: row.data('item-id') || null,
                description: row.data('desc'),
                quantity: qty,
                unitPrice: Number(priceStr || 0)
            };
        }).get().filter(line => line.quantity > 0)
    };
    if (currentDeliveryId) request.deliveryOrderId = currentDeliveryId;

    $.ajax({
        url: currentDeliveryId ? '/DeliveryOrder/Edit' : '/DeliveryOrder/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#delivery_modal').modal('hide'); window.deliveryTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save delivery order')
    });
}

function postDelivery(id) {
    if (!confirm('Post this delivery order? Stock will be decreased and an inventory journal posted.')) return;
    $.ajax({ url: '/DeliveryOrder/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.deliveryTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to post delivery order') });
}

function deleteDelivery(id) {
    if (!confirm('Delete this draft delivery order?')) return;
    $.ajax({ url: '/DeliveryOrder/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.deliveryTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete delivery order') });
}

function convertToInvoice(id) {
    if (!confirm('Convert this posted delivery order to a sales invoice?')) return;
    $.ajax({ url: '/DeliveryOrder/ConvertToInvoice', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.deliveryTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to convert delivery order') });
}

function showDeliveryDetail(id) {
    $.get(`/DeliveryOrder/GetById?id=${id}`, data => {
        const rows = (data.lines || []).map(line => `
            <tr>
                <td>${line.itemCode || line.description}</td>
                <td>${line.description}</td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatCurrency(line.unitPrice)}</td>
                <td class="text-end">${formatCurrency(line.lineTotal)}</td>
            </tr>`).join('');
        $('#delivery-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.deliveryNumber}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.deliveryDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>Sales Order:</strong> ${data.orderNumber}</div>
                <div class="col-md-4"><strong>Customer:</strong> ${data.customerName}</div>
                <div class="col-md-4"><strong>Invoice:</strong> ${data.invoiceNumber || '-'}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item</th><th>Description</th><th class="text-end">Qty</th><th class="text-end">Price</th><th class="text-end">Total</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="5" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold">Total: ${formatCurrency(data.totalAmount)}</div>`);
        $('#delivery_detail_modal').modal('show');
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