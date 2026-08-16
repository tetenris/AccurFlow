let currentQuoteId = null;
let quoteItems = [];

$(document).ready(function () {
    initSalesQuotation();
});

function initSalesQuotation() {
    window.quoteTable = $('#quote_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/SalesQuotation/Datatable',
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
            { data: 'quotationNumber' },
            { data: 'quotationDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'validUntil', render: data => data ? new Date(data).toLocaleDateString('en-GB') : '-' },
            { data: 'customerName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalAmount', render: formatCurrency },
            { data: null, orderable: false, render: quoteActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.quoteTable.ajax.reload());
    $('#btn-add-quote').on('click', openQuoteCreateModal);
    $('#btn-save-quote').on('click', saveQuote);
    $('#btn-add-quote-line').on('click', () => appendQuoteLineRow({}));
    $('#quote-items-body').on('click', '.quote-line-remove', removeQuoteLineRow);
    $('#quote-items-body').on('change', '.quote-line-item', onQuoteLineItemChange);
    $('#quote-items-body').on('input', '.quote-line-input', updateQuoteLineTotal);
    $('#quote_datatable').on('click', '.btn-qapprove', function () { approveQuote($(this).data('id')); });
    $('#quote_datatable').on('click', '.btn-qdetail', function () { showQuoteDetail($(this).data('id')); });
    $('#quote_datatable').on('click', '.btn-qedit', function () { openQuoteEditModal($(this).data('id')); });
    $('#quote_datatable').on('click', '.btn-qdelete', function () { deleteQuote($(this).data('id')); });

    loadQuoteLookups();
}

function quoteActionButtons(row) {
    const id = row.salesQuotationId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-danger btn-qdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-qapprove" data-id="${id}">Approve</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-qdetail" data-id="${id}">Detail</button><button class="btn btn-sm btn-light-primary btn-qedit" data-id="${id}">Edit</button>${draft}</div>`;
}

function loadQuoteLookups() {
    $.get('/Inventory/GetActiveItems', data => { quoteItems = data || []; fillSelect('#quote-customer-id', ['Loading...']); });
    $.get('/Customer/GetActiveCustomers', data => fillSelect('#quote-customer-id', data, 'customerId', x => `${x.customerCode} - ${x.customerName}`));
}

function openQuoteCreateModal() {
    currentQuoteId = null;
    $('#quote-date').val(new Date().toISOString().substring(0, 10));
    $('#quote-valid-until').val('');
    $('#quote-customer-id').val('');
    $('#quote-notes').val('');
    $('#quote-items-body').empty();
    appendQuoteLineRow({});
    $('#quote_modal').modal('show');
}

function openQuoteEditModal(id) {
    $.get(`/SalesQuotation/GetById?id=${id}`, data => {
        currentQuoteId = data.salesQuotationId;
        $('#quote-date').val(data.quotationDate?.substring(0, 10));
        $('#quote-valid-until').val(data.validUntil?.substring(0, 10) || '');
        $('#quote-notes').val(data.notes || '');
        if ($('#quote-customer-id option[value]').length <= 1) {
            $.get('/Customer/GetActiveCustomers', customers => {
                fillSelect('#quote-customer-id', customers, 'customerId', x => `${x.customerCode} - ${x.customerName}`);
                $('#quote-customer-id').val(data.customerId);
                $('#quote_modal').modal('show');
            });
        } else {
            $('#quote-customer-id').val(data.customerId);
            $('#quote_modal').modal('show');
        }
        $('#quote-items-body').empty();
        (data.lines || []).forEach(line => appendQuoteLineRow(line));
    });
}

function appendQuoteLineRow(line) {
    const tr = $(`<tr class="quote-line-row"></tr>`);
    tr.html(`
        <td><select class="form-select form-select-sm quote-line-item"></select></td>
        <td><input type="text" class="form-control form-control-sm quote-line-desc" value="${escapeHtml(line.description || '')}" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm quote-line-qty quote-line-input text-end" value="${line.quantity || 1}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm quote-line-price quote-line-input text-end" value="${line.unitPrice || 0}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm quote-line-disc quote-line-input text-end" value="${line.discountAmount || 0}" min="0" step="any" /></td>
        <td class="text-end"><input type="number" class="form-control form-control-sm quote-line-tax quote-line-input text-end" value="${line.taxAmount || 0}" min="0" step="any" /></td>
        <td class="text-end fw-bold quote-line-total">0</td>
        <td class="text-end"><button class="btn btn-sm btn-icon btn-light-danger quote-line-remove" title="Remove"><i class="ki-duotone ki-trash fs-3"></i></button></td>`);
    fillItemSelect(tr.find('.quote-line-item'), line.itemId);
    $('#quote-items-body').append(tr);
    updateQuoteRowTotal(tr);
}

function fillItemSelect(select, selected) {
    select.empty();
    select.append('<option value="">Select Item</option>');
    (quoteItems || []).forEach(item => {
        select.append(`<option value="${item.itemId}" ${item.itemId === selected ? 'selected' : ''}>${item.itemCode} - ${item.itemName}</option>`);
    });
}

function onQuoteLineItemChange() {
    const tr = $(this).closest('tr');
    const item = (quoteItems || []).find(x => x.itemId === $(this).val());
    if (item) {
        tr.find('.quote-line-desc').val(`${item.itemCode} - ${item.itemName}`);
        tr.find('.quote-line-price').val(item.salesPrice || 0);
        updateQuoteRowTotal(tr);
    }
}

function removeQuoteLineRow() {
    $(this).closest('tr').remove();
}

function updateQuoteLineTotal() {
    updateQuoteRowTotal($(this).closest('tr'));
}

function updateQuoteRowTotal(tr) {
    const qty = Number(tr.find('.quote-line-qty').val() || 0);
    const price = Number(tr.find('.quote-line-price').val() || 0);
    const disc = Number(tr.find('.quote-line-disc').val() || 0);
    const tax = Number(tr.find('.quote-line-tax').val() || 0);
    tr.find('.quote-line-total').text(formatNumber((qty * price) - disc + tax));
}

function saveQuote() {
    const request = {
        quotationDate: $('#quote-date').val(),
        validUntil: $('#quote-valid-until').val() || null,
        customerId: $('#quote-customer-id').val(),
        notes: $('#quote-notes').val(),
        lines: $('.quote-line-row').map(function () {
            const tr = $(this);
            return {
                itemId: tr.find('.quote-line-item').val() || null,
                description: tr.find('.quote-line-desc').val(),
                quantity: Number(tr.find('.quote-line-qty').val() || 0),
                unitPrice: Number(tr.find('.quote-line-price').val() || 0),
                discountAmount: Number(tr.find('.quote-line-disc').val() || 0),
                taxAmount: Number(tr.find('.quote-line-tax').val() || 0)
            };
        }).get().filter(l => l.quantity > 0)
    };
    if (currentQuoteId) request.salesQuotationId = currentQuoteId;

    $.ajax({
        url: currentQuoteId ? '/SalesQuotation/Edit' : '/SalesQuotation/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#quote_modal').modal('hide'); window.quoteTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save quotation')
    });
}

function approveQuote(id) {
    if (!confirm('Approve this quotation?')) return;
    $.ajax({ url: '/SalesQuotation/Approve', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.quoteTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to approve quotation') });
}

function deleteQuote(id) {
    if (!confirm('Delete this draft quotation?')) return;
    $.ajax({ url: '/SalesQuotation/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.quoteTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete quotation') });
}

function showQuoteDetail(id) {
    $.get(`/SalesQuotation/GetById?id=${id}`, data => {
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
        $('#quote-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.quotationNumber}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.quotationDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Valid Until:</strong> ${data.validUntil ? new Date(data.validUntil).toLocaleDateString('en-GB') : '-'}</div>
                <div class="col-md-4"><strong>Customer:</strong> ${data.customerName}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item</th><th>Description</th><th class="text-end">Qty</th><th class="text-end">Price</th><th class="text-end">Disc</th><th class="text-end">Tax</th><th class="text-end">Total</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="7" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold">Total: ${formatCurrency(data.totalAmount)}</div>`);
        $('#quote_detail_modal').modal('show');
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