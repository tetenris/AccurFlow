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
            { data: 'purchaseOrderId', orderable: false, render: data => `<button class="btn btn-sm btn-light-success btn-approve" data-id="${data}">Approve</button>` }
        ]
    });

    $('#purchase_order_datatable').on('click', '.btn-approve', function () {
        $.ajax({ url: '/PurchaseOrder/Approve', type: 'POST', contentType: 'application/json', data: JSON.stringify($(this).data('id')), success: () => table.ajax.reload() });
    });
    $('#btn-add-po').on('click', () => $('#po_modal').modal('show'));
    $('#btn-save-po').on('click', () => savePo(table));
});

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

    $.ajax({
        url: '/PurchaseOrder/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#po_modal').modal('hide'); table.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save purchase order')
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}
