let currentGroupId = null;

$(document).ready(function () {
    initItemGroup();
});

function initItemGroup() {
    window.groupTable = $('#group_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/ItemGroup/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'groupCode' },
            { data: 'groupName' },
            { data: 'description', render: data => data || '-' },
            { data: 'itemCount' },
            { data: 'isActive', render: data => data ? '<span class="badge badge-light-success">Active</span>' : '<span class="badge badge-light-warning">Inactive</span>' },
            { data: null, orderable: false, render: row => groupActionButtons(row) }
        ]
    });

    $('#btn-add-group').on('click', openCreateModal);
    $('#btn-save-group').on('click', saveGroup);
    $('#group_datatable').on('click', '.btn-edit', function () { openEditModal($(this).data('id')); });
    $('#group_datatable').on('click', '.btn-delete', function () { deleteGroup($(this).data('id')); });
}

function groupActionButtons(row) {
    return `<div class="d-flex gap-1">
        <button class="btn btn-sm btn-light-primary btn-edit" data-id="${row.itemGroupId}">Edit</button>
        <button class="btn btn-sm btn-light-danger btn-delete" data-id="${row.itemGroupId}">Delete</button>
    </div>`;
}

function openCreateModal() {
    currentGroupId = null;
    $('#group-code').val('');
    $('#group-name').val('');
    $('#group-desc').val('');
    $('#group-active').prop('checked', true);
    $('#group_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/ItemGroup/GetById?id=${id}`, data => {
        currentGroupId = data.itemGroupId;
        $('#group-code').val(data.groupCode);
        $('#group-name').val(data.groupName);
        $('#group-desc').val(data.description || '');
        $('#group-active').prop('checked', data.isActive);
        $('#group_modal').modal('show');
    });
}

function saveGroup() {
    const request = {
        groupCode: $('#group-code').val(),
        groupName: $('#group-name').val(),
        description: $('#group-desc').val(),
        isActive: $('#group-active').is(':checked')
    };
    if (currentGroupId) request.itemGroupId = currentGroupId;

    $.ajax({
        url: currentGroupId ? '/ItemGroup/Edit' : '/ItemGroup/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#group_modal').modal('hide'); window.groupTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save item group')
    });
}

function deleteGroup(id) {
    if (!confirm('Delete this item group?')) return;
    $.ajax({ url: '/ItemGroup/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.groupTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete item group') });
}