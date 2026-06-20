let paymentTable;
let currentPaymentId = null;

$(document).ready(function () {
    loadLookups();

    paymentTable = $('#payment_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Payment/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify({
                    draw: d.draw,
                    search: d.search.value || '',
                    page: (d.start / d.length) + 1,
                    size: d.length,
                    paymentType: $('#filter-payment-type').val() || null,
                    status: $('#filter-status').val() || null
                });
            }
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'paymentNumber' },
            { data: 'paymentType' },
            { data: 'paymentDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'partnerName' },
            { data: 'paymentMethod' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'totalAmount', render: formatCurrency },
            { data: null, orderable: false, render: row => actionButtons(row) }
        ]
    });

    $('#btn-apply-filter').on('click', () => paymentTable.ajax.reload());
    $('#btn-add-payment').on('click', openCreateModal);
    $('#btn-save-payment').on('click', savePayment);
    $('#payment-type').on('change', () => { togglePartnerType(); loadOpenInvoices(); });
    $('#payment_datatable').on('click', '.btn-post', function () {
        $.ajax({ url: '/Payment/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify($(this).data('id')), success: () => paymentTable.ajax.reload() });
    });
    $('#payment_datatable').on('click', '.btn-detail', function () { showDetail($(this).data('id')); });
    $('#payment_datatable').on('click', '.btn-edit', function () { openEditModal($(this).data('id')); });
    $('#payment_datatable').on('click', '.btn-delete', function () { deletePayment($(this).data('id')); });
});

function actionButtons(row) {
    const id = row.paymentId;
    const draftButtons = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-edit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-delete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-post" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-detail" data-id="${id}">Detail</button>${draftButtons}</div>`;
}

function openCreateModal() {
    currentPaymentId = null;
    $('#allocated-amount').val(0);
    $('#reference-number').val('');
    $('#payment-notes').val('');
    togglePartnerType();
    loadOpenInvoices();
    $('#payment_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/Payment/GetById?id=${id}`, data => {
        currentPaymentId = data.paymentId;
        $('#payment-type').val(data.paymentType);
        $('#payment-date').val(data.paymentDate?.substring(0, 10));
        $('#payment-method').val(data.paymentMethod);
        $('#payment-customer-id').val(data.customerId || '');
        $('#payment-supplier-id').val(data.supplierId || '');
        $('#cash-bank-account-id').val(data.cashBankAccountId);
        const allocation = data.allocations?.[0] || {};
        $('#invoice-id').val(allocation.invoiceId || '');
        $('#allocated-amount').val(allocation.allocatedAmount || 0);
        $('#reference-number').val(data.referenceNumber || '');
        $('#payment-notes').val(data.notes || '');
        togglePartnerType();
        loadOpenInvoices();
        $('#payment_modal').modal('show');
    });
}

function showDetail(id) {
    $.get(`/Payment/GetById?id=${id}`, data => {
        const allocations = (data.allocations || []).map(a => `<tr><td>${a.invoiceNumber}</td><td>${formatCurrency(a.allocatedAmount)}</td></tr>`).join('');
        $('#payment-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.paymentNumber}</div>
                <div class="col-md-4"><strong>Type:</strong> ${data.paymentType}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-6"><strong>Partner:</strong> ${data.partnerName}</div>
                <div class="col-md-6"><strong>Journal:</strong> ${data.journalNumber || '-'}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <div class="mb-4"><a class="btn btn-sm btn-light-primary" target="_blank" href="/Payment/Print?id=${data.paymentId}">Print / PDF</a></div>
            <table class="table table-sm"><thead><tr><th>Invoice</th><th>Allocated</th></tr></thead><tbody>${allocations}</tbody></table>
            <div class="text-end fw-bold mb-5">Total: ${formatCurrency(data.totalAmount)}</div>
            ${attachmentSection('Payment', data.paymentId)}`);
        $('#payment_detail_modal').modal('show');
        loadAttachments('Payment', data.paymentId);
    });
}

function togglePartnerType() {
    const type = $('#payment-type').val();
    $('#payment-customer-wrapper').toggleClass('d-none', type !== 'Receipt');
    $('#payment-supplier-wrapper').toggleClass('d-none', type !== 'Payment');
}

function loadLookups() {
    $.get('/Customer/GetActiveCustomers', data => fillSelect('#payment-customer-id', data, 'customerId', x => `${x.customerCode} - ${x.customerName}`));
    $.get('/Supplier/GetActiveSuppliers', data => fillSelect('#payment-supplier-id', data, 'supplierId', x => `${x.supplierCode} - ${x.supplierName}`));
    $.get('/ChartOfAccount/GetActiveAccounts', data => fillSelect('#cash-bank-account-id', data, 'accountId', x => `${x.accountCode} - ${x.accountName}`));
}

function loadOpenInvoices() {
    const invoiceType = $('#payment-type').val() === 'Payment' ? 'Purchase' : 'Sales';
    $.get(`/Invoice/GetOpenInvoices?invoiceType=${invoiceType}`, data => fillSelect('#invoice-id', data || [], 'invoiceId', x => `${x.invoiceNumber} - ${formatCurrency(x.outstandingAmount)}`));
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function savePayment() {
    const type = $('#payment-type').val();
    const amount = Number($('#allocated-amount').val() || 0);
    const request = {
        paymentType: type,
        paymentDate: $('#payment-date').val(),
        customerId: type === 'Receipt' ? $('#payment-customer-id').val() || null : null,
        supplierId: type === 'Payment' ? $('#payment-supplier-id').val() || null : null,
        cashBankAccountId: $('#cash-bank-account-id').val(),
        paymentMethod: $('#payment-method').val(),
        referenceNumber: $('#reference-number').val(),
        notes: $('#payment-notes').val(),
        allocations: [{ invoiceId: $('#invoice-id').val(), allocatedAmount: amount }]
    };
    if (currentPaymentId) request.paymentId = currentPaymentId;

    $.ajax({
        url: currentPaymentId ? '/Payment/Edit' : '/Payment/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#payment_modal').modal('hide'); paymentTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save payment')
    });
}

function deletePayment(id) {
    if (!confirm('Delete this draft payment?')) return;
    $.ajax({ url: '/Payment/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => paymentTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete payment') });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
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
