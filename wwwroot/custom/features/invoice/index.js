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
            <div class="mb-4"><a class="btn btn-sm btn-light-primary" target="_blank" href="/Invoice/Print?id=${data.invoiceId}">Print / PDF</a></div>
            <table class="table table-sm"><thead><tr><th>Description</th><th>Qty</th><th>Price</th><th>Tax</th><th>Total</th></tr></thead><tbody>${lines}</tbody></table>
            <div class="text-end fw-bold mb-5">Total: ${formatCurrency(data.totalAmount)}</div>
            ${attachmentSection('Invoice', data.invoiceId)}`);
        $('#invoice_detail_modal').modal('show');
        loadAttachments('Invoice', data.invoiceId);
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

function attachmentSection(documentType, documentId) {
    return `
        <div class="separator my-5"></div>
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h6 class="mb-0">Attachments</h6>
            <div class="d-flex gap-2">
                <input type="file" class="form-control form-control-sm" id="attachment-file" />
                <button class="btn btn-sm btn-primary" onclick="uploadAttachment('${documentType}', '${documentId}')">Upload</button>
            </div>
        </div>
        <div id="attachment-list"></div>`;
}

function loadAttachments(documentType, documentId) {
    $.get(`/DocumentAttachment/GetByDocument?documentType=${documentType}&documentId=${documentId}`, data => {
        const rows = (data || []).map(file => `
            <tr>
                <td>${file.fileName}</td>
                <td>${formatFileSize(file.fileSize)}</td>
                <td>${new Date(file.createdAt).toLocaleString('en-GB')}</td>
                <td class="text-end">
                    <a class="btn btn-sm btn-light-primary" href="/DocumentAttachment/Download?id=${file.documentAttachmentId}">Download</a>
                    <button class="btn btn-sm btn-light-danger" onclick="deleteAttachment('${file.documentAttachmentId}', '${documentType}', '${documentId}')">Delete</button>
                </td>
            </tr>`).join('');
        $('#attachment-list').html(`<table class="table table-sm"><thead><tr><th>File</th><th>Size</th><th>Uploaded</th><th></th></tr></thead><tbody>${rows || '<tr><td colspan="4" class="text-center text-muted">No attachments</td></tr>'}</tbody></table>`);
    });
}

function uploadAttachment(documentType, documentId) {
    const fileInput = $('#attachment-file')[0];
    if (!fileInput.files.length) return alert('Choose a file first');
    const formData = new FormData();
    formData.append('documentType', documentType);
    formData.append('documentId', documentId);
    formData.append('file', fileInput.files[0]);
    $.ajax({ url: '/DocumentAttachment/Upload', type: 'POST', data: formData, processData: false, contentType: false, success: () => loadAttachments(documentType, documentId), error: xhr => alert(xhr.responseJSON?.message || 'Failed to upload attachment') });
}

function deleteAttachment(id, documentType, documentId) {
    if (!confirm('Delete this attachment?')) return;
    $.ajax({ url: '/DocumentAttachment/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => loadAttachments(documentType, documentId), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete attachment') });
}

function formatFileSize(size) {
    if (!size) return '0 B';
    if (size < 1024) return `${size} B`;
    if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
    return `${(size / 1024 / 1024).toFixed(1)} MB`;
}
