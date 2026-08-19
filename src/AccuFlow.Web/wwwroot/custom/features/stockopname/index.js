$(document).ready(function () {
    initOpname();
});

let currentOpnameId = null;

function initOpname() {
    window.opnameTable = $('#opname_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/StockOpname/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({
                draw: d.draw,
                search: d.search.value || '',
                page: (d.start / d.length) + 1,
                size: d.length,
                status: $('#filter-status').val() || null
            })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'stockOpnameNumber' },
            { data: 'opnameDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'warehouseName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'lineCount' },
            { data: 'totalDifference', render: formatNumber },
            { data: null, orderable: false, render: opnameActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.opnameTable.ajax.reload());
    $('#btn-add-opname').on('click', openOpnameCreateModal);
    $('#btn-save-opname').on('click', saveOpname);
    $('#btn-load-items').on('click', loadOpnameItems);
    $('#opname-warehouse-id').on('change', loadOpnameItems);
    $('#opname-items-body').on('input', '.opname-actual', updateDifference);
    $('#opname_datatable').on('click', '.btn-opost', function () { postOpname($(this).data('id')); });
    $('#opname_datatable').on('click', '.btn-odetail', function () { showOpnameDetail($(this).data('id')); });
    $('#opname_datatable').on('click', '.btn-oedit', function () { openOpnameEditModal($(this).data('id')); });
    $('#opname_datatable').on('click', '.btn-odelete', function () { deleteOpname($(this).data('id')); });

    loadWarehouses();
}

function opnameActionButtons(row) {
    const id = row.stockOpnameId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-oedit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-odelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-opost" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-odetail" data-id="${id}">Detail</button>${draft}</div>`;
}

function loadWarehouses() {
    $.get('/StockOpname/GetWarehouses', data => fillSelect('#opname-warehouse-id', data, 'warehouseId', x => `${x.warehouseCode} - ${x.warehouseName}`));
}

function openOpnameCreateModal() {
    currentOpnameId = null;
    $('#opname-date').val(new Date().toISOString().substring(0, 10));
    $('#opname-notes').val('');
    $('#opname-warehouse-id').val('');
    renderOpnameItems([]);
    $('#opname_modal').modal('show');
}

function openOpnameEditModal(id) {
    $.get(`/StockOpname/GetById?id=${id}`, data => {
        currentOpnameId = data.stockOpnameId;
        $('#opname-date').val(data.opnameDate?.substring(0, 10));
        $('#opname-notes').val(data.notes || '');
        $('#opname-warehouse-id').val(data.warehouseId);
        $('#opname_modal').modal('show');
        loadOpnameItems(() => mergeOpnameLines(data.lines || []));
    });
}

function loadOpnameItems(afterLoad) {
    const warehouseId = $('#opname-warehouse-id').val();
    if (!warehouseId) {
        renderOpnameItems([]);
        return;
    }
    $.get(`/StockOpname/GetStockQuantities?warehouseId=${warehouseId}`, data => {
        renderOpnameItems(data || []);
        if (afterLoad) afterLoad();
    });
}

function renderOpnameItems(lines) {
    const body = $('#opname-items-body');
    if (!lines.length) {
        body.html('<tr id="opname-items-empty"><td colspan="7" class="text-center text-muted py-5">No stock items found in this warehouse</td></tr>');
        return;
    }
    body.empty();
    (lines || []).forEach(line => {
        body.append(`
            <tr class="opname-item-row" data-item-id="${line.itemId}">
                <td>${line.itemCode}</td>
                <td>${line.itemName}</td>
                <td>${line.unit || '-'}</td>
                <td class="text-end"><input type="number" class="form-control form-control-sm opname-system text-end" value="${line.systemQuantity}" readonly /></td>
                <td class="text-end"><input type="number" class="form-control form-control-sm opname-actual text-end" value="${line.actualQuantity}" min="0" /></td>
                <td class="text-end fw-bold opname-diff">${formatNumber(line.differenceQuantity)}</td>
                <td><input type="text" class="form-control form-control-sm opname-note" /></td>
            </tr>`);
    });
}

function mergeOpnameLines(lines) {
    lines.forEach(line => {
        const row = $(`#opname-items-body tr.opname-item-row[data-item-id="${line.itemId}"]`);
        if (!row.length) return;
        row.find('.opname-system').val(line.systemQuantity);
        row.find('.opname-actual').val(line.actualQuantity);
        row.find('.opname-note').val(line.notes || '');
        updateDifference.call(row.find('.opname-actual')[0]);
    });
}

function updateDifference() {
    const row = $(this).closest('tr');
    const system = Number(row.find('.opname-system').val() || 0);
    const actual = Number(row.find('.opname-actual').val() || 0);
    row.find('.opname-diff').text(formatNumber(actual - system));
}

function saveOpname() {
    const request = {
        opnameDate: $('#opname-date').val(),
        warehouseId: $('#opname-warehouse-id').val(),
        notes: $('#opname-notes').val(),
        lines: $('#opname-items-body tr.opname-item-row').map(function () {
            const row = $(this);
            const system = Number(row.find('.opname-system').val() || 0);
            const actual = Number(row.find('.opname-actual').val() || 0);
            return {
                itemId: row.data('item-id'),
                systemQuantity: system,
                actualQuantity: actual,
                differenceQuantity: actual - system,
                notes: row.find('.opname-note').val()
            };
        }).get()
    };
    if (currentOpnameId) request.stockOpnameId = currentOpnameId;

    $.ajax({
        url: currentOpnameId ? '/StockOpname/Edit' : '/StockOpname/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#opname_modal').modal('hide'); window.opnameTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save stock opname')
    });
}

function postOpname(id) {
    if (!confirm('Post this stock opname? Stock adjustments will be created.')) return;
    $.ajax({ url: '/StockOpname/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.opnameTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to post stock opname') });
}

function deleteOpname(id) {
    if (!confirm('Delete this draft stock opname?')) return;
    $.ajax({ url: '/StockOpname/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.opnameTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete stock opname') });
}

function showOpnameDetail(id) {
    $.get(`/StockOpname/GetById?id=${id}`, data => {
        const rows = (data.lines || []).map(line => `
            <tr>
                <td>${line.itemCode}</td>
                <td>${line.itemName}</td>
                <td>${line.unit || '-'}</td>
                <td class="text-end">${formatNumber(line.systemQuantity)}</td>
                <td class="text-end">${formatNumber(line.actualQuantity)}</td>
                <td class="text-end fw-bold">${formatNumber(line.differenceQuantity)}</td>
                <td>${line.notes || '-'}</td>
            </tr>`).join('');
        $('#opname-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.stockOpnameNumber}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.opnameDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>Warehouse:</strong> ${data.warehouseName}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm">
                <thead><tr><th>Item Code</th><th>Item Name</th><th>Unit</th><th class="text-end">System Qty</th><th class="text-end">Actual Qty</th><th class="text-end">Difference</th><th>Notes</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="7" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold">Total Difference: ${formatNumber(data.totalDifference)}</div>`);
        $('#opname_detail_modal').modal('show');
    });
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function formatNumber(value) {
    return new Intl.NumberFormat('id-ID', { maximumFractionDigits: 2 }).format(value || 0);
}