var selectedprogrammeIds = [];

$(document).ready(function () {

    const style = document.createElement("style");
    style.innerHTML = `
  .badge-light-orange {
    background-color: #fff3cd !important;
    color: #b96d00 !important;
  }

  .badge-light-purple {
    background-color: #f3e8ff !important;
    color: #6f42c1 !important;
  }

  /* Warna biru muda untuk seluruh kolom Transfer To Service */
    #tblDetailBudgetDetail thead th:nth-child(11),
    #tblDetailBudgetDetail tbody td:nth-child(11),
    #tblDetailBudgetDetail tfoot th:nth-child(11) {
    background-color: #e6f4ff !important;
    }

    /* Override gaya striping DataTables dan Bootstrap */
    #tblDetailBudgetDetail tbody tr.odd td:nth-child(11),
    #tblDetailBudgetDetail tbody tr.even td:nth-child(11) {
    background-color: #e6f4ff !important;
    }

    /* Warna saat hover */
    #tblDetailBudgetDetail tbody tr:hover td:nth-child(11) {
    background-color: #d6ecff !important;
    }

    /* Footer total */
    #tblDetailBudgetDetail tfoot th {
    background-color: #f3f6f9;
    font-weight: bold;
    }

`;

    document.head.appendChild(style);

    var btnAdd = $('<a>', {
        href: '#',
        class: 'btn btn-primary',
        type: 'button',
        id: 'btn-add-budget',
        html: `<i class="fa fa-plus"></i> Create Budget Transfer`
    });
    $('#toolbar-section-button').append(btnAdd);

    let tblDetailDex; // variabel global

    // Event klik tombol tambah
    $(document).on("click", "#btn-add-budget", function (e) {
        e.preventDefault();

        $("#modalAddBudget").modal("show");

        if ($.fn.DataTable.isDataTable("#tblDetailBudgetAdd")) {
            tblDetailDex.ajax.reload();
            return;
        }

        // Inisialisasi DataTable
        tblDetailDex = $("#tblDetailBudgetAdd").DataTable({
            processing: true,
            serverSide: true,
            ajax: {
                url: "/BudgetTransferUnit/GetData",
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
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

            columns: [
                {
                    data: "registerUnitId",
                    render: function (data) {
                        return `<input type="checkbox" class="row-check" data-id="${data}">`;
                    },
                    orderable: false
                },
                {
                    data: null,
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    }
                },
                { data: "customerName" },
                { data: "modelTypeSN" },
                { data: "joinProgramme" },
                { data: "programmeType" },
                { data: "package" },
                { data: "provisionType" },
                { data: "amountProvisionPackage" },
                { data: "amountSupportDefault" },
                { data: "amountSpecialSupport" },
                { data: "transferToService" },
                { data: "profit" },
                { data: "invoiceToUT" },
                { data: "note" }
            ],
            scrollX: true,
            order: [],
            columnDefs: [
                { targets: 0, width: "40px", className: "text-center" },
                { targets: 1, width: "40px", className: "text-center" }
            ]
        });

        // Search manual
        $("#searchModelTypeAdd").on("keyup", function () {
            tblDetailDex.search(this.value).draw();
        });

        // Select All
        $(document).on("change", "#checkAll", function () {
            $(".row-check").prop("checked", $(this).prop("checked"));
        });
    });


    $(document).on("click", "#add-budget", function (e) {
        e.preventDefault();

        // ambil semua ID yang dicentang
        const selectedIds = [];
        $("#tblDetailBudgetAdd  tbody input[type='checkbox']:checked").each(function () {
            selectedIds.push($(this).data("id"));
        });
        console.log("budget");
        console.log(selectedIds);

        if (selectedIds.length === 0) {
            Swal.fire({
                icon: "warning",
                title: "No data selected",
                text: "Please select at least one item before saving.",
            });
            return;
        }

        // tampilkan konfirmasi sebelum kirim
        Swal.fire({
            title: "Confirm Save",
            //    html: `
            //    <p>Are you sure you want to save these <b>${selectedIds.length}</b> items?</p>
            //    <pre style="text-align:left; background:#f8f9fa; padding:10px; border-radius:5px; max-height:200px; overflow:auto;">${selectedIds.join("\n")}</pre>
            //`,
            html: `
            <p>Are you sure you want to save ?</p>
        `,
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Yes, Save",
            cancelButtonText: "Cancel",
            reverseButtons: true,
        }).then((result) => {
            if (result.isConfirmed) {
                // kirim data ke controller
                $.ajax({
                    url: "/BudgetTransferUnit/SaveBudgetTransfer",
                    type: "POST",
                    data: JSON.stringify({ RegisterUnitIds: selectedIds }),
                    contentType: "application/json; charset=utf-8",
                    success: function (response) {
                        // Tutup modal tambah budget
                        $('#modalAddBudget').modal('hide');

                        // Setelah modal benar-benar tertutup, tampilkan pesan sukses
                        $('#modalAddBudget').one('hidden.bs.modal', function () {
                            Swal.fire({
                                icon: 'success',
                                title: 'Success',
                                text: 'Data has been saved successfully!',
                                timer: 2000,
                                showConfirmButton: false
                            });

                            // Reload tabel utama (daftar BudgetTransferUnit)
                            BudgetTransferUnit();
                        });
                    },
                    error: function (xhr) {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: xhr.responseText || "Something went wrong while saving data."
                        });
                    }
                });
            }
        });
    });



    BudgetTransferUnit()

});

function BudgetTransferUnit() {

    //checkdate  
    $("#filter-check-date").val("");
    $("#filter-check-date").attr("placeholder", "Select Date Range");

    $("#filter-check-date").daterangepicker({
        autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        }
    });

    $("#filter-check-date").on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD') + ' - ' + picker.endDate.format('YYYY/MM/DD'));

        $(this).data('startDate', picker.startDate.format('YYYY/MM/DD'));
        $(this).data('endDate', picker.endDate.format('YYYY/MM/DD'));
    });

    $("#filter-check-date").on('cancel.daterangepicker', function (ev, picker) {
        $(this).val("");
        $(this).attr("placeholder", "Select Date Range");

        $(this).removeData('startDate');
        $(this).removeData('endDate');
    });

    if ($.fn.DataTable.isDataTable('#unit_datatable')) {
        $('#unit_datatable').DataTable().destroy();
        //$('#unit_datatable').remove(); // hapus element table lama
    }

    // rebuild table
    var table = `<table id="unit_datatable" class="table table-striped table-row-bordered gy-5 gs-7 border rounded w-100">
                    <thead>
                        <tr>
                            <th>No</th>
                            <th>Transfer Period</th>
                            <th>Total</th>
                            <th>Download</th>
                            <th>Action</th>
                        </tr>
                   </thead>
                 </table>`;

    $("#unit-datatable").html(table);

    var datatable = $("#unit_datatable").DataTable({
        serverSide: true,
        searching: true,
        scrollX: true,
        pageLength: 10,
        responsive: true,
        "dom": '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: {
            search: '',
            searchPlaceholder: 'Search',
            emptyTable: "No data displayed",
            infoEmpty: "No entries to show"
        },
        ajax: {
            url: '/BudgetTransferUnit/DatatableBudgetTransferUnit',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                var startCheckDate = $('#filter-check-date').data('startDate');
                var endCheckDate = $('#filter-check-date').data('endDate');

                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    startCheckDate: startCheckDate != null || startCheckDate != undefined ? moment(startCheckDate).format('YYYY-MM-DD') : null,
                    endCheckDate: endCheckDate != null || endCheckDate != undefined ? moment(endCheckDate).format('YYYY-MM-DD') : null,
                });
            }
        },

        "initComplete": function () {
            if ($('#unit_datatable_length').find('span.mr-2').length === 0) {
                $('#unit_datatable_length').prepend('<span class="mr-2">Show</span>');
            }
        },
        order: [[1, 'asc']],
        columns: [
            {
                data: null,
                orderable: false,
                render: function (data, type, row, meta) {
                    return meta.settings._iDisplayStart + meta.row + 1;
                }
            },
            {
                data: "transferPeriode",
                render: function (data) {
                    return moment(data).format("DD-MMMM-YYYY");
                }
            },
            {
                data: "totalUnit",
                render: function (data) {
                    return data;
                }
            },
            {
                data: "transferPeriode",
                orderable: false,
                className: "text-center align-middle",
                render: function (data, type, row) {
                    return `
            <div class="d-inline-flex gap-1">
                <a  href="/BudgetTransferUnit/DownloadExcel?period=${data}" class="btn btn-outline-primary btn-sm btn-dex" data-id="${data}">
                    <i class="bi bi-download"></i> Unit
                </a>
              
            </div>
        `;
                }
            },
            {
                data: "transferPeriode",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    return `<div class="action-buttons">
                                <a href="#" class="btn-icon-table button-icon-primary no-border btn-detail" data-period="${data}">
                                   <i class='fa fa-eye'></i>
                                </a>
                                 <a href="#" class="btn-icon-table button-icon-success no-border btn-edit-budget" data-period="${data}">
                                   <i class='fa fa-pencil'></i>
                                </a>                              
                                
                                <button class="btn-icon-table button-icon-warning no-border btn-audit" data-id="${data}" data-createdby="${row.createdBy}" data-createdat="${row.createdAt}" data-updatedby="${row.updatedBy}" data-updatedat="${row.updatedAt}">
                                        <svg width="14" height="14" viewBox="0 0 14 14" fill="none" xmlns="http://www.w3.org/2000/svg">
                                            <path fill-rule="evenodd" clip-rule="evenodd" d="M4.7421 9.39312C4.7421 9.64094 4.94429 9.84184 5.19371 9.84184H8.80661C9.05603 9.84184 9.25823 9.64094 9.25823 9.39312C9.25823 9.1453 9.05603 8.9444 8.80661 8.9444H5.19371C4.94429 8.9444 4.7421 9.1453 4.7421 9.39312Z" fill="#FFA800"/>
                                            <path fill-rule="evenodd" clip-rule="evenodd" d="M4.7421 6.99996C4.7421 7.24778 4.94429 7.44868 5.19371 7.44868H7.00016C7.24958 7.44868 7.45178 7.24778 7.45178 6.99996C7.45178 6.75214 7.24958 6.55124 7.00016 6.55124H5.19371C4.94429 6.55124 4.7421 6.75214 4.7421 6.99996Z" fill="#FFA800"/>
                                            <path fill-rule="evenodd" clip-rule="evenodd" d="M5.23168 1.6723C5.419 1.36755 5.74755 1.16663 6.11812 1.16663H7.8822C8.25278 1.16663 8.58133 1.36755 8.76864 1.6723L9.099 2.20977C9.42537 2.23515 9.73487 2.26246 10.005 2.28788C10.7917 2.36189 11.4046 2.98521 11.4663 3.76549C11.5493 4.81413 11.6668 6.52005 11.6668 7.78418C11.6668 8.97722 11.5623 10.3318 11.4807 11.2093C11.4092 11.9777 10.8003 12.5808 10.029 12.6537C9.19083 12.7329 7.94504 12.8333 7.00016 12.8333C6.05528 12.8333 4.8095 12.7329 3.97137 12.6537C3.20002 12.5808 2.59116 11.9777 2.51967 11.2093C2.43803 10.3318 2.3335 8.97722 2.3335 7.78418C2.3335 6.52005 2.45103 4.81412 2.534 3.76549C2.59574 2.98521 3.20864 2.36189 3.99529 2.28788C4.26546 2.26246 4.57496 2.23515 4.90133 2.20977L5.23168 1.6723ZM4.68107 3.12793C4.4675 3.14576 4.26484 3.16397 4.08043 3.18132C3.73315 3.21399 3.46198 3.48787 3.43445 3.83582C3.35152 4.88393 3.23672 6.55873 3.23672 7.78418C3.23672 8.93519 3.33815 10.257 3.41906 11.1267C3.45043 11.4638 3.71582 11.7281 4.05687 11.7603C4.89301 11.8393 6.10174 11.9359 7.00016 11.9359C7.89859 11.9359 9.10732 11.8393 9.94346 11.7603C10.2845 11.7281 10.5499 11.4638 10.5813 11.1267C10.6622 10.257 10.7636 8.93519 10.7636 7.78418C10.7636 6.55873 10.6488 4.88393 10.5659 3.83582C10.5383 3.48787 10.2672 3.21399 9.91989 3.18132C9.73549 3.16397 9.53282 3.14576 9.31925 3.12793C9.29572 3.25513 9.25115 3.37653 9.18888 3.48601C9.01874 3.78518 8.69717 4.0234 8.28364 4.0234H5.71669C5.30315 4.0234 4.98158 3.78518 4.81144 3.48601C4.74918 3.37653 4.7046 3.25514 4.68107 3.12793ZM6.11812 2.06406C6.08604 2.06406 6.03856 2.08141 6.00253 2.14005L5.60109 2.79316C5.59518 2.80278 5.59016 2.81234 5.58591 2.82182C5.55354 2.89406 5.56025 2.97847 5.59781 3.04451C5.63364 3.10752 5.67831 3.12596 5.71669 3.12596H8.28364C8.32202 3.12596 8.36668 3.10752 8.40252 3.04451C8.44007 2.97847 8.44679 2.89406 8.41441 2.82182C8.41017 2.81234 8.40514 2.80277 8.39923 2.79316L7.9978 2.14005C7.96176 2.08141 7.91429 2.06406 7.8822 2.06406H6.11812Z" fill="#FFA800"/>
                                        </svg>
                                </button>
                            </div>`;
                }
            }
        ]
    });


    $('#applyFilter-budget-unit').on('click', function () {

        datatable.ajax.reload();
    });


    let tblDetailBudget; // variabel global DataTable

    $(document).on("click", ".btn-detail", function (e) {
        e.preventDefault();

        const period = $(this).data("period");
        const url = `/BudgetTransferUnit/Detail?period=${period}`;

        // buka modal
        $("#modalDetailBudget").modal("show");

        // hapus event lama sebelum pasang baru
        $("#modalDetailBudget").off("shown.bs.modal");

        $("#modalDetailBudget").on("shown.bs.modal", function () {

            // kalau DataTable sudah ada, destroy dulu biar ga duplikat
            if ($.fn.DataTable.isDataTable("#tblDetailBudgetDetail")) {
                $("#tblDetailBudgetDetail").DataTable().clear().destroy();
                $("#tblDetailBudgetDetail tfoot th").html(""); // bersihkan footer
            }

            // inisialisasi DataTable
            tblDetailBudget = $("#tblDetailBudgetDetail").DataTable({
                ajax: {
                    url: url,
                    dataSrc: ""
                },
                paging: false,
                searching: true,
                info: false,
                columns: [
                    { data: null, render: (data, type, row, meta) => meta.row + 1 },
                    { data: "customerName" },
                    { data: "modelTypeSN" },
                    { data: "joinProgramme" },
                    { data: "programmeType" },
                    { data: "package" },
                    { data: "provisionType" },
                    {
                        data: "amountProvisionPackage",
                        render: $.fn.dataTable.render.number(',', '.', 0, '')
                    },
                    {
                        data: "amountSupportDefault",
                        render: $.fn.dataTable.render.number(',', '.', 0, '')
                    },
                    {
                        data: "amountSpecialSupport",
                        render: $.fn.dataTable.render.number(',', '.', 0, '')
                    },
                    {
                        data: "transferToService",
                        render: $.fn.dataTable.render.number(',', '.', 0, '')
                    },
                    {
                        data: "profit",
                        render: $.fn.dataTable.render.number(',', '.', 0, '')
                    },
                    { data: "invoiceToUT" },
                    { data: "note" }
                ],
                footerCallback: function (row, data, start, end, display) {
                    const api = this.api();

                    const intVal = (i) =>
                        typeof i === "string"
                            ? i.replace(/[\$,]/g, "") * 1
                            : typeof i === "number"
                                ? i
                                : 0;

                    // total Transfer To Service (kolom index 10)
                    const totalTransfer = api
                        .column(10, { page: "all" })
                        .data()
                        .reduce((a, b) => intVal(a) + intVal(b), 0);

                    $(api.column(10).footer()).html(totalTransfer.toLocaleString("en-US"));

                }
            });

            // fitur pencarian manual
            $("#searchModelTypeDetail").off("keyup").on("keyup", function () {
                tblDetailBudget.search(this.value).draw();
            });
        });
    });

    let tblEditBudget;

    // Event klik tombol edit
    $(document).on("click", ".btn-edit-budget", function (e) {
        e.preventDefault();

        const period = $(this).data("period");

        // buka modal edit
        $("#modalEditBudget").modal("show");
        $("#editPeriod").val(period);

        // Jika DataTable sudah ada, destroy dulu agar tidak duplikat
        if ($.fn.DataTable.isDataTable("#tblEditBudget")) {
            $("#tblEditBudget").DataTable().clear().destroy();
        }

        // Inisialisasi DataTable baru
        tblEditBudget = $("#tblEditBudget").DataTable({
            processing: true,
            serverSide: true, // karena backend sudah return BaseDatatableResponse
            ajax: {
                url: "/BudgetTransferUnit/EditDataByPeriod",
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: function (d) {
                    return JSON.stringify({
                        Draw: d.draw,
                        Search: d.search.value || "",
                        OrderBy: (d.order && d.order.length > 0)
                            ? d.columns[d.order[0].column].data
                            : null,
                        OrderType: (d.order && d.order.length > 0)
                            ? d.order[0].dir
                            : null,
                        Page: (d.start / d.length) + 1,
                        Size: d.length,
                        Period: period // 🟢 tambahan di sini
                    });
                },
                dataSrc: "data"
            },

            columns: [
                {
                    data: "registerUnitId",
                    render: function (data, type, row) {
                        const checked = row.isTransferred ? "checked" : "";
                        return `<input type="checkbox" class="row-check-edit" data-id="${data}" ${checked}>`;
                    },
                    orderable: false,
                    className: "text-center",
                    width: "40px"
                },
                {
                    data: null,
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    },
                    className: "text-center",
                    width: "40px"
                },
                { data: "customerName" },
                { data: "modelTypeSN" },
                { data: "joinProgramme" },
                { data: "programmeType" },
                { data: "package" },
                { data: "provisionType" },
                {
                    data: "amountProvisionPackage",
                    render: $.fn.dataTable.render.number(',', '.', 0, '')
                },
                {
                    data: "amountSupportDefault",
                    render: $.fn.dataTable.render.number(',', '.', 0, '')
                },
                {
                    data: "amountSpecialSupport",
                    render: $.fn.dataTable.render.number(',', '.', 0, '')
                },
                {
                    data: "transferToService",
                    render: $.fn.dataTable.render.number(',', '.', 0, '')
                },
                {
                    data: "profit",
                    render: $.fn.dataTable.render.number(',', '.', 0, '')
                },
                { data: "invoiceToUT" },
                { data: "note" }
            ],
            scrollX: true,
            order: [],
            footerCallback: function (row, data, start, end, display) {
                const api = this.api();

                const intVal = (i) =>
                    typeof i === "string"
                        ? i.replace(/[\$,]/g, "") * 1
                        : typeof i === "number"
                            ? i
                            : 0;

                // total kolom TransferToService (index 11)
                const totalTransfer = api
                    .column(11, { page: "all" })
                    .data()
                    .reduce((a, b) => intVal(a) + intVal(b), 0);

                $(api.column(11).footer()).html(totalTransfer.toLocaleString("en-US"));
            }
        });

        // Search manual
        $("#searchModelTypeEdit").off("keyup").on("keyup", function () {
            tblEditBudget.search(this.value).draw();
        });

        // Select All
        $(document).off("change", "#checkAllEdit").on("change", "#checkAllEdit", function () {
            $(".row-check-edit").prop("checked", $(this).prop("checked"));
        });
    });


    $(document).on("click", "#btnSaveBudgetEdit", function () {


        let period = $("#editPeriod").val();
        let selectedIds = [];

        $(".row-check-edit:checked").each(function () {
            selectedIds.push($(this).data("id"));
        });

        if (!period) {
            Swal.fire({
                icon: "warning",
                title: "Missing period",
                text: "Please select a transfer period before saving.",
            });
            return;
        }

        document.activeElement.blur();
        Swal.fire({
            title: "Save changes?",
            text: "Are you sure you want to update the budget transfer data?",
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Yes, save it",
            cancelButtonText: "Cancel",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/BudgetTransferUnit/UpdateBudgetTransfer",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        Period: period,
                        SelectedRegisterUnitIds: selectedIds
                    }),
                    success: function () {
                        $('#modalEditBudget').modal('hide');
                        $('#modalEditBudget').one('hidden.bs.modal', function () {
                            Swal.fire({
                                icon: 'success',
                                title: 'Success',
                                text: 'Budget transfer updated successfully!',
                                timer: 1500,
                                showConfirmButton: false
                            });
                            datatable.ajax.reload();
                        });
                    },
                    error: function (xhr) {
                        Swal.fire({
                            icon: "error",
                            title: "Failed to save",
                            text: xhr.responseText || "An unexpected error occurred.",
                        });
                    }
                });
            }
        });
    });


}

function isNullOrEmpty(value) {
    return value === null || value === undefined || value === "undefined" || value === "";
}


