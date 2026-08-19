let currentUnitId = null;

$(document).ready(function () {
    initItemUnit();
});

function initItemUnit() {
    window.unitTable = $('#unit_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/ItemUnit/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'unitCode' },
            { data: 'unitName' },
            { data: 'description', render: data => data || '-' },
            { data: 'isActive', render: data => data ? '<span class="badge badge-light-success">Active</span>' : '<span class="badge badge-light-warning">Inactive</span>' },
            { data: null, orderable: false, render: row => unitActionButtons(row) }
        ]
    });

    $('#btn-add-unit').on('click', openCreateModal);
    $('#btn-save-unit').on('click', saveUnit);
    $('#unit_datatable').on('click', '.btn-edit', function () { openEditModal($(this).data('id')); });
    $('#unit_datatable').on('click', '.btn-delete', function () { deleteUnit($(this).data('id')); });
}

function unitActionButtons(row) {
    return `<div class="d-flex gap-1">
        <button class="btn btn-sm btn-light-primary btn-edit" data-id="${row.unitId}">Edit</button>
        <button class="btn btn-sm btn-light-danger btn-delete" data-id="${row.unitId}">Delete</button>
    </div>`;
}

function openCreateModal() {
    currentUnitId = null;
    $('#unit-code').val('');
    $('#unit-name').val('');
    $('#unit-desc').val('');
    $('#unit-active').prop('checked', true);
    $('#unit_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/ItemUnit/GetById?id=${id}`, data => {
        currentUnitId = data.unitId;
        $('#unit-code').val(data.unitCode);
        $('#unit-name').val(data.unitName);
        $('#unit-desc').val(data.description || '');
        $('#unit-active').prop('checked', data.isActive);
        $('#unit_modal').modal('show');
    });
}

function saveUnit() {
    const request = {
        unitCode: $('#unit-code').val(),
        unitName: $('#unit-name').val(),
        description: $('#unit-desc').val(),
        isActive: $('#unit-active').is(':checked')
    };
    if (currentUnitId) request.unitId = currentUnitId;

    $.ajax({
        url: currentUnitId ? '/ItemUnit/Edit' : '/ItemUnit/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#unit_modal').modal('hide'); window.unitTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save unit')
    });
}

function deleteUnit(id) {
    if (!confirm('Delete this unit?')) return;
    $.ajax({ url: '/ItemUnit/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.unitTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete unit') });
}