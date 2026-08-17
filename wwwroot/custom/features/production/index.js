let bomTable;
let poTable;
let itemsList = [];
let bomsList = [];

$(document).ready(function () {
    initializeBomTable();
    initializePoTable();
    loadItems();
    loadWarehouses();
    loadBoms();
});

function serverData(d) {
    return JSON.stringify({
        Draw: d.draw,
        Search: d.search.value || "",
        OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
        OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
        Page: (d.start / d.length) + 1,
        Size: d.length
    });
}

function initializeBomTable() {
    bomTable = $('#bom_datatable').DataTable({
        processing: true,
        serverSide: true,
        searching: true,
        scrollX: true,
        pageLength: 10,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: { search: '', searchPlaceholder: 'Search' },
        ajax: { url: '/Production/BomDatatable', type: 'POST', dataType: "json", contentType: 'application/json; charset=utf-8', data: serverData },
        order: [[1, 'desc']],
        columns: [
            { data: null, orderable: false, render: function (data, type, row, meta) { return meta.row + meta.settings._iDisplayStart + 1; } },
            { data: 'bomNumber' },
            { data: null, render: (d, t, row) => `${row.finishedItemCode} - ${row.finishedItemName}` },
            { data: 'lineCount' },
            { data: 'isActive', render: d => d ? '<span class="badge badge-light-success">Active</span>' : '<span class="badge badge-light-secondary">Inactive</span>' },
            {
                data: "bomId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data) {
                    return `<div class="d-flex gap-2 justify-content-center">
                        <button class="btn btn-sm btn-icon btn-light-primary btn-view-bom" data-id="${data}" title="Detail"><i class="fa fa-eye"></i></button>
                        <button class="btn btn-sm btn-icon btn-light-danger btn-delete-bom" data-id="${data}" title="Delete"><i class="fa fa-trash"></i></button>
                    </div>`;
                }
            }
        ]
    });
}

function initializePoTable() {
    poTable = $('#po_datatable').DataTable({
        processing: true,
        serverSide: true,
        searching: true,
        scrollX: true,
        pageLength: 10,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: { search: '', searchPlaceholder: 'Search' },
        ajax: { url: '/Production/ProductionOrderDatatable', type: 'POST', dataType: "json", contentType: 'application/json; charset=utf-8', data: serverData },
        order: [[5, 'desc']],
        columns: [
            { data: null, orderable: false, render: function (data, type, row, meta) { return meta.row + meta.settings._iDisplayStart + 1; } },
            { data: 'productionOrderNumber' },
            { data: 'bomNumber', render: d => d || '-' },
            { data: null, render: (d, t, row) => `${row.finishedItemCode} - ${row.finishedItemName}` },
            { data: 'quantity' },
            { data: 'productionDate', render: d => new Date(d).toLocaleDateString('en-GB') },
            { data: 'warehouseName' },
            { data: 'journalNumber', render: d => d || '-' },
            { data: 'status', render: d => { const b = { 'Draft': 'badge-light-warning', 'Posted': 'badge-light-success' }; return `<span class="badge ${b[d] || 'badge-light-secondary'}">${d}</span>`; } },
            {
                data: "productionOrderId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    let buttons = `<div class="d-flex gap-2 justify-content-center">
                        <button class="btn btn-sm btn-icon btn-light-primary btn-view-po" data-id="${data}" title="Detail"><i class="fa fa-eye"></i></button>`;
                    if (row.status === 'Draft') {
                        buttons += `<button class="btn btn-sm btn-icon btn-light-danger btn-delete-po" data-id="${data}" title="Delete"><i class="fa fa-trash"></i></button>`;
                    }
                    buttons += `</div>`;
                    return buttons;
                }
            }
        ]
    });
}

function loadItems() {
    $.get('/Production/Items', function (data) {
        itemsList = data;
        let options = '<option value="">Select Item</option>';
        data.forEach(function (item) { options += `<option value="${item.value}">${item.text}</option>`; });
        $('#bom-finished-item').html(options);
    });
}

function loadWarehouses() {
    $.get('/Production/Warehouses', function (data) {
        let options = '<option value="">Select Warehouse</option>';
        data.forEach(function (wh) { options += `<option value="${wh.value}">${wh.text}</option>`; });
        $('#po-warehouse').html(options);
    });
}

function loadBoms() {
    $.get('/Production/Boms', function (data) {
        bomsList = data || [];
        let options = '<option value="">Select BOM</option>';
        bomsList.forEach(function (bom) { options += `<option value="${bom.bomId}">${bom.bomNumber} - ${bom.finishedItemCode} ${bom.finishedItemName}</option>`; });
        $('#po-bom').html(options);
    });
}

var btnAddBom = $('<a>', {
    href: 'javascript:void(0)', class: 'btn btn-outline btn-outline-primary', type: 'button', id: 'btn-add-bom',
    html: '<i class="fa fa-plus"></i> Buat BOM'
});
$('#toolbar-section-button').append(btnAddBom);

var btnAddPo = $('<a>', {
    href: 'javascript:void(0)', class: 'btn btn-outline btn-outline-primary', type: 'button', id: 'btn-create-po',
    html: '<i class="fa fa-industry"></i> Production Order'
});
$('#toolbar-section-button').append(btnAddPo);

$(document).on('click', '#btn-add-bom', function () {
    $('#bom-notes').val('');
    $('#bom-lines-body').empty();
    addBomLine();
    addBomLine();
    $('#modal-bom').modal('show');
});

$(document).on('click', '#btn-add-bom-line', function () { addBomLine(); });

$(document).on('click', '#btn-create-po', function () {
    $('#form-create-po')[0].reset();
    $('#po-date').val(new Date().toISOString().split('T')[0]);
    $('#po-qty').val(1);
    loadBoms();
    $('#modal-create-po').modal('show');
});

$(document).on('click', '.btn-remove-bom-line', function () { $(this).closest('tr').remove(); });

function addBomLine() {
    const finishedId = $('#bom-finished-item').val();
    let options = '<option value="">Select Component</option>';
    itemsList.forEach(function (item) {
        if (!finishedId || item.value !== finishedId) options += `<option value="${item.value}">${item.text}</option>`;
    });
    const row = `
        <tr>
            <td><select class="form-select bom-component" required>${options}</select></td>
            <td><input type="number" class="form-control bom-qty" step="0.0001" min="0.0001" value="1" required /></td>
            <td class="text-center"><button type="button" class="btn btn-sm btn-icon btn-light-danger btn-remove-bom-line"><i class="fa fa-trash"></i></button></td>
        </tr>`;
    $('#bom-lines-body').append(row);
}

$('#btn-save-bom').on('click', function () {
    const lines = [];
    let isValid = true;
    $('#bom-lines-body tr').each(function () {
        const componentId = $(this).find('.bom-component').val();
        const qty = parseFloat($(this).find('.bom-qty').val()) || 0;
        if (!componentId) { Swal.fire('Error', 'Please select component for all lines', 'error'); isValid = false; return false; }
        if (qty <= 0) { Swal.fire('Error', 'Component qty must be positive', 'error'); isValid = false; return false; }
        lines.push({ componentItemId: componentId, quantityPerUnit: qty });
    });
    if (!isValid) return;
    if (lines.length === 0) { Swal.fire('Error', 'Add at least one component', 'error'); return; }

    $.ajax({
        url: '/Production/CreateBom',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ finishedItemId: $('#bom-finished-item').val(), notes: $('#bom-notes').val() || null, lines: lines }),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-bom').modal('hide');
                bomTable.ajax.reload();
                loadBoms();
            } else { Swal.fire('Error', response.message, 'error'); }
        },
        error: function (xhr) { Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error'); }
    });
});

$('#bom_datatable').on('click', '.btn-view-bom', function () {
    const id = $(this).data('id');
    $.get('/Production/GetBomById', { id: id }, function (data) {
        $('#bom-view-number').text(data.bomNumber);
        $('#bom-view-item').text(`${data.finishedItemCode} - ${data.finishedItemName}`);
        $('#bom-view-body').empty();
        data.lines.forEach(function (line) {
            $('#bom-view-body').append(`<tr><td>${line.componentItemCode} - ${line.componentItemName}</td><td>${line.quantityPerUnit}</td></tr>`);
        });
        $('#modal-view-bom').modal('show');
    }).fail(function () { Swal.fire('Error', 'Failed to load BOM', 'error'); });
});

$('#bom_datatable').on('click', '.btn-delete-bom', function () {
    const id = $(this).data('id');
    confirmDelete('/Production/DeleteBom', id, 'Delete this BOM?', () => { bomTable.ajax.reload(); loadBoms(); });
});

$('#btn-save-po').on('click', function () {
    if (!$('#form-create-po')[0].checkValidity()) { $('#form-create-po')[0].reportValidity(); return; }
    const bomId = $('#po-bom').val();
    const bom = bomsList.find(b => b.bomId === bomId);
    if (!bom) { Swal.fire('Error', 'Select a valid BOM', 'error'); return; }
    $.ajax({
        url: '/Production/CreateProductionOrder',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            bomId: bomId,
            finishedItemId: bom.finishedItemId,
            quantity: parseFloat($('#po-qty').val()) || 0,
            productionDate: $('#po-date').val(),
            warehouseId: $('#po-warehouse').val(),
            notes: $('#po-notes').val() || null
        }),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-create-po').modal('hide');
                poTable.ajax.reload();
            } else { Swal.fire('Error', response.message, 'error'); }
        },
        error: function (xhr) { Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error'); }
    });
});

$('#po_datatable').on('click', '.btn-view-po', function () { viewPo($(this).data('id')); });
$('#po_datatable').on('click', '.btn-delete-po', function () {
    confirmDelete('/Production/DeleteProductionOrder', $(this).data('id'), 'Delete this production order draft?', () => poTable.ajax.reload());
});

function viewPo(id) {
    $.get('/Production/GetProductionOrderById', { id: id }, function (data) {
        $('#po-detail-number').text(data.productionOrderNumber);
        $('#po-detail-bom').text(data.bomNumber || '-');
        $('#po-detail-item').text(`${data.finishedItemCode} - ${data.finishedItemName}`);
        $('#po-detail-qty').text(data.quantity);
        $('#po-detail-date').text(new Date(data.productionDate).toLocaleDateString('en-GB'));
        $('#po-detail-wh').text(data.warehouseName);
        $('#po-detail-journal').text(data.journalNumber || '-');
        const b = { 'Draft': 'badge-light-warning', 'Posted': 'badge-light-success' };
        $('#po-detail-status').html(`<span class="badge ${b[data.status]}">${data.status}</span>`);

        $('#po-detail-body').empty();
        data.lines.forEach(function (line) {
            $('#po-detail-body').append(`
                <tr>
                    <td>${line.componentItemCode} - ${line.componentItemName}</td>
                    <td>${line.quantityRequired}</td>
                    <td>${formatCurrency(line.unitCost)}</td>
                    <td>${formatCurrency(line.lineCost)}</td>
                </tr>`);
        });

        $('#btn-post-po').hide().data('id', id);
        if (data.canPost) $('#btn-post-po').show();

        $('#modal-po-detail').modal('show');
    }).fail(function () { Swal.fire('Error', 'Failed to load production order', 'error'); });
}

$('#btn-post-po').on('click', function () {
    const id = $(this).data('id');
    Swal.fire({
        title: 'Post Production?',
        text: 'Stok komponen akan dikurangi, stok barang jadi ditambah, dan jurnal otomatis dibuat.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Post',
        cancelButtonText: 'Cancel',
        customClass: { confirmButton: 'btn btn-success', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Production/PostProductionOrder',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        $('#modal-po-detail').modal('hide');
                        poTable.ajax.reload();
                    }
                },
                error: function (xhr) { Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error'); }
            });
        }
    });
});

function confirmDelete(url, id, text, onSuccess) {
    Swal.fire({
        title: 'Delete?',
        text: text,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Delete',
        cancelButtonText: 'Cancel',
        customClass: { confirmButton: 'btn btn-danger', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        if (onSuccess) onSuccess();
                    }
                },
                error: function (xhr) { Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error'); }
            });
        }
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 2 }).format(amount || 0);
}