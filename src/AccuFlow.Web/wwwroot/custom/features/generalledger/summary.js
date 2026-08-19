$(document).ready(function () {
    initializeSummaryTable();
    initializeEventHandlers();
});

function initializeSummaryTable() {
    $('#ledger_summary_datatable').DataTable({
        processing: true,
        searching: true,
        scrollX: false,
        pageLength: 25,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: {
            search: '',
            searchPlaceholder: 'Search'
        },
        initComplete: function (settings, json) {
            if ($('#ledger_summary_datatable_length').find('span.mr-2').length === 0) {
                $('#ledger_summary_datatable_length').prepend('<span class="mr-2" style="margin-right: 10px;">Show</span>');
            }
            
            // Add search icon
            $('#ledger_summary_datatable_filter').css('position', 'relative');
            $('#ledger_summary_datatable_filter').prepend('<i class="fa fa-search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #a1a5b7; pointer-events: none;"></i>');
            
            // Style search box
            $('#ledger_summary_datatable_filter input').css({
                'width': '400px',
                'display': 'inline-block',
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 0.75rem 0.5rem 2.5rem',
                'height': '42px',
                'font-size': '1rem'
            });
            
            // Style show dropdown
            $('#ledger_summary_datatable_length select').css({
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 2rem 0.5rem 0.75rem',
                'height': '42px',
                'font-size': '1rem'
            });
            
            // Make top section flex
            $('.top').css({
                'display': 'flex',
                'align-items': 'center',
                'gap': '15px'
            });
        },
        ajax: {
            url: '/GeneralLedger/GetSummary',
            type: 'GET',
            data: function (d) {
                return {
                    accountType: $('#filter-account-type').val(),
                    dateFrom: $('#filter-date-from').val(),
                    dateTo: $('#filter-date-to').val()
                };
            },
            dataSrc: ''
        },
        order: [[0, 'asc']],
        columns: [
            { data: 'accountCode' },
            { data: 'accountName' },
            { data: 'accountType' },
            {
                data: 'totalDebit',
                render: function (data) {
                    return data.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                }
            },
            {
                data: 'totalCredit',
                render: function (data) {
                    return data.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                }
            },
            {
                data: 'balance',
                render: function (data) {
                    const formatted = Math.abs(data).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                    return data < 0 ? `(${formatted})` : formatted;
                }
            },
            { data: 'transactionCount' },
            {
                data: 'accountId',
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data) {
                    return `
                        <div class="d-flex gap-2 justify-content-center">
                            <button class="btn btn-sm btn-icon btn-light-primary btn-view-ledger" data-id="${data}" title="View Ledger">
                                <i class='fa fa-eye'></i>
                            </button>
                        </div>
                    `;
                }
            }
        ]
    });
}

function initializeEventHandlers() {
    $('#btn-apply-filter').on('click', function () {
        $('#ledger_summary_datatable').DataTable().ajax.reload();
    });

    var btnExport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-export-summary',
        html: '<i class="fa fa-download"></i> Export Summary'
    });
    $('#toolbar-section-button').append(btnExport);

    $(document).on('click', '#btn-export-summary', function () {
        exportSummary();
    });

    $(document).on('click', '.btn-view-ledger', function () {
        const accountId = $(this).data('id');
        viewAccountLedger(accountId);
    });
}

function viewAccountLedger(accountId) {
    window.location.href = `/GeneralLedger/AccountLedger?accountId=${accountId}`;
}

function exportSummary() {
    const params = new URLSearchParams({
        accountType: $('#filter-account-type').val() || '',
        dateFrom: $('#filter-date-from').val() || '',
        dateTo: $('#filter-date-to').val() || ''
    });

    window.location.href = `/GeneralLedger/ExportSummary?${params.toString()}`;
}
