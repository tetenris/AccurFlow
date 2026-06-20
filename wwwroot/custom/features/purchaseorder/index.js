$(document).ready(function () {
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
});

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}
