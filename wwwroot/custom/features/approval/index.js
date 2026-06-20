$(document).ready(function () {
    const table = $('#approval_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Approval/Datatable', type: 'POST', contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'documentType' }, { data: 'documentId' }, { data: 'status' }, { data: 'requestedByName' },
            { data: 'requestedAt', render: data => new Date(data).toLocaleString('en-GB') }, { data: 'notes', render: data => data || '-' },
            { data: 'approvalRequestId', orderable: false, render: data => `<button class="btn btn-sm btn-light-success btn-approve" data-id="${data}">Approve</button> <button class="btn btn-sm btn-light-danger btn-reject" data-id="${data}">Reject</button>` }
        ]
    });
    $('#approval_datatable').on('click', '.btn-approve', function () { action('/Approval/Approve', $(this).data('id'), table); });
    $('#approval_datatable').on('click', '.btn-reject', function () { action('/Approval/Reject', $(this).data('id'), table); });
});

function action(url, id, table) {
    $.ajax({ url, type: 'POST', contentType: 'application/json', data: JSON.stringify({ approvalRequestId: id, notes: '' }), success: () => table.ajax.reload() });
}
