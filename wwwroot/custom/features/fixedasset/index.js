let currentAssetId = null;
let currentDeprecAssetId = null;

$(document).ready(function () {
    initFixedAsset();
});

function initFixedAsset() {
    window.assetTable = $('#asset_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/FixedAsset/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'assetCode' },
            { data: 'assetName' },
            { data: 'category' },
            { data: 'purchaseDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'purchaseCost', render: formatCurrency },
            { data: 'accumulatedDepreciation', render: formatCurrency },
            { data: 'netBookValue', render: formatCurrency },
            { data: 'status', render: data => data === 'Active' ? '<span class="badge badge-light-success">Active</span>' : '<span class="badge badge-light-warning">Inactive</span>' },
            { data: null, orderable: false, render: assetActionButtons }
        ]
    });

    $('#btn-add-asset').on('click', openCreateModal);
    $('#btn-save-asset').on('click', saveAsset);
    $('#asset_datatable').on('click', '.btn-asset-edit', function () { openEditModal($(this).data('id')); });
    $('#asset_datatable').on('click', '.btn-asset-delete', function () { deleteAsset($(this).data('id')); });
    $('#asset_datatable').on('click', '.btn-asset-deprec', function () { openDeprecModal($(this).data('id')); });
    $('#asset_datatable').on('click', '.btn-asset-detail', function () { showDetail($(this).data('id')); });
    $('#btn-run-deprec').on('click', runDepreciation);
}

function assetActionButtons(row) {
    const deprecBtn = row.status === 'Active' ? `<button class="btn btn-sm btn-light-success btn-asset-deprec" data-id="${row.assetId}">Depreciate</button>` : '';
    return `<div class="d-flex gap-1">
        <button class="btn btn-sm btn-light-info btn-asset-detail" data-id="${row.assetId}">Detail</button>
        <button class="btn btn-sm btn-light-primary btn-asset-edit" data-id="${row.assetId}">Edit</button>
        ${deprecBtn}
        <button class="btn btn-sm btn-light-danger btn-asset-delete" data-id="${row.assetId}">Delete</button>
    </div>`;
}

function openCreateModal() {
    currentAssetId = null;
    $('#asset-name').val('');
    $('#asset-category').val('Equipment');
    $('#asset-purchase-date').val(new Date().toISOString().substring(0, 10));
    $('#asset-cost').val('');
    $('#asset-salvage').val('0');
    $('#asset-useful-life').val(60);
    $('#asset-notes').val('');
    $('#asset_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/FixedAsset/GetById?id=${id}`, data => {
        currentAssetId = data.assetId;
        $('#asset-name').val(data.assetName);
        $('#asset-category').val(data.category);
        $('#asset-purchase-date').val(data.purchaseDate?.substring(0, 10));
        $('#asset-cost').val(data.purchaseCost);
        $('#asset-salvage').val(data.salvageValue);
        $('#asset-useful-life').val(data.usefulLifeMonths);
        $('#asset-notes').val(data.notes || '');
        $('#asset_modal').modal('show');
    });
}

function saveAsset() {
    const request = {
        assetName: $('#asset-name').val(),
        category: $('#asset-category').val(),
        purchaseDate: $('#asset-purchase-date').val(),
        purchaseCost: Number($('#asset-cost').val() || 0),
        salvageValue: Number($('#asset-salvage').val() || 0),
        usefulLifeMonths: Number($('#asset-useful-life').val() || 60),
        notes: $('#asset-notes').val()
    };
    if (currentAssetId) request.assetId = currentAssetId;

    $.ajax({
        url: currentAssetId ? '/FixedAsset/Edit' : '/FixedAsset/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#asset_modal').modal('hide'); window.assetTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save fixed asset')
    });
}

function deleteAsset(id) {
    if (!confirm('Delete this fixed asset?')) return;
    $.ajax({ url: '/FixedAsset/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.assetTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete fixed asset') });
}

function openDeprecModal(id) {
    currentDeprecAssetId = id;
    $('#deprec-period').val(new Date().toISOString().substring(0, 10));
    $('#deprec_modal').modal('show');
}

function runDepreciation() {
    if (!confirm('Post depreciation journal for this asset period?')) return;
    $.ajax({
        url: '/FixedAsset/Depreciate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ assetId: currentDeprecAssetId, periodDate: $('#deprec-period').val() }),
        success: () => { $('#deprec_modal').modal('hide'); window.assetTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to run depreciation')
    });
}

function showDetail(id) {
    $.get(`/FixedAsset/GetById?id=${id}`, data => {
        const rows = (data.depreciations || []).map(d => `
            <tr>
                <td>${new Date(d.periodDate).toLocaleDateString('en-GB')}</td>
                <td class="text-end">${formatCurrency(d.amount)}</td>
                <td>${d.journalId ? 'Posted' : '-'}</td>
            </tr>`).join('') || '<tr><td colspan="3" class="text-center text-muted">No depreciation yet</td></tr>';
        $('#asset-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-6"><strong>Code:</strong> ${data.assetCode}</div>
                <div class="col-md-6"><strong>Name:</strong> ${data.assetName}</div>
                <div class="col-md-4"><strong>Category:</strong> ${data.category}</div>
                <div class="col-md-4"><strong>Purchase Date:</strong> ${new Date(data.purchaseDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>Cost:</strong> ${formatCurrency(data.purchaseCost)}</div>
                <div class="col-md-4"><strong>Salvage:</strong> ${formatCurrency(data.salvageValue)}</div>
                <div class="col-md-4"><strong>Useful Life:</strong> ${data.usefulLifeMonths} months</div>
                <div class="col-md-4"><strong>Accum. Depr.:</strong> ${formatCurrency(data.accumulatedDepreciation)}</div>
                <div class="col-md-4"><strong>Net Book Value:</strong> ${formatCurrency(data.netBookValue)}</div>
                <div class="col-md-4"><strong>Last Depr.:</strong> ${data.lastDepreciationDate ? new Date(data.lastDepreciationDate).toLocaleDateString('en-GB') : '-'}</div>
            </div>
            <h6 class="mb-3">Depreciation History</h6>
            <table class="table table-sm">
                <thead><tr><th>Period</th><th class="text-end">Amount</th><th>Journal</th></tr></thead>
                <tbody>${rows}</tbody>
            </table>`);
        $('#asset_detail_modal').modal('show');
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 2 }).format(amount || 0);
}