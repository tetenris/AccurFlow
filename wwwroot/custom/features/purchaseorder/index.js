let currentPurchaseOrderId = null;

$(document).ready(function () {
    loadLookups();

    const table = $('#purchase_order_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/PurchaseOrder/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'purchaseOrderNumber' },
            { data: 'orderDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'expectedDate', render: data => data ? new Date(data).toLocaleDateString('en-GB') : '-' },
            { data: 'supplierName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'totalAmount', render: formatCurrency },
            { data: null, orderable: false, render: row => actionButtons(row) }
        ]
    });

    $('#purchase_order_datatable').on('click', '.btn-approve', function () {
        $.ajax({ url: '/PurchaseOrder/Approve', type: 'POST', contentType: 'application/json', data: JSON.stringify($(this).data('id')), success: () => table.ajax.reload() });
    });
    $('#purchase_order_datatable').on('click', '.btn-detail', function () { showDetail($(this).data('id')); });
    $('#purchase_order_datatable').on('click', '.btn-edit', function () { openEditModal($(this).data('id')); });
    $('#purchase_order_datatable').on('click', '.btn-delete', function () { deletePo($(this).data('id'), table); });
    $('#purchase_order_datatable').on('click', '.btn-convert', function () { $.ajax({ url: '/PurchaseOrder/ConvertToInvoice', type: 'POST', contentType: 'application/json', data: JSON.stringify($(this).data('id')), success: () => table.ajax.reload() }); });
    $('#btn-add-po').on('click', openCreateModal);
    $('#btn-save-po').on('click', () => savePo(table));
});

function actionButtons(row) {
    const id = row.purchaseOrderId;
    const draftButtons = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-edit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-delete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-approve" data-id="${id}">Approve</button>`
        : '';
    const convertButton = row.status === 'Approved' ? `<button class="btn btn-sm btn-light-warning btn-convert" data-id="${id}">Convert</button>` : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-detail" data-id="${id}">Detail</button>${draftButtons}${convertButton}</div>`;
}

function openCreateModal() {
    currentPurchaseOrderId = null;
    $('#po-description').val('');
    $('#po-quantity').val(1);
    $('#po-unit-price').val(0);
    $('#po-tax-amount').val(0);
    $('#po-notes').val('');
    $('#po_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/PurchaseOrder/GetById?id=${id}`, data => {
        currentPurchaseOrderId = data.purchaseOrderId;
        $('#order-date').val(data.orderDate?.substring(0, 10));
        $('#expected-date').val(data.expectedDate?.substring(0, 10) || '');
        $('#po-supplier-id').val(data.supplierId);
        const line = data.lines?.[0] || {};
        $('#po-item-id').val(line.itemId || '');
        $('#po-description').val(line.description || '');
        $('#po-quantity').val(line.quantity || 1);
        $('#po-unit-price').val(line.unitPrice || 0);
        $('#po-tax-amount').val(line.taxAmount || 0);
        $('#po-notes').val(data.notes || '');
        $('#po_modal').modal('show');
    });
}

function showDetail(id) {
    $.get(`/PurchaseOrder/GetById?id=${id}`, data => {
        const lines = (data.lines || []).map(line => `<tr><td>${line.description}</td><td>${line.quantity}</td><td>${formatCurrency(line.unitPrice)}</td><td>${formatCurrency(line.taxAmount)}</td><td>${formatCurrency(line.lineTotal)}</td></tr>`).join('');
        $('#po-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.purchaseOrderNumber}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>Supplier:</strong> ${data.supplierName}</div>
                <div class="col-md-6"><strong>Invoice:</strong> ${data.purchaseInvoiceNumber || '-'}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm"><thead><tr><th>Description</th><th>Qty</th><th>Price</th><th>Tax</th><th>Total</th></tr></thead><tbody>${lines}</tbody></table>
            <div class="text-end fw-bold mb-5">Total: ${formatCurrency(data.totalAmount)}</div>
            ${attachmentSection('PurchaseOrder', data.purchaseOrderId)}`);
        $('#po_detail_modal').modal('show');
        loadAttachments('PurchaseOrder', data.purchaseOrderId);
    });
}

function loadLookups() {
    $.get('/Supplier/GetActiveSuppliers', data => fillSelect('#po-supplier-id', data, 'supplierId', x => `${x.supplierCode} - ${x.supplierName}`));
    $.get('/Inventory/GetActiveItems', data => fillSelect('#po-item-id', data, 'itemId', x => `${x.itemCode} - ${x.itemName}`));
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function savePo(table) {
    const request = {
        orderDate: $('#order-date').val(),
        expectedDate: $('#expected-date').val() || null,
        supplierId: $('#po-supplier-id').val(),
        notes: $('#po-notes').val(),
        lines: [{
            itemId: $('#po-item-id').val() || null,
            description: $('#po-description').val(),
            quantity: Number($('#po-quantity').val() || 0),
            unitPrice: Number($('#po-unit-price').val() || 0),
            taxAmount: Number($('#po-tax-amount').val() || 0)
        }]
    };
    if (currentPurchaseOrderId) request.purchaseOrderId = currentPurchaseOrderId;

    $.ajax({
        url: currentPurchaseOrderId ? '/PurchaseOrder/Edit' : '/PurchaseOrder/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#po_modal').modal('hide'); table.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save purchase order')
    });
}

function deletePo(id, table) {
    if (!confirm('Delete this draft purchase order?')) return;
    $.ajax({ url: '/PurchaseOrder/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => table.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete purchase order') });
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
