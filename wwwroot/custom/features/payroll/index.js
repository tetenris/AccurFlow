let employeeTable;
let payrollTable;

$(document).ready(function () {
    initializeEmployeeTable();
    initializePayrollTable();
});

function initializeEmployeeTable() {
    employeeTable = $('#employee_datatable').DataTable({
        processing: true,
        serverSide: true,
        searching: true,
        scrollX: true,
        pageLength: 10,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: { search: '', searchPlaceholder: 'Search' },
        ajax: {
            url: '/Payroll/EmployeeDatatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length
                });
            }
        },
        order: [[1, 'asc']],
        columns: [
            { data: null, orderable: false, render: function (data, type, row, meta) { return meta.row + meta.settings._iDisplayStart + 1; } },
            { data: 'employeeCode' },
            { data: 'fullName' },
            { data: 'position' },
            { data: 'department' },
            { data: 'hireDate', render: d => d ? new Date(d).toLocaleDateString('en-GB') : '-' },
            { data: 'basicSalary', render: d => formatCurrency(d) },
            {
                data: 'isActive',
                render: function (data) {
                    return data
                        ? '<span class="badge badge-light-success">Active</span>'
                        : '<span class="badge badge-light-secondary">Inactive</span>';
                }
            },
            {
                data: "employeeId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    return `
                        <div class="d-flex gap-2 justify-content-center">
                            <button class="btn btn-sm btn-icon btn-light-success btn-edit-employee" data-id="${data}" title="Edit">
                                <i class="fa fa-pencil"></i>
                            </button>
                            <button class="btn btn-sm btn-icon btn-light-danger btn-delete-employee" data-id="${data}" title="Delete">
                                <i class="fa fa-trash"></i>
                            </button>
                        </div>`;
                }
            }
        ]
    });
}

function initializePayrollTable() {
    payrollTable = $('#payroll_datatable').DataTable({
        processing: true,
        serverSide: true,
        searching: true,
        scrollX: true,
        pageLength: 10,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: { search: '', searchPlaceholder: 'Search' },
        ajax: {
            url: '/Payroll/PayrollDatatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length
                });
            }
        },
        order: [[1, 'desc']],
        columns: [
            { data: null, orderable: false, render: function (data, type, row, meta) { return meta.row + meta.settings._iDisplayStart + 1; } },
            { data: 'payrollNumber' },
            { data: null, render: (d, t, row) => `${row.periodMonth}/${row.periodYear}` },
            { data: 'payrollDate', render: d => new Date(d).toLocaleDateString('en-GB') },
            { data: 'lineCount' },
            { data: 'totalNetSalary', render: d => formatCurrency(d) },
            { data: 'journalNumber', render: d => d || '-' },
            {
                data: 'status',
                render: function (data) {
                    const badges = { 'Draft': 'badge-light-warning', 'Posted': 'badge-light-success' };
                    return `<span class="badge ${badges[data] || 'badge-light-secondary'}">${data}</span>`;
                }
            },
            {
                data: "payrollId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    let buttons = `<div class="d-flex gap-2 justify-content-center">
                        <button class="btn btn-sm btn-icon btn-light-primary btn-view-payroll" data-id="${data}" title="Detail">
                            <i class="fa fa-eye"></i>
                        </button>`;
                    if (row.status === 'Draft') {
                        buttons += `<button class="btn btn-sm btn-icon btn-light-danger btn-delete-payroll" data-id="${data}" title="Delete">
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

var btnAddEmployee = $('<a>', {
    href: 'javascript:void(0)',
    class: 'btn btn-outline btn-outline-primary',
    type: 'button',
    id: 'btn-add-employee',
    html: '<i class="fa fa-user-plus"></i> Add Employee'
});
$('#toolbar-section-button').append(btnAddEmployee);

var btnAddPayroll = $('<a>', {
    href: 'javascript:void(0)',
    class: 'btn btn-outline btn-outline-primary',
    type: 'button',
    id: 'btn-add-payroll',
    html: '<i class="fa fa-plus"></i> Buat Penggajian'
});
$('#toolbar-section-button').append(btnAddPayroll);

$(document).on('click', '#btn-add-employee', function () {
    $('#form-employee')[0].reset();
    $('#employee-id').val('');
    $('#modal-employee-title').text('Add Employee');
    $('#modal-employee').modal('show');
});

$(document).on('click', '#btn-add-payroll', function () {
    $('#form-create-payroll')[0].reset();
    $('#pr-date').val(new Date().toISOString().split('T')[0]);
    $('#pr-year').val(new Date().getFullYear());
    $('#pr-month').val(String(new Date().getMonth() + 1));
    $('#modal-create-payroll').modal('show');
});

$('#employee_datatable').on('click', '.btn-edit-employee', function () {
    const id = $(this).data('id');
    const row = employeeTable.row($(this).closest('tr')).data();
    $('#employee-id').val(id);
    $('#emp-code').val(row.employeeCode);
    $('#emp-name').val(row.fullName);
    $('#emp-position').val(row.position || '');
    $('#emp-department').val(row.department || '');
    $('#emp-hire-date').val(row.hireDate ? row.hireDate.split('T')[0] : '');
    $('#emp-salary').val(row.basicSalary);
    $('#emp-bank').val(row.bankAccountNumber || '');
    $('#modal-employee-title').text('Edit Employee');
    $('#modal-employee').modal('show');
});

$('#employee_datatable').on('click', '.btn-delete-employee', function () {
    const id = $(this).data('id');
    confirmDelete('/Payroll/DeleteEmployee', id, 'Delete this employee?', () => employeeTable.ajax.reload());
});

$('#btn-save-employee').on('click', function () {
    if (!$('#form-employee')[0].checkValidity()) { $('#form-employee')[0].reportValidity(); return; }
    const id = $('#employee-id').val();
    const data = {
        employeeCode: $('#emp-code').val(),
        fullName: $('#emp-name').val(),
        position: $('#emp-position').val() || null,
        department: $('#emp-department').val() || null,
        hireDate: $('#emp-hire-date').val() || null,
        basicSalary: parseFloat($('#emp-salary').val()) || 0,
        bankAccountNumber: $('#emp-bank').val() || null
    };
    const url = id ? `/Payroll/UpdateEmployee?id=${id}` : '/Payroll/CreateEmployee';
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-employee').modal('hide');
                employeeTable.ajax.reload();
            } else { Swal.fire('Error', response.message, 'error'); }
        },
        error: function (xhr) { Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error'); }
    });
});

$('#btn-save-payroll').on('click', function () {
    if (!$('#form-create-payroll')[0].checkValidity()) { $('#form-create-payroll')[0].reportValidity(); return; }
    const data = {
        periodMonth: parseInt($('#pr-month').val()),
        periodYear: parseInt($('#pr-year').val()),
        payrollDate: $('#pr-date').val(),
        notes: $('#pr-notes').val() || null
    };
    $.ajax({
        url: '/Payroll/CreatePayroll',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-create-payroll').modal('hide');
                payrollTable.ajax.reload();
            } else { Swal.fire('Error', response.message, 'error'); }
        },
        error: function (xhr) { Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error'); }
    });
});

$('#payroll_datatable').on('click', '.btn-view-payroll', function () {
    viewPayroll($(this).data('id'));
});

$('#payroll_datatable').on('click', '.btn-delete-payroll', function () {
    const id = $(this).data('id');
    confirmDelete('/Payroll/DeletePayroll', id, 'Delete this payroll draft?', () => payrollTable.ajax.reload());
});

function viewPayroll(id) {
    $.get('/Payroll/GetPayrollById', { id: id }, function (data) {
        $('#detail-number').text(data.payrollNumber);
        $('#detail-period').text(`${data.periodMonth}/${data.periodYear}`);
        $('#detail-date').text(new Date(data.payrollDate).toLocaleDateString('en-GB'));
        $('#detail-journal').text(data.journalNumber || '-');
        $('#detail-total').text(formatCurrency(data.totalNetSalary));
        const badges = { 'Draft': 'badge-light-warning', 'Posted': 'badge-light-success' };
        $('#detail-status').html(`<span class="badge ${badges[data.status]}">${data.status}</span>`);

        $('#detail-lines-body').empty();
        data.lines.forEach(function (line) {
            $('#detail-lines-body').append(`
                <tr>
                    <td>${line.employeeCode}</td>
                    <td>${line.employeeName}</td>
                    <td>${formatCurrency(line.basicSalary)}</td>
                    <td>${formatCurrency(line.allowances)}</td>
                    <td>${formatCurrency(line.deductions)}</td>
                    <td>${formatCurrency(line.netSalary)}</td>
                </tr>`);
        });

        $('#btn-post-payroll').hide().data('id', id);
        if (data.canPost) $('#btn-post-payroll').show();

        $('#modal-payroll-detail').modal('show');
    }).fail(function () { Swal.fire('Error', 'Failed to load payroll detail', 'error'); });
}

$('#btn-post-payroll').on('click', function () {
    const id = $(this).data('id');
    Swal.fire({
        title: 'Post Payroll?',
        text: 'Jurnal otomatis akan dibuat (Debit Salary Expense / Credit Payroll Payable).',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Post',
        cancelButtonText: 'Cancel',
        customClass: { confirmButton: 'btn btn-success', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Payroll/PostPayroll',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        $('#modal-payroll-detail').modal('hide');
                        payrollTable.ajax.reload();
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