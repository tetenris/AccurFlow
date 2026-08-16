let currentOrderId = null;
let orderItems = [];

$(document).ready(function () {
    initSalesOrder();
});

function initSalesOrder() {
    window.orderTable = $('#order_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/SalesOrder/Datatable',
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
            { data: 'orderNumber' },
            { data: 'orderDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'expectedDate', render: data => data ? new Date(data).toLocaleDateString('en-GB') : '-' },
            { data: 'customerName' },
            { data: 'quotationNumber', render: data => data || '-' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalAmount', render: formatCurrency },
            { data: null, orderable: false, render: orderActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.orderTable.ajax.reload());
    $('#btn-add-order').on('click', openOrderCreateModal);
    $('#btn-save-order').on('click', saveOrder);
    $('#btn-add-order-line').on('click', () => appendOrderLineRow({}));
    $('#order-items-body').on('click', '.order-line-remove', removeOrderLineRow);
    $('#order-items-body').on('change', '.order-line-item', onOrderLineItemChange);
    $('#order-items-body').on('input', '.order-line-input', updateOrderLineTotal);
    $('#order-quote-id').on('change', loadOrderQuoteLines);
    $('#order_datatable').on('click', '.btn-sapprove', function () { approveOrder($(this).data('id')); });
    $('#order_datatable').on('click', '.btn-sdetail', function () { showOrderDetail($(this).data('id')); });
    $('#order_datatable').on('click', '.btn-sedit', function () { openOrderEditModal($(this).data('id')); });
    $('#order_datatable').on('click', '.btn-sdelete', function () { deleteOrder($(this).data('id')); });

    loadOrderLookups();
}

function orderActionButtons(row) {
    const id = row.salesOrderId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-danger btn-sdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-sapprove" data-id="${id}">Approve</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-sdetail" data-id="${id}">Detail</button><button class="btn btn-sm btn-light-primary btn-sedit" data-id="${id}">Edit</button>${draft}</div>`;
}

function loadOrderLookups() {
    $.get('/Inventory/GetActiveItems', data => { orderItems = data || []; });
    $.get('/Customer/GetActiveCustomers', data => fillSelect('#order-customer-id', data, 'customerId', x => `${x.customerCode} - ${x.customerName}`));
    $.get('/SalesOrder/GetApprovedQuotes', data => fillSelect('#order-quote-id', data, 'salesQuotationId', x => `${x.quotationNumber} - ${x.customerName}`));
}

function openOrderCreateModal() {
    currentOrderId = null;
    $('#order-date').val(new Date().toISOString().substring(0, 10));
    $('#order-expected-date').val('');
    $('#order-customer-id').val('');
    $('#order-quote-id').val('');
    $('#order-notes').val('');
    $('#order-items-body').empty();
    appendOrderLineRow({});
    $('#order_modal').modal('show');
}

function openOrderEditModal(id) {
    $.get(`/SalesOrder/GetById?id=${id}`, data => {
        currentOrderId = data.salesOrderId;
        $('#order-date').val(data.orderDate?.substring(0, 10));
        $('#order-expected-date').val(data.expectedDate?.substring(0, 10) || '');
        $('#order-notes').val(data.notes || '');
        $('#order-customer-id').val(data.customerId);
        $('#order-quote-id').val(data.quotationId || '');
        $('#order-items-body').empty();
        (data.lines || []).forEach(line => appendOrderLineRow(line));
        $('#order_modal').modal('show');
    });
}

function loadOrderQuoteLines() {
    const quoteId = $('#order-quote-id').val();
    if (!quoteId) return;
    $.get(`/SalesOrder/GetQuoteById?id=${quoteId}`, data => {
        if (!data) return;
        $('#order-customer-id').val(data.customerId || '');
        $('#order-items-body').empty();
        (data.lines || []).forEach(line => appendOrderLineRow(line));
    });
}

function appendOrderLineRow(line) {
    const tr = $(`<tr class="order-line-row"></tr>`);
    tr.html(`
        <td><select class="form-select form-select-sm order-line-item"></select></td>
        <td><input type="text" class="form-control form-control-sm order-line-desc" value="${escapeHtml(line.description || '')}" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm order-line-qty order-line-input text-end" value="${line.quantity || 1}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm order-line-price order-line-input text-end" value="${line.unitPrice || 0}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm order-line-disc order-line-input text-end" value="${line.discountAmount || 0}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm order-line-tax order-line-input text-end" value="${line.taxAmount || 0}" min="0" step="any" /></td>
        <td class="text-end fw-bold order-line-total">0</td>
        <td class="text-end"><button class="btn btn-sm btn-icon btn-light-danger order-line-remove" title="Remove"><i class="ki-duotone ki-trash fs-3"></i></button></td>`);
    fillItemSelect(tr.find('.order-line-item'), line.itemId);
    $('#order-items-body').append(tr);
    updateOrderRowTotal(tr);
}

function fillItemSelect(select, selected) {
    select.empty();
    select.append('<option value="">Select Item</option>');
    (orderItems || []).forEach(item => {
        select.append(`<option value="${item.itemId}" ${item.itemId === selected ? 'selected' : ''}>${item.itemCode} - ${item.itemName}</option>`);
    });
}

function onOrderLineItemChange() {
    const tr = $(this).closest('tr');
    const item = (orderItems || []).find(x => x.itemId === $(this).val());
    if (item) {
        tr.find('.order-line-desc').val(`${item.itemCode} - ${item.itemName}`);
        tr.find('.order-line-price').val(item.salesPrice || 0);
        updateOrderRowTotal(tr);
    }
}

function removeOrderLineRow() {
    $(this).closest('tr').remove();
}

function updateOrderLineTotal() {
    updateOrderRowTotal($(this).closest('tr'));
}

function updateOrderRowTotal(tr) {
    const qty = Number(tr.find('.order-line-qty').val() || 0);
    const price = Number(tr.find('.order-line-price').val() || 0);
    const disc = Number(tr.find('.order-line-disc').val() || 0);
    const tax = Number(tr.find('.order-line-tax').val() || 0);
    tr.find('.order-line-total').text(formatNumber((qty * price) - disc + tax));
}

function saveOrder() {
    const request = {
        orderDate: $('#order-date').val(),
        expectedDate: $('#order-expected-date').val() || null,
        customerId: $('#order-customer-id').val(),
        quotationId: $('#order-quote-id').val() || null,
        notes: $('#order-notes').val(),
        lines: $('.order-line-row').map(function () {
            const tr = $(this);
            return {
                itemId: tr.find('.order-line-item').val() || null,
                description: tr.find('.order-line-desc').val(),
                quantity: Number(tr.find('.order-line-qty').val() || 0),
                unitPrice: Number(tr.find('.order-line-price').val() || 0),
                discountAmount: Number(tr.find('.order-line-disc').val() || 0),
                taxAmount: Number(tr.find('.order-line-tax').val() || 0)
            };
        }).get().filter(l => l.quantity > 0)
    };
    if (currentOrderId) request.salesOrderId = currentOrderId;

    $.ajax({
        url: currentOrderId ? '/SalesOrder/Edit' : '/SalesOrder/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#order_modal').modal('hide'); window.orderTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save sales order')
    });
}

function approveOrder(id) {
    if (!confirm('Approve this sales order?')) return;
    $.ajax({ url: '/SalesOrder/Approve', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.orderTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to approve sales order') });
}

function deleteOrder(id) {
    if (!confirm('Delete this draft sales order?')) return;
    $.ajax({ url: '/SalesOrder/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.orderTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete sales order') });
}

function showOrderDetail(id) {
    $.get(`/SalesOrder/GetById?id=${id}`, data => {
        const rows = (data.lines || []).map(line => `
            <tr>
                <td>${line.itemCode || line.description}</td>
                <td>${line.description}</td>
                <td class="text-end">${formatNumber(line.quantity)}</td>
                <td class="text-end">${formatCurrency(line.unitPrice)}</td>
                <td class="text-end">${formatCurrency(line.discountAmount)}</td>
                <td class="text-end">${formatCurrency(line.taxAmount)}</td>
                <td class="text-end">${formatCurrency(line.lineTotal)}</td>
            </tr>`).join('');
        $('#order-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.orderNumber}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.orderDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Expected:</strong> ${data.expectedDate ? new Date(data.expectedDate).toLocaleDateString('en-GB') : '-'}</div>
                <div class="col-md-4"><strong>Customer:</strong> ${data.customerName}</div>
                <div class="col-md-4"><strong>Quotation:</strong> ${data.quotationNumber || '-'}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item</th><th>Description</th><th class="text-end">Qty</th><th class="text-end">Price</th><th class="text-end">Disc</th><th class="text-end">Tax</th><th class="text-end">Total</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="7" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold">Total: ${formatCurrency(data.totalAmount)}</div>`);
        $('#order_detail_modal').modal('show');
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