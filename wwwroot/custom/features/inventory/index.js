$(document).ready(function () {
    $('#item_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Inventory/DatatableItems',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'itemCode' },
            { data: 'itemName' },
            { data: 'itemType' },
            { data: 'itemGroupName', render: data => data || '-' },
            { data: 'unit' },
            { data: 'salesPrice', render: formatCurrency },
            { data: 'purchasePrice', render: formatCurrency },
            { data: 'isActive', render: data => data ? '<span class="badge badge-light-success">Active</span>' : '<span class="badge badge-light-warning">Inactive</span>' }
        ]
    });
});

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}
