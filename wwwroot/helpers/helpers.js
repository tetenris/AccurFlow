const helpers = {
    StatusPMPEnum: {
        DRAFT: "DRAFT",
        PREPARE_DATASOURCE: "Prepare Datasource",
        PLEASE_UPLOAD_DATASOURCE: "Please Upload Datasource OR Check Smr Warranty",
        PROCESSING: "PROCESSING",
        APPROVED: "APPROVED",
        REVISED: "REVISED",
        REJECTED: "REJECTED",
        DONE: "DONE",
        DATASOURCE_READY: "datasource ready",
    },
    
    initializeBulkCheckListListeners: function() {
        this.handleAllCheckbox();
        this.handleSingleItemCheckboxes();
        this.updateSendToApproverState();
        this.updateApprovalSendToApproverState();
    },

    // Event Listener for Check/uncheck All Buttons in Datatables
    handleAllCheckbox: function() {
        $(document).on("click", ".cbx-all", function () {
            $("input:checkbox.cbx-item:not(:disabled)").prop('checked', this.checked);
            helpers.updateSendToApproverState();
            helpers.updateApprovalSendToApproverState();
        });
    },

    // Disabled select all checkbox when checkboxes in dataTables disabled
    updateAllCheckboxState: function() {
        const allCheckboxes = $('.cbx-item');
        const enabledCheckboxes = allCheckboxes.not(':disabled');
        const allChecked = enabledCheckboxes.length > 0 && enabledCheckboxes.length === enabledCheckboxes.filter(':checked').length;
        
        $('.cbx-all').prop('checked', allChecked);

        if (allCheckboxes.length === allCheckboxes.filter(':disabled').length) {
            $('.cbx-all').prop('disabled', true);
        } else {
            $('.cbx-all').prop('disabled', false);
        }
    },

    // Event Listener for individual cbx-item checkboxes
    handleSingleItemCheckboxes: function() {
        $(document).on('change', '.cbx-item', function() {
            helpers.updateAllCheckboxState();
            helpers.updateSendToApproverState();
            helpers.updateApprovalSendToApproverState();
        });
    },

    // Event Listener for Trigger Disabled/Enabled Bulk Button
    updateSendToApproverState: function() {
        const anyChecked = $('.cbx-item:checked').length > 0;
        $('#bulkButton').prop('disabled', !anyChecked);
    },

    updateApprovalSendToApproverState: function () {
        const anyChecked = $('.cbx-item:checked').length > 0;
        $('#bulkButton').prop('disabled', !anyChecked);
        $('#bulkButtonApprove').prop('disabled', !anyChecked); //for handle button bulk approve in approval
    },

    // Event Listener for Checkbox Is Active
    updateStateCheckboxIsActive: function() {
        const setCheckboxState = (selector, value) => {
            const isActive = (value === "True");
            $(selector).prop('checked', isActive);
        };

        const createCheckboxValue = $("#is_active").val();
        // for checkbox Edit using modal in the same page
        const EditCheckboxValue = $("#is_active_edit").val();

        $(".cbx").each(function() {
            setCheckboxState(this, createCheckboxValue);
        });

        $(".cbx-edit").each(function() {
            setCheckboxState(this, EditCheckboxValue);
        });
    },
    
    
    
    // Render data for dataTables Status
    renderStatusPMP(data) {
        const statusClasses = {
            [helpers.StatusPMPEnum.DRAFT]: "status-draft",
            [helpers.StatusPMPEnum.PREPARE_DATASOURCE]: "status-prepare-data",
            [helpers.StatusPMPEnum.PLEASE_UPLOAD_DATASOURCE]: "status-upload-data",
            [helpers.StatusPMPEnum.PROCESSING]: "status-processing",
            [helpers.StatusPMPEnum.APPROVED]: "status-approved",
            [helpers.StatusPMPEnum.REVISED]: "status-revised",
            [helpers.StatusPMPEnum.REJECTED]: "status-rejected",
            [helpers.StatusPMPEnum.DONE]: "status-data-ready",
            [helpers.StatusPMPEnum.DATASOURCE_READY]: "status-data-ready"
        };

        const normalizedData = data.toLowerCase() === "datasource ready" ? "Datasource Ready" : data;
        const statusClass = statusClasses[normalizedData] || "status-draft";

        return `<span class="text-status ${statusClass}">${normalizedData}</span>`;
    },

    // Render data for dataTables Boolean True/false
    renderBoolean: function(data) {
        if (data === true) {
            return '<p class="text-success"><b>TRUE</b></p>';
        } else if (data === false) {
            return '<p class="text-danger"><b>FALSE</b></p>';
        } else {
            return '-';
        }
    },

    renderApprovalStatus: function(data) {
        if (data === "PROCESSING") {
            return '<span class="badge badge-primary badge-sm">' + data + '</span>';
        } else if (data === "APPROVED") {
            return '<span class="badge badge-success badge-sm">' + data + '</span>';
        } else if (data === "REJECTED") {
            return '<span class="badge badge-danger badge-sm">' + data + '</span>';
        } else {
            return '<span class="badge badge-warning badge-sm">' + data + '</span>';
        }
    },

    // Render data for dataTables Active/Inactive
    renderStatusData: function(data) {
        if (data === true) {
            return '<span class="badge badge-success">Active</span>';
        } else if (data === false) {
            return '<span class="badge badge-danger">Inactive</span>';
        } else {
            return '-';
        }
    },

    // Render data for handling null values
    renderNullValue: function(value) {
        return value === null || value === "" ? "-" : value;
    },

    renderNullValueWithRedText: function(value, flag) {
        if (value === null || value === "") {
            return "-"
        }
        else {
            if (flag === true || flag === "true") {
                return '<p class="text-danger">' + value + '</p>'
            }
            else {
                return '<p>' + value + '</p>'
            }
        }
    },

    // Show Swal loading indicator
    showLoading: function() {
        Swal.fire({
            title: 'Processing...',
            text: 'Please wait while the request is being processed.',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    },

    // Hide Swal loading indicator
    hideLoading: function() {
        Swal.close();
    },
    
    cleanModals: function(id) {
        // Close any open modals to prevent double opening
        $('.modal').modal('hide');

        // Show the modal after a brief delay to ensure the previous modal is closed
        setTimeout(() => {
            $(id).modal('show');
        }, 500);
    }
};

