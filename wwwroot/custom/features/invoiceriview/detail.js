$(document).ready(function () {

    var invoiceId = $('#invoice-id').val();

    // ==============================
    // JQUERY VALIDATION: FORM INVOICE
    // ==============================
    var $formInvoice = $('#form-invoice');

    $formInvoice.validate({
        ignore: [],
        errorClass: 'is-invalid',
        validClass: 'is-valid',
        errorElement: 'div',
        errorPlacement: function (error, element) {
            error.addClass('invalid-feedback d-block');
            if (element.hasClass('form-select') && element.data('control') === 'select2') {
                element.next('.select2-container').after(error);
            } else {
                error.insertAfter(element);
            }
        },
        highlight: function (element) {
            if ($(element).hasClass('form-select') && $(element).data('control') === 'select2') {
                $(element).next('.select2-container').find('.select2-selection').addClass('is-invalid');
            } else {
                $(element).addClass('is-invalid');
            }
        },
        unhighlight: function (element) {
            if ($(element).hasClass('form-select') && $(element).data('control') === 'select2') {
                $(element).next('.select2-container').find('.select2-selection').removeClass('is-invalid');
            } else {
                $(element).removeClass('is-invalid');
            }
        },
        rules: {
            invoiceDate: { required: true },
            dueDate: { required: true },
            invoiceCode: { required: true }
        },
        messages: {
            invoiceDate: 'Invoice Date is required',
            dueDate: 'Due Date is required',
            invoiceCode: 'Invoice Code is required'
        },
        submitHandler: function () {
            // Validate PIC and Rate manually
            const pic = $('#invoice-pic').val();
            const rate = $('#invoice-rate').val();

            let hasError = false;

            // Validate PIC
            if (!pic || pic.length === 0) {
                $('#invoice-pic').next('.select2-container').find('.select2-selection').addClass('is-invalid');
                if (!$('#invoice-pic').next('.select2-container').next('.invalid-feedback').length) {
                    $('#invoice-pic').next('.select2-container').after('<div class="invalid-feedback d-block">PIC is required</div>');
                }
                hasError = true;
            } else {
                $('#invoice-pic').next('.select2-container').find('.select2-selection').removeClass('is-invalid');
                $('#invoice-pic').next('.select2-container').next('.invalid-feedback').remove();
            }

            // Validate Rate
            if (!rate || parseFloat(rate) <= 0) {
                $('#invoice-rate').addClass('is-invalid');
                if (!$('#invoice-rate').next('.invalid-feedback').length) {
                    $('#invoice-rate').after('<div class="invalid-feedback d-block">Rate is required and must be greater than 0</div>');
                }
                hasError = true;
            } else {
                $('#invoice-rate').removeClass('is-invalid');
                $('#invoice-rate').next('.invalid-feedback').remove();
            }

            if (hasError) {
                return false;
            }

            // Collect bank accounts
            const bankAccounts = [];
            $('.bank-account-field input').each(function () {
                const value = $(this).val().trim();
                if (value) {
                    bankAccounts.push(value);
                }
            });

            // Collect CCs
            const ccs = [];
            $('.cc-field select').each(function () {
                const value = $(this).val();
                if (value) {
                    ccs.push(value);
                }
            });

            // Calculate current values
            const totalUSD = parseFloat($('#invoice-total-usd').val()) || 0;
            const rateValue = parseFloat($('#invoice-rate').val()) || 0;
            const taxFormula = $('#invoice-formula-tax').val() || '11/12';
            const vatPercentage = parseFloat($('#invoice-formula-vat').val()) || 12;
            const subTotal = totalUSD * rateValue;

            let taxBasis = 0;
            if (taxFormula && taxFormula.includes('/')) {
                const parts = taxFormula.split('/');
                const numerator = parseFloat(parts[0]) || 0;
                const denominator = parseFloat(parts[1]) || 1;
                if (denominator !== 0) {
                    taxBasis = (subTotal * numerator) / denominator;
                }
            }

            const vat = (taxBasis * vatPercentage) / 100;
            const grandTotal = subTotal + taxBasis + vat;

            // Prepare data - use PascalCase to match C# model
            const data = {
                InvoiceReviewId: $('#invoice-id').val(),
                InvoiceDate: $('#invoice-date').val(),
                DueDate: $('#invoice-duedate').val(),
                InvoiceNumber: $('#invoice-number').val(),
                InvoiceCode: $('#invoice-code').val(),
                Pic: pic,
                Rate: rateValue,
                Subtotal: Math.round(subTotal),
                TaxBasic: taxFormula,  // Keep as string "11/12"
                VatPercentage: Math.round(vatPercentage),
                Tax: Math.round(taxBasis),
                Vat: Math.round(vat),
                GrandTotal: Math.round(grandTotal),
                BankAccount: bankAccounts,
                Ccs: ccs
            };

            console.log('Submitting data:', JSON.stringify(data, null, 2));

            // Show confirmation
            Swal.fire({
                title: 'Confirmation',
                text: 'Are you sure you want to submit this invoice?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#18A4F5',
                confirmButtonText: 'Yes, Submit',
                cancelButtonText: 'Cancel',
                reverseButtons: true,
                customClass: {
                    confirmButton: 'btn btn-primary',
                    cancelButton: 'btn button-outline-blue-komatsu'
                },
                buttonsStyling: false
            }).then((result) => {
                if (result.isConfirmed) {
                    // Submit via AJAX
                    $.ajax({
                        url: '/InvoiceReview/EditInvoice',
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify(data),
                        beforeSend: function () {
                            Swal.fire({
                                title: 'Processing...',
                                text: 'Please wait while submitting invoice',
                                allowOutsideClick: false,
                                didOpen: () => {
                                    Swal.showLoading();
                                }
                            });
                        },
                        success: function (response) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Success',
                                text: 'Invoice has been submitted successfully!',
                                timer: 2000,
                                willClose: () => {
                                    window.location.href = '/InvoiceReview';
                                }
                            });
                        },
                        error: function (xhr) {
                            let errorMessage = 'Failed to submit invoice.';
                            if (xhr.responseJSON && xhr.responseJSON.message) {
                                errorMessage = xhr.responseJSON.message;
                            }
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: errorMessage
                            });
                        }
                    });
                }
            });

            return false; // Prevent default form submission
        }
    });

    // Remove validation error when PIC changes
    $('#invoice-pic').on('change', function () {
        if ($(this).val() && $(this).val().length > 0) {
            $(this).next('.select2-container').find('.select2-selection').removeClass('is-invalid');
            $(this).next('.select2-container').next('.invalid-feedback').remove();
        }
    });

    // Remove validation error when Rate changes
    $('#invoice-rate').on('input', function () {
        if ($(this).val() && parseFloat($(this).val()) > 0) {
            $(this).removeClass('is-invalid');
            $(this).next('.invalid-feedback').remove();
        }
    });

    // ==============================
    // JQUERY VALIDATION: FORM FORMULA
    // ==============================

    // Custom method: Tax Basis Formula -> number/number (contoh 11/12)
    $.validator.addMethod("taxFormula", function (value, element) {
        // this.optional() biar kalau required yg nendang, bukan taxFormula
        return this.optional(element) || /^\d+\/\d+$/.test(value);
    }, "Format must be number/number (e.g. 11/12)");

    var $formFormula = $('#form-formula');

    $formFormula.validate({
        ignore: [],
        errorClass: 'is-invalid',
        validClass: 'is-valid',
        errorElement: 'span',
        errorPlacement: function (error, element) {
            // styling pesan error
            error.addClass('invalid-feedback d-block mt-1');

            // cari wrapper baris (colon + input)
            var $rowFlex = element.closest('.d-flex');

            if ($rowFlex.length) {
                // taruh error DI LUAR .d-flex -> jadi di bawah input
                error.insertAfter($rowFlex);
            } else {
                // fallback (misal nanti ada field lain tanpa .d-flex)
                error.insertAfter(element);
            }
        },
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        },
        rules: {
            taxBasisFormula: {
                required: true,
                taxFormula: true,
                normalizer: function (value) {
                    var cleaned = value.replace(/[^0-9/]/g, '');
                    $(this).val(cleaned);
                    return cleaned;
                }
            },
            vatPercentage: {
                required: true,
                digits: true,
                normalizer: function (value) {
                    var cleaned = value.replace(/[^0-9]/g, '');
                    $(this).val(cleaned);
                    return cleaned;
                }
            }
        },
        messages: {
            taxBasisFormula: {
                required: "Tax Basis Formula is required"
            },
            vatPercentage: {
                required: "VAT Percentage is required",
                digits: "VAT Percentage must be a number"
            }
        },
        submitHandler: function () {
            const taxFormula = $('#tax-basis-formula').val();
            const vatPercentage = $('#vat-percentage').val();

            // Show confirmation dialog
            Swal.fire({
                title: 'Confirmation',
                text: 'Are you sure you want to apply these changes?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#18A4F5',
                confirmButtonText: 'Yes, Apply',
                cancelButtonText: 'Cancel',
                reverseButtons: true,
                customClass: {
                    confirmButton: 'btn btn-primary',
                    cancelButton: 'btn button-outline-blue-komatsu'
                },
                buttonsStyling: false
            }).then((result) => {
                if (result.isConfirmed) {
                    // Update hidden inputs with new formula values
                    $('#invoice-formula-tax').val(taxFormula);
                    $('#invoice-formula-vat').val(vatPercentage);

                    // Recalculate all invoice values with new formula
                    calculateInvoiceValues();

                    // Close modal
                    $('#formula-modal').modal('hide');

                    // Show success message
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Formula has been applied successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    });
                }
            });
        }
    });

    // Function to calculate invoice values based on rate
    function calculateInvoiceValues() {
        const totalUSD = parseFloat($('#invoice-total-usd').val()) || 0;
        const rate = parseFloat($('#invoice-rate').val()) || 0;
        const taxFormula = $('#invoice-formula-tax').val() || '11/12';
        const vatPercentage = parseFloat($('#invoice-formula-vat').val()) || 12;

        // Calculate Amount and SubTotal (same value)
        const subTotal = totalUSD * rate;

        // Calculate Tax Basis from formula
        let taxBasis = 0;
        if (taxFormula && taxFormula.includes('/')) {
            const parts = taxFormula.split('/');
            const numerator = parseFloat(parts[0]) || 0;
            const denominator = parseFloat(parts[1]) || 1;
            
            if (denominator !== 0) {
                taxBasis = (subTotal * numerator) / denominator;
            }
        }

        // Calculate VAT
        const vat = (taxBasis * vatPercentage) / 100;

        // Calculate Grand Total
        const grandTotal = subTotal + taxBasis + vat;

        // Format numbers with thousand separator
        const formatNumber = (num) => {
            return num.toFixed(0).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        };

        // Update display values
        $('#invoice-amount').text('IDR ' + formatNumber(subTotal));
        $('#invoice-subtotal-display').text('IDR ' + formatNumber(subTotal));
        $('#invoice-tax-display').text('IDR ' + formatNumber(taxBasis));
        $('#invoice-vat-display').text('IDR ' + formatNumber(vat));
        $('#invoice-grandtotal-display').text('IDR ' + formatNumber(grandTotal));

        // Update hidden input for subtotal (for modal calculation)
        $('#invoice-subtotal').val(subTotal);
    }

    $('#invoice-rate').on('keyup', function () {
        calculateInvoiceValues();
    });


    // Optional: UX realtime sanitizing (boleh dihapus, karena normalizer sudah handle)
    $('#tax-basis-formula').on('input', function () {
        this.value = this.value.replace(/[^0-9/]/g, '');
        calculatePreview();
    });

    // Function to calculate and update preview
    function calculatePreview() {
        const formula = $('#tax-basis-formula').val();
        const vatPercent = parseFloat($('#vat-percentage').val()) || 0;
        const subTotal = parseFloat($('#invoice-subtotal').val()) || 0;

        let taxBasis = 0;
        let vat = 0;
        let grandTotal = 0;

        // Calculate tax basis from formula (e.g., 11/12)
        if (formula && formula.includes('/')) {
            const parts = formula.split('/');
            const numerator = parseFloat(parts[0]) || 0;
            const denominator = parseFloat(parts[1]) || 1;
            
            if (denominator !== 0) {
                taxBasis = (subTotal * numerator) / denominator;
            }
        }

        // Calculate VAT
        vat = (taxBasis * vatPercent) / 100;

        // Calculate grand total (SubTotal + Tax Basis + VAT)
        grandTotal = subTotal + taxBasis + vat;

        // Format numbers with thousand separator
        const formatNumber = (num) => {
            return num.toFixed(0).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        };

        // Update preview
        $('#preview-tax-basis').text(': ' + formatNumber(taxBasis));
        $('#preview-vat').text(': ' + formatNumber(vat));
        $('#preview-grand-total').text(': ' + formatNumber(grandTotal));
    }

    $('#tax-basis-formula').change(function () {
        calculatePreview();
    });

    $('#vat-percentage').on('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
        calculatePreview();
    });

    $('#vat-percentage').change(function () {
        calculatePreview();
    });

    // ==============================
    // BUTTON: LOAD FORMULA
    // ==============================
    $('#btn-formula').click(function (e) {
        e.preventDefault();

        // Get current formula values from hidden inputs
        const currentTaxFormula = $('#invoice-formula-tax').val() || '11/12';
        const currentVatPercentage = $('#invoice-formula-vat').val() || '12';

        // Set values to modal inputs
        $('#tax-basis-formula').val(currentTaxFormula);
        $('#vat-percentage').val(currentVatPercentage);

        // Calculate and show preview
        calculatePreview();

        // Reset validation state
        $formFormula.validate().resetForm();
        $formFormula.find('.is-invalid, .is-valid').removeClass('is-invalid is-valid');

        // Show modal
        $('#formula-modal').modal('show');
    });

    // ==============================
    // ADD/REMOVE BANK ACCOUNT
    // ==============================
    $('#btn-add-bank').click(function (e) {
        e.preventDefault();

        const newBankField = `
            <div class="mb-1 bank-account-field d-flex gap-2">
                A/C # 
                <input type="text" class="form-control" placeholder="Enter account number" style="width: 150px;" />
                <button type="button" class="btn btn-danger btn-sm btn-delete-bank"><i class='fa fa-trash-alt'></i></button>
            </div>
        `;
        $(this).before(newBankField);
    });

    $(document).on('click', '.btn-delete-bank', function () {
        $(this).closest('.bank-account-field').remove();
    });

    // ==============================
    // ADD/REMOVE CC
    // ==============================
    $('#btn-add-cc').click(function (e) {
        e.preventDefault();

        $.ajax({
            url: '/InvoiceReview/GetListUser',
            type: 'GET',
            success: function (users) {
                let options = '<option value="">Select CC</option>';
                users.forEach(function (user) {
                    options += `<option value="${user.value}">${user.text}</option>`;
                });

                const newCcField = `
                    <div class="mb-1 cc-field d-flex gap-2 align-items-center">
                        <select class="form-select select2-cc" style="width: 250px;">
                            ${options}
                        </select>
                        <button type="button" class="btn btn-danger btn-sm btn-delete-cc"><i class='fa fa-trash-alt'></i></button>
                    </div>
                `;
                $('#btn-add-cc').before(newCcField);
                $('.select2-cc').last().select2({
                    placeholder: 'Select CC',
                    allowClear: true,
                    width: '250px'
                });
            },
            error: function () {
                alert('Failed to load user list');
            }
        });
    });

    $(document).on('click', '.btn-delete-cc', function () {
        $(this).closest('.cc-field').remove();
    });

    // Calculate initial values on page load
    calculateInvoiceValues();
});
