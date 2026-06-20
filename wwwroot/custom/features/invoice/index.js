let invoiceTable;
let currentInvoiceId = null;

$(document).ready(function () {
    loadLookups();

    invoiceTable = $('#invoice_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Invoice/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify({
                    draw: d.draw,
                    search: d.search.value || '',
                    page: (d.start / d.length) + 1,
                    size: d.length,
                    invoiceType: $('#filter-invoice-type').val() || null,
                    status: $('#filter-status').val() || null
                });
            }
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'invoiceNumber' },
            { data: 'invoiceType' },
            { data: 'invoiceDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'dueDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'partnerName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'totalAmount', render: formatCurrency },
            { data: 'outstandingAmount', render: formatCurrency },
            { data: null, orderable: false, render: row => actionButtons(row) }
        ]
    });

    $('#btn-apply-filter').on('click', () => invoiceTable.ajax.reload());
    $('#btn-add-invoice').on('click', openCreateModal);
    $('#btn-save-invoice').on('click', saveInvoice);
    $('#invoice-type').on('change', togglePartnerType);
    $('#invoice_datatable').on('click', '.btn-post', function () {
        postDocument('/Invoice/Post', $(this).data('id'), invoiceTable);
    });
    $('#invoice_datatable').on('click', '.btn-detail', function () { showDetail($(this).data('id')); });
    $('#invoice_datatable').on('click', '.btn-edit', function () { openEditModal($(this).data('id')); });
    $('#invoice_datatable').on('click', '.btn-delete', function () { deleteInvoice($(this).data('id')); });
});

function actionButtons(row) {
    const id = row.invoiceId;
    const draftButtons = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-edit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-delete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-post" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-detail" data-id="${id}">Detail</button>${draftButtons}</div>`;
}

function openCreateModal() {
    currentInvoiceId = null;
    $('#line-description').val('');
    $('#quantity').val(1);
    $('#unit-price').val(0);
    $('#discount-amount').val(0);
    $('#tax-amount').val(0);
    $('#notes').val('');
    togglePartnerType();
    $('#invoice_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/Invoice/GetById?id=${id}`, data => {
        currentInvoiceId = data.invoiceId;
        $('#invoice-type').val(data.invoiceType);
        $('#invoice-date').val(data.invoiceDate?.substring(0, 10));
        $('#due-date').val(data.dueDate?.substring(0, 10));
        $('#customer-id').val(data.customerId || '');
        $('#supplier-id').val(data.supplierId || '');
        const line = data.lines?.[0] || {};
        $('#account-id').val(line.accountId || '');
        $('#line-description').val(line.description || '');
        $('#quantity').val(line.quantity || 1);
        $('#unit-price').val(line.unitPrice || 0);
        $('#discount-amount').val(line.discountAmount || 0);
        $('#tax-amount').val(line.taxAmount || 0);
        $('#notes').val(data.notes || '');
        togglePartnerType();
        $('#invoice_modal').modal('show');
    });
}

function showDetail(id) {
    $.get(`/Invoice/GetById?id=${id}`, data => {
        const lines = (data.lines || []).map(line => `<tr><td>${line.description}</td><td>${line.quantity}</td><td>${formatCurrency(line.unitPrice)}</td><td>${formatCurrency(line.taxAmount)}</td><td>${formatCurrency(line.lineTotal)}</td></tr>`).join('');
        $('#invoice-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.invoiceNumber}</div>
                <div class="col-md-4"><strong>Type:</strong> ${data.invoiceType}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-6"><strong>Partner:</strong> ${data.partnerName}</div>
                <div class="col-md-6"><strong>Journal:</strong> ${data.journalNumber || '-'}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm"><thead><tr><th>Description</th><th>Qty</th><th>Price</th><th>Tax</th><th>Total</th></tr></thead><tbody>${lines}</tbody></table>
            <div class="text-end fw-bold">Total: ${formatCurrency(data.totalAmount)}</div>`);
        $('#invoice_detail_modal').modal('show');
    });
}

function togglePartnerType() {
    const type = $('#invoice-type').val();
    $('#customer-wrapper').toggleClass('d-none', type !== 'Sales');
    $('#supplier-wrapper').toggleClass('d-none', type !== 'Purchase');
}

function loadLookups() {
    $.get('/Customer/GetActiveCustomers', data => fillSelect('#customer-id', data, 'customerId', x => `${x.customerCode} - ${x.customerName}`));
    $.get('/Supplier/GetActiveSuppliers', data => fillSelect('#supplier-id', data, 'supplierId', x => `${x.supplierCode} - ${x.supplierName}`));
    $.get('/ChartOfAccount/GetActiveAccounts', data => fillSelect('#account-id', data, 'accountId', x => `${x.accountCode} - ${x.accountName}`));
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function saveInvoice() {
    const type = $('#invoice-type').val();
    const request = {
        invoiceType: type,
        invoiceDate: $('#invoice-date').val(),
        dueDate: $('#due-date').val(),
        customerId: type === 'Sales' ? $('#customer-id').val() || null : null,
        supplierId: type === 'Purchase' ? $('#supplier-id').val() || null : null,
        notes: $('#notes').val(),
        lines: [{
            accountId: $('#account-id').val() || null,
            description: $('#line-description').val(),
            quantity: Number($('#quantity').val() || 0),
            unitPrice: Number($('#unit-price').val() || 0),
            discountAmount: Number($('#discount-amount').val() || 0),
            taxAmount: Number($('#tax-amount').val() || 0)
        }]
    };
    if (currentInvoiceId) request.invoiceId = currentInvoiceId;

    $.ajax({
        url: currentInvoiceId ? '/Invoice/Edit' : '/Invoice/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#invoice_modal').modal('hide'); invoiceTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save invoice')
    });
}

function deleteInvoice(id) {
    if (!confirm('Delete this draft invoice?')) return;
    $.ajax({ url: '/Invoice/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => invoiceTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete invoice') });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}

function postDocument(url, id, table) {
    $.ajax({ url, type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => table.ajax.reload() });
}
