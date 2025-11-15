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
  `;
    document.head.appendChild(style);

    var btnDownload = $('<a>', {
        href: "/RegisterUnit/DownloadTemplateRegisterUnit",
        class: 'btn button-outline-blue-komatsu',
        type: 'button',
        id: 'btn-download-billing',
        html: `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M5.16659 15.1665C4.28253 15.1665 3.43468 14.8153 2.80956 14.1902C2.18444 13.5651 1.83325 12.7172 1.83325 11.8332V9.83317C1.83325 9.65636 1.90349 9.48679 2.02851 9.36177C2.15354 9.23674 2.32311 9.1665 2.49992 9.1665C2.67673 9.1665 2.8463 9.23674 2.97132 9.36177C3.09635 9.48679 3.16659 9.65636 3.16659 9.83317V11.8332C3.16659 12.3636 3.3773 12.8723 3.75237 13.2474C4.12744 13.6225 4.63615 13.8332 5.16659 13.8332H11.8333C12.3637 13.8332 12.8724 13.6225 13.2475 13.2474C13.6225 12.8723 13.8333 12.3636 13.8333 11.8332V9.83317C13.8333 9.65636 13.9035 9.48679 14.0285 9.36177C14.1535 9.23674 14.3231 9.1665 14.4999 9.1665C14.6767 9.1665 14.8463 9.23674 14.9713 9.36177C15.0963 9.48679 15.1666 9.65636 15.1666 9.83317V11.8332C15.1666 12.7172 14.8154 13.5651 14.1903 14.1902C13.5652 14.8153 12.7173 15.1665 11.8333 15.1665H5.16659Z" fill="#18A4F5"/>
                <path fill-rule="evenodd" clip-rule="evenodd" d="M5.0235 7.76642C4.96221 7.82904 4.91387 7.90312 4.88123 7.98443C4.84859 8.06574 4.8323 8.15269 4.83329 8.2403C4.83429 8.32791 4.85254 8.41447 4.887 8.49502C4.92147 8.57558 4.97148 8.64854 5.03417 8.70976L8.03417 11.6431C8.15873 11.7648 8.32599 11.833 8.50017 11.833C8.67435 11.833 8.84161 11.7648 8.96617 11.6431L11.9662 8.70976C12.0301 8.64887 12.0813 8.5759 12.1168 8.49507C12.1523 8.41424 12.1714 8.32715 12.173 8.23888C12.1746 8.15061 12.1586 8.0629 12.126 7.98085C12.0934 7.89879 12.0449 7.82403 11.9832 7.7609C11.9214 7.69777 11.8478 7.64753 11.7665 7.61309C11.6852 7.57865 11.5979 7.5607 11.5096 7.56028C11.4213 7.55987 11.3338 7.57699 11.2522 7.61066C11.1706 7.64433 11.0965 7.69388 11.0342 7.75642L9.16684 9.58242V3.83309C9.16684 3.65628 9.0966 3.48671 8.97157 3.36168C8.84655 3.23666 8.67698 3.16642 8.50017 3.16642C8.32336 3.16642 8.15379 3.23666 8.02877 3.36168C7.90374 3.48671 7.8335 3.65628 7.8335 3.83309V9.58242L5.96684 7.75642C5.90422 7.69513 5.83014 7.64679 5.74883 7.61415C5.66752 7.58151 5.58057 7.56522 5.49296 7.56621C5.40535 7.5672 5.31879 7.58545 5.23824 7.61992C5.15768 7.65439 5.08471 7.7044 5.0235 7.76709V7.76642Z" fill="#18A4F5"/>
              </svg> Download Template`
    });


    // Buat tombol upload
    var btnUpload = $('<button>', {
        class: 'btn button-outline-blue-komatsu',
        type: 'button',
        id: 'btn-upload-registet-unit',
        html: `<svg width="24" height="24" viewBox="0 0 24 24" fill="none"
            xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" clip-rule="evenodd"
            d="M5.16659 15.1665C4.28253 15.1665 3.43468 14.8153 2.80956 14.1902C2.18444 13.5651 1.83325 12.7172 1.83325 11.8332V9.83317C1.83325 9.65636 1.90349 9.48679 2.02851 9.36177C2.15354 9.23674 2.32311 9.1665 2.49992 9.1665C2.67673 9.1665 2.8463 9.23674 2.97132 9.36177C3.09635 9.48679 3.16659 9.65636 3.16659 9.83317V11.8332C3.16659 12.3636 3.3773 12.8723 3.75237 13.2474C4.12744 13.6225 4.63615 13.8332 5.16659 13.8332H11.8333C12.3637 13.8332 12.8724 13.6225 13.2475 13.2474C13.6225 12.8723 13.8333 12.3636 13.8333 11.8332V9.83317C13.8333 9.65636 13.9035 9.48679 14.0285 9.36177C14.1535 9.23674 14.3231 9.1665 14.4999 9.1665C14.6767 9.1665 14.8463 9.23674 14.9713 9.36177C15.0963 9.48679 15.1666 9.65636 15.1666 9.83317V11.8332C15.1666 12.7172 14.8154 13.5651 14.1903 14.1902C13.5652 14.8153 12.7173 15.1665 11.8333 15.1665H5.16659Z"
            fill="#18A4F5"/>
            <path fill-rule="evenodd" clip-rule="evenodd"
            d="M5.0235 7.23309C4.96221 7.17048 4.91387 7.0964 4.88123 7.01508C4.84859 6.93377 4.8323 6.84682 4.83329 6.75921C4.83429 6.6716 4.85254 6.58504 4.887 6.50449C4.92147 6.42394 4.97148 6.35097 5.03417 6.28976L8.03417 3.35642C8.15873 3.23467 8.32599 3.1665 8.50017 3.1665C8.67435 3.1665 8.84161 3.23467 8.96617 3.35642L11.9662 6.28976C12.0301 6.35064 12.0813 6.42361 12.1168 6.50444C12.1523 6.58528 12.1714 6.67236 12.173 6.76063C12.1746 6.8489 12.1586 6.93661 12.126 7.01867C12.0934 7.10072 12.0449 7.17548 11.9832 7.23861C11.9214 7.30174 11.8478 7.35199 11.7665 7.38643C11.6852 7.42086 11.5979 7.43881 11.5096 7.43923C11.4213 7.43965 11.3338 7.42252 11.2522 7.38885C11.1706 7.35518 11.0965 7.30564 11.0342 7.24309L9.16684 5.41709V11.1664C9.16684 11.3432 9.0966 11.5128 8.97157 11.6378C8.84655 11.7629 8.67698 11.8331 8.50017 11.8331C8.32336 11.8331 8.15379 11.7629 8.02877 11.6378C7.90374 11.5128 7.8335 11.3432 7.8335 11.1664V5.41709L5.96684 7.24309C5.90422 7.30438 5.83014 7.35273 5.74883 7.38536C5.66752 7.418 5.58057 7.43429 5.49296 7.4333C5.40535 7.43231 5.31879 7.41406 5.23824 7.37959C5.15768 7.34512 5.08471 7.29511 5.0235 7.23242V7.23309Z"
            fill="#18A4F5"/>
           </svg> Upload`
    });

    // Buat input file tersembunyi & append ke body
    var inputUpload = $('<input>', {
        type: 'file',
        id: 'input-upload-excel',
        accept: '.xlsx,.xls',
        style: 'display:none'
    }).appendTo('body'); // ⬅ penting! append ke DOM



    // Tambahkan ke halaman
    $('#toolbar-container').append(btnUpload, inputUpload); // sesuaikan id container-nya


    // Default aktif ke tab overview
    $('#tab-unit-reviewt a[href="#tab-unit"]').tab('show');
    $('#toolbar-section-button').append(btnDownload).append(btnUpload);
    RegisterUnit()

    $('#tab-unit-review a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        var target = $(e.target).attr("href");
        $('#btn-download-billing').remove();
        $('#btn-upload-billing').remove();
        if (target === "#tab-unit") {
            $('#dex-container').prop('hidden', true);
            $('#unit-container').prop('hidden', false);
            $('#toolbar-section-button').append(btnDownload).append(btnUpload);
            RegisterUnit()
        } else {
            $('#dex-container').prop('hidden', false);
            $('#unit-container').prop('hidden', true);
            RegisterDex();
        }
    });

    // Saat tombol diklik → buka file explorer
    $('#btn-upload-registet-unit').on('click', function () {
        $('#input-upload-excel').click();
    });

    // Saat file dipilih → kirim ke server
    $('#input-upload-excel').on('change', function (e) {
        var file = e.target.files[0];
        if (!file) return;

        var formData = new FormData();
        formData.append('file', file);

        Swal.fire({
            title: 'Upload file ini?',
            text: file.name,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Ya, Upload',
            cancelButtonText: 'Batal'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/RegisterUnit/UploadExcel',
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (response) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Berhasil',
                            text: 'File berhasil diupload.'
                        });
                        // bisa reload datatable di sini jika perlu
                    },
                    error: function (xhr, status, error) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Gagal',
                            text: 'Upload gagal: ' + (xhr.responseText || error)
                        });
                    }
                });
            }
        });
    });

});

function RegisterUnit() {

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



    //registerdate

    $("#filter-register-date").val("");
    $("#filter-register-date").attr("placeholder", "Select Date Range");

    $("#filter-register-date").daterangepicker({
        autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        }
    });

    $("#filter-register-date").on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD') + ' - ' + picker.endDate.format('YYYY/MM/DD'));

        $(this).data('startDate', picker.startDate.format('YYYY/MM/DD'));
        $(this).data('endDate', picker.endDate.format('YYYY/MM/DD'));
    });

    $("#filter-register-date").on('cancel.daterangepicker', function (ev, picker) {
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
                            <th>Check Date</th>
                            <th>Customer Name</th>
                            <th>Model</th>
                            <th>SN</th>
                            <th>Contract Type</th>
                            <th>Status</th>
                            <th>RegisterDate</th>
                            <th>Methode</th>
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
            url: '/RegisterUnit/DatatableRegisterUnit',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                var startCheckDate = $('#filter-check-date').data('startDate');
                var endCheckDate = $('#filter-check-date').data('endDate');
                var startRegDate = $('#filter-register-date').data('startDate');
                var endRegDate = $('#filter-register-date').data('endDate');
                var statusReg = $('#filter-status').val();
                var methodeReg = $('#filter-methode').val();
                statusReg = statusReg && statusReg.length > 0 ? parseInt(statusReg) : null;
                methodeReg = methodeReg && methodeReg.length > 0 ? parseInt(methodeReg) : null;

                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    status: statusReg,
                    methode: methodeReg,
                    startCheckDate: startCheckDate != null || startCheckDate != undefined ? moment(startCheckDate).format('YYYY-MM-DD') : null,
                    endCheckDate: endCheckDate != null || endCheckDate != undefined ? moment(endCheckDate).format('YYYY-MM-DD') : null,
                    startRegDate: startRegDate != null || startRegDate != undefined ? moment(startRegDate).format('YYYY-MM-DD') : null,
                    endRegDate: endRegDate != null || endRegDate != undefined ? moment(endRegDate).format('YYYY-MM-DD') : null,
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
                data: "checkDate",
                render: function (data) {
                    return moment(data).format("DD-MMMM-YYYY");
                }
            },
            {
                data: "customerName",
                render: function (data) {
                    return data;
                }
            },
            { data: "model", defaultContent: "-" },
            { data: "sn", defaultContent: "-" },
            { data: "contractType", defaultContent: "-" },
            {
                data: "status",
                orderable: true,
                render: function (data) {
                    if (data === null || data === undefined || data === '') {
                        return '-';
                    }

                    switch (data) {
                        case 0:
                            return `<span class="badge badge-light-warning">Pending</span>`
                            break;
                        case 1:
                            return `<span class="badge badge-light-orange">Incomplete</span>` // orange
                            break;
                        case 2:
                            return `<span class="badge badge-light-success">Completed</span>`
                            break;
                        case 3:
                            return `<span class="badge badge-light-primary">InReview</span>`
                            break;
                        case 4:
                            return `<span class="badge badge-light-purple">Ready to Register</span>` //ungu
                            break;
                        case 5:
                            return `<span class="badge badge-light-danger">Rejected</span>`
                            break;
                        case 6:
                            return `<span class="badge badge-light-info">Registered DEX</span>`
                            break;
                        case 7:
                            return `<span class="badge badge-light-dark">Extended</span>`
                            break;
                    }

                    return data;
                }
            },
            { data: "registeredDate", defaultContent: "-" },
            { data: "methode", defaultContent: "-" },
            {
                data: "registerUnitId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    return `<div class="action-buttons">
                                <a href="/RegisterUnit/DetailRegister/${data}" class="btn-icon-table button-icon-primary no-border" data-id="${data}">
                                   <i class='fa fa-eye'></i>
                                </a>
                                 <a href="#" class="btn-icon-table button-icon-success no-border btn-edit-register" data-id="${data}" data-status="${row.status}">
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


    $('#applyFilter-register-unit').on('click', function () {
        datatable.ajax.reload();
    });

    $('#unit_datatable').on('click', '.btn-delete', function () {
        var registerUnitId = $(this).data('id');
        var status = $(this).data('status'); // ambil status dari row
        console.log(`status ${registerUnitId}`)

        if (status !== 5) {
            Swal.fire({
                title: 'Data cannot be deleted',
                text: 'Data cannot be deleted because its curren status does not allow deletion',
                icon: 'info',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false
            });
            return; // stop disini
        }
        Swal.fire({
            title: 'Delete selected data?',
            text: "Are you sure you want to delete this data?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            reverseButtons: true, // Swap positions of confirm & cancel
            customClass: {
                confirmButton: 'btn btn-danger', // custom CSS class for confirm
                cancelButton: 'btn button-outline-blue-komatsu' // custom CSS class for cancel
            },
            buttonsStyling: false // Allow custom Bootstrap classes
        })
            .then((result) => {
                if (result.isConfirmed) {
                    // Ajax call to delete
                    $.ajax({
                        url: `/RegisterUnit/Deleted`,
                        type: 'DELETE',
                        contentType: 'application/json',
                        data: JSON.stringify(registerUnitId),
                        success: function (response) {
                            Swal.fire(
                                'Successfully',
                                'Selected data successfully deleted',
                                'success'
                            );
                            // Optionally reload datatable or remove row
                            $('#unit_datatable').DataTable().ajax.reload();
                        },
                        error: function () {
                            Swal.fire(
                                'Error!',
                                'Something went wrong while deleting.',
                                'error'
                            );
                        }
                    });
                }
            });
    });

    $('#unit_datatable').on('click', '.btn-audit', function () {
        $('#audit-trail-modal').modal('show');

        console.log($(this).data("createdby"));

        var createdBy = isNullOrEmpty($(this).data('createdby')) ? "-" : $(this).data('createdby');
        var createdAtRaw = $(this).data('createdat');
        var createdAt = isNullOrEmpty(createdAtRaw) ? "-" : moment(createdAtRaw).format('DD/MMMM/YYYY');
        var updatedBy = isNullOrEmpty($(this).data('updatedby')) ? "-" : $(this).data('updatedby') == "00000000-0000-0000-0000-000000000000" ? "System" : $(this).data('updatedby');
        var updatedAtRaw = $(this).data('updatedat');
        var updatedAt = isNullOrEmpty(updatedAtRaw) ? "-" : moment(updatedAtRaw).format('DD/MMMM/YYYY');

        $('#audit-created-by').html(createdBy)
        $('#audit-created-date').html(createdAt)
        $('#audit-updated-by').html(updatedBy)
        $('#audit-updated-date').html(updatedAt)
    });

    $(document).on("click", ".btn-edit-register", function (e) {
        e.preventDefault();

        var id = $(this).data("id");
        var status = parseInt($(this).data("status"));

        if (status === 0 || status === 1 || status === 5) { //untuk latihan sajua nanti 3 di ganti sama 0
            window.location.href = "/RegisterUnit/EditRegister/" + id;
        } else {
            Swal.fire({
                title: "Tidak bisa di edit",
                text: "Data dengan status Pending, Incomplete atau Rejected tidak dapat diubah.",
                icon: "warning",
                confirmButtonText: "OK"
            });
        }
    });


}

function isNullOrEmpty(value) {
    return value === null || value === undefined || value === "undefined" || value === "";
}

function RegisterDex() {

    $("#filter-period-dex").val("");
    $("#filter-period-dex").attr("placeholder", "Select Date Range");

    $("#filter-period-dex").daterangepicker({
        autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        }
    });

    $("#filter-period-dex").on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD') + ' - ' + picker.endDate.format('YYYY/MM/DD'));

        $(this).data('startDate', picker.startDate.format('YYYY/MM/DD'));
        $(this).data('endDate', picker.endDate.format('YYYY/MM/DD'));
    });

    $("#filter-period-dex").on('cancel.daterangepicker', function (ev, picker) {
        $(this).val("");
        $(this).attr("placeholder", "Select Date Range");

        $(this).removeData('startDate');
        $(this).removeData('endDate');
    });

    if ($.fn.DataTable.isDataTable('#dex_datatable')) {
        $('#dex_datatable').DataTable().destroy();
        $('#dex_datatable').remove(); // hapus element table lama
    }

    // rebuild table
    var table = `<table id="dex_datatable" class="table table-striped table-row-bordered gy-5 gs-7 border rounded w-100">
                    <thead>
                        <tr>
                            <th>No</th>
                            <th>Period</th>
                            <th>Total Unit</th>
                            <th>New</th>
                            <th>Extension</th>
                            <th>Status</th>
                            <th class="text-center">Action Download</th>
                            <th>Action</th>
                        </tr>
                   </thead>
                 </table>`;

    $("#dex-datatable").html(table);

    var datatable = $("#dex_datatable").DataTable({
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
            url: '/RegisterUnit/DatatableDex',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                var startDate = $('#filter-period-dex').data('startDate');
                var endDate = $('#filter-period-dex').data('endDate');
                var statusDex = $('#filter-status-dex').val();
                statusDex = statusDex && statusDex.length > 0 ? parseInt(statusDex) : null;

                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    status: statusDex,
                    startCheckDate: startDate != null ? moment(startDate).format('YYYY-MM-DD') : null,
                    endCheckDate: endDate != null ? moment(endDate).format('YYYY-MM-DD') : null,
                });
            }
        },
        "initComplete": function () {
            if ($('#dex_datatable_length').find('span.mr-2').length === 0) {
                $('#dex_datatable_length').prepend('<span class="mr-2">Show</span>');
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
                data: "period",
                render: function (data) {
                    return moment(data).format("DD-MMMM-YYYY HH:mm:ss");
                }
            },
            {
                data: "totalUnit",
                render: function (data) {
                    return data;
                }
            },
            {
                data: "new",
                render: function (data) {
                    return data;
                }
            },
            {
                data: "extension",
                render: function (data) {
                    return data;
                }
            },
            {
                data: "status",
                orderable: true,
                render: function (data) {
                    if (data === null || data === undefined || data === '') {
                        return '-';
                    }

                    switch (data) {
                        case 0:
                            return `<span class="badge badge-light-warning">Pending</span>`
                            break;
                        case 1:
                            return `<span class="badge badge-light-orange">Incomplete</span>` // orange
                            break;
                        case 2:
                            return `<span class="badge badge-light-success">Completed</span>`
                            break;
                        case 3:
                            return `<span class="badge badge-light-primary">InReview</span>`
                            break;
                        case 4:
                            return `<span class="badge badge-light-purple">Ready to Register</span>` //ungu
                            break;
                        case 5:
                            return `<span class="badge badge-light-danger">Rejected</span>`
                            break;
                        case 6:
                            return `<span class="badge badge-light-info">Registered DEX</span>`
                            break;
                        case 7:
                            return `<span class="badge badge-light-dark">Record Only</span>`
                            break;
                    }

                    return data;
                }
            },
            {
                data: "period",
                orderable: false,
                className: "text-center align-middle",
                render: function (data, type, row) {
                    return `
            <div class="d-inline-flex gap-2">
                <a  href="/RegisterUnit/DownloadDex?periode=${data}" class="btn btn-outline-primary btn-sm btn-dex" data-id="${data}">
                    <i class="bi bi-download"></i> Download DEX
                </a>
                <a href="/RegisterUnit/DownloadDexSystem?periode=${data}" class="btn btn-outline-secondary btn-sm btn-system" data-id="${data}">
                    <i class="bi bi-download"></i> Download System
                </a>
            </div>
        `;
                }
            },

            {
                data: "period",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    return `<div class="action-buttons">
                                <a href="#" class="btn-icon-table button-icon-primary no-border btn-detail" data-period="${data}">
                                   <i class='fa fa-eye'></i>
                                </a>
                            </div>`;
                }
            }
        ]
    });

    $('#applyFilter-dex').on('click', function () {
        datatable.ajax.reload();
    });

    $('#dex_datatable').on('click', '.btn-delete', function () {
        var unitReviewId = $(this).data('id');

        Swal.fire({
            title: 'Delete selected data?',
            text: "Are you sure you want to delete this data?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            reverseButtons: true, // Swap positions of confirm & cancel
            customClass: {
                confirmButton: 'btn btn-danger', // custom CSS class for confirm
                cancelButton: 'btn button-outline-blue-komatsu' // custom CSS class for cancel
            },
            buttonsStyling: false // Allow custom Bootstrap classes
        }).then((result) => {
            if (result.isConfirmed) {
                // Ajax call to delete
                $.ajax({
                    url: `/RegisterUnit/Delete/${unitReviewId}`,
                    type: 'DELETE',
                    success: function (response) {
                        Swal.fire(
                            'Successfully',
                            'Selected data successfully deleted',
                            'success'
                        );
                        // Optionally reload datatable or remove row
                        $('#unit_datatable').DataTable().ajax.reload();
                    },
                    error: function () {
                        Swal.fire(
                            'Error!',
                            'Something went wrong while deleting.',
                            'error'
                        );
                    }
                });
            }
        });
    });

    $('#dex_datatable').on('click', '.btn-audit', function () {
        $('#audit-trail-modal').modal('show');

        var createdBy = isNullOrEmpty($(this).data('createdby')) ? "-" : $(this).data('createdby');
        var createdAtRaw = $(this).data('createdat');
        var createdAt = isNullOrEmpty(createdAtRaw) ? "-" : moment(createdAtRaw).format('DD/MMMM/YYYY');
        var updatedBy = isNullOrEmpty($(this).data('updatedby')) ? "-" : $(this).data('updatedby');
        var updatedAtRaw = $(this).data('updatedat');
        var updatedAt = isNullOrEmpty(updatedAtRaw) ? "-" : moment(updatedAtRaw).format('DD/MMMM/YYYY');

        $('#audit-created-by').html(createdBy)
        $('#audit-created-date').html(createdAt)
        $('#audit-updated-by').html(updatedBy)
        $('#audit-updated-date').html(updatedAt)
    });



    let tblDetailDex; // global variable

    $(document).on("click", ".btn-detail", function (e) {
        e.preventDefault();

        var period = $(this).data("period");
        var url = `/RegisterUnit/DetailDex?period=${period}`;

        // buka modal
        $("#modalDetail").modal("show");

        // destroy dulu kalau tabel sudah pernah dibuat
        if (tblDetailDex) {
            tblDetailDex.destroy();
            $("#tblDetailDex tbody").empty();
        }

        // inisialisasi DataTable dengan AJAX
        tblDetailDex = $("#tblDetailDex").DataTable({
            ajax: url,
            paging: false,
            searching: false,
            info: false,
            columns: [
                {
                    data: null,
                    render: function (data, type, row, meta) {
                        return meta.row + 1; // nomor urut
                    }
                },
                { data: "customerName" },
                { data: "model" },
                { data: "sn" },
                { data: "programmeType" },
                { data: "package" },
                { data: "defaultSupport" }
            ]
        });
    });
}

