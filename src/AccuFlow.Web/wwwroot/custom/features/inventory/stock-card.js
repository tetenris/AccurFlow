$(document).ready(function () {
    $('#stock_card_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Inventory/StockCardDatatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'movementDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'itemName' },
            { data: 'warehouseName' },
            { data: 'movementType' },
            { data: 'quantityIn' },
            { data: 'quantityOut' },
            { data: 'unitCost', render: amount => new Intl.NumberFormat('id-ID').format(amount || 0) }
        ]
    });
});
