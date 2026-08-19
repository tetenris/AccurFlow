let currentReorderItemId = null;

$(document).ready(function () {
    window.stockMinimumTable = $('#stock_minimum_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Inventory/StockMinimumDatatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length, belowOnly: $('#filter-below-only').is(':checked') })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'itemCode' },
            { data: 'itemName' },
            { data: 'unit' },
            { data: 'currentStock', render: formatNumber },
            { data: 'reorderPoint', render: formatNumber },
            { data: 'isBelow', render: data => data ? '<span class="badge badge-light-danger">Below Minimum</span>' : '<span class="badge badge-light-success">OK</span>' },
            { data: null, orderable: false, render: row => `<button class="btn btn-sm btn-light-primary btn-set-reorder" data-item="${row.itemId}" data-stock="${row.currentStock}" data-reorder="${row.reorderPoint}">Set Reorder Point</button>` }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.stockMinimumTable.ajax.reload());
    $('#stock_minimum_datatable').on('click', '.btn-set-reorder', function () {
        currentReorderItemId = $(this).data('item');
        $('#reorder-current-stock').val(formatNumber($(this).data('stock')));
        $('#reorder-point').val($(this).data('reorder'));
        $('#reorder_modal').modal('show');
    });
    $('#btn-save-reorder').on('click', saveReorderPoint);
});

function saveReorderPoint() {
    const itemId = currentReorderItemId;
    const reorderPoint = Number($('#reorder-point').val() || 0);
    $.ajax({
        url: '/Inventory/UpdateReorderPoint',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ itemId: itemId, reorderPoint: reorderPoint }),
        success: () => { $('#reorder_modal').modal('hide'); window.stockMinimumTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to update reorder point')
    });
}

function formatNumber(value) {
    return new Intl.NumberFormat('id-ID', { maximumFractionDigits: 2 }).format(value || 0);
}