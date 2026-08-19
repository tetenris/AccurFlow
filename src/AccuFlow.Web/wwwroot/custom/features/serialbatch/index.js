let dataTable;

$(document).ready(function () {
    initializeDataTable();
    loadItemOptions();
});

function initializeDataTable() {
    dataTable = $('#serialbatch_datatable').DataTable({
        processing: true,
        serverSide: true,
        searching: true,
        scrollX: false,
        pageLength: 10,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: {
            search: '',
            searchPlaceholder: 'Search'
        },
        ajax: {
            url: '/SerialBatch/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                const itemId = $('#filter-item').val();
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    ItemId: itemId || null
                });
            }
        },
        order: [[0, 'desc']],
        columns: [
            {
                data: null,
                orderable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { data: 'itemCode' },
            { data: 'itemName' },
            { data: 'batchNumber' },
            {
                data: 'expiryDate',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString('en-GB') : '-';
                }
            },
            { data: 'quantity' },
            { data: 'remainingQuantity' },
            {
                data: 'status',
                render: function (data) {
                    const badges = { 'Available': 'badge-light-success', 'Partial': 'badge-light-warning', 'Out': 'badge-light-danger' };
                    return `<span class="badge ${badges[data] || 'badge-light-secondary'}">${data}</span>`;
                }
            },
            {
                data: "stockBatchId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    let buttons = `<div class="d-flex gap-2 justify-content-center">`;
                    if (row.remainingQuantity > 0) {
                        buttons += `<button class="btn btn-sm btn-icon btn-light-primary btn-consume" data-id="${data}" data-remaining="${row.remainingQuantity}" title="Consume">
                            <i class="fa fa-arrow-down"></i>
                        </button>`;
                    }
                    if (row.status === 'Available') {
                        buttons += `<button class="btn btn-sm btn-icon btn-light-danger btn-delete" data-id="${data}" title="Delete">
                            <i class="fa fa-trash"></i>
                        </button>`;
                    }
                    buttons += `</div>`;
                    return buttons;
                }
            }
        ]
    });
}

function loadItemOptions() {
    $.get('/SerialBatch/Items', function (data) {
        let options = '<option value="">All Items</option>';
        data.forEach(function (item) { options += `<option value="${item.value}">${item.text}</option>`; });
        $('#filter-item').html(options);

        let createOptions = '<option value="">Select Item</option>';
        data.forEach(function (item) { createOptions += `<option value="${item.value}">${item.text}</option>`; });
        $('#batch-item').html(createOptions);
    });
}

$('#btn-apply-filter').on('click', function () { dataTable.ajax.reload(); });

var btnAdd = $('<a>', {
    href: 'javascript:void(0)',
    class: 'btn btn-outline btn-outline-primary',
    type: 'button',
    id: 'btn-add-batch',
    html: '<i class="fa fa-plus"></i> Register Batch / Serial'
});
$('#toolbar-section-button').append(btnAdd);

$(document).on('click', '#btn-add-batch', function () {
    $('#form-batch')[0].reset();
    $('#batch-quantity').val(1);
    $('#modal-batch').modal('show');
});

$('#btn-save-batch').on('click', function () {
    if (!$('#form-batch')[0].checkValidity()) {
        $('#form-batch')[0].reportValidity();
        return;
    }
    $.ajax({
        url: '/SerialBatch/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            itemId: $('#batch-item').val(),
            batchNumber: $('#batch-number').val(),
            quantity: parseFloat($('#batch-quantity').val()) || 0,
            expiryDate: $('#batch-expiry').val() || null,
            notes: $('#batch-notes').val() || null
        }),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-batch').modal('hide');
                dataTable.ajax.reload();
            } else {
                Swal.fire('Error', response.message, 'error');
            }
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
        }
    });
});

$('#serialbatch_datatable').on('click', '.btn-consume', function () {
    const id = $(this).data('id');
    const remaining = $(this).data('remaining');
    Swal.fire({
        title: 'Consume Batch',
        html: `
            <p>Remaining quantity: <b>${remaining}</b></p>
            <label>Quantity:</label>
            <input type="number" id="consume-qty" class="form-control" step="0.01" min="0.01" max="${remaining}" value="${remaining}" />
            <label class="mt-2">Note:</label>
            <input type="text" id="consume-note" class="form-control" placeholder="e.g. Sales / Delivery" />
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Consume',
        cancelButtonText: 'Cancel',
        customClass: { confirmButton: 'btn btn-primary', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false,
        preConfirm: () => {
            const qty = parseFloat(document.getElementById('consume-qty').value) || 0;
            if (qty <= 0 || qty > remaining) Swal.showValidationMessage(`Quantity must be between 0.01 and ${remaining}`);
            return qty;
        }
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/SerialBatch/Consume',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ stockBatchId: id, quantity: result.value, note: $('#consume-note').val() || null }),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        dataTable.ajax.reload();
                    }
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
                }
            });
        }
    });
});

$('#serialbatch_datatable').on('click', '.btn-delete', function () {
    const id = $(this).data('id');
    Swal.fire({
        title: 'Delete batch?',
        text: 'Are you sure you want to delete this batch?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Delete',
        cancelButtonText: 'Cancel',
        customClass: { confirmButton: 'btn btn-danger', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/SerialBatch/Delete',
                type: 'DELETE',
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        dataTable.ajax.reload();
                    }
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
                }
            });
        }
    });
});