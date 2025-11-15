$(document).ready(function () {

    // 🔹 Simpan data provision untuk setiap dropdown
    let provisionData = {
        packages: {},
        defaultSupports: {},
        specialSupports: {}
    };

    $('.select2').select2({
        placeholder: "Select option(s)",
        allowClear: true,
        width: '100%'
    });

    // 🔹 Inisialisasi provision data dari option yang sudah ada di halaman
    function initializeProvisionData() {
        // Load provision dari package options
        $('.package-select option').each(function () {
            const value = $(this).val();
            const provision = $(this).data('provision');
            if (value && provision !== undefined) {
                provisionData.packages[value] = provision;
            }
        });

        // Load provision dari default support options
        $('.default-support-select option').each(function () {
            const value = $(this).val();
            const provision = $(this).data('provision');
            if (value && provision !== undefined) {
                provisionData.defaultSupports[value] = provision;
            }
        });

        // Load provision dari special support options
        $('.special-support-select option').each(function () {
            const value = $(this).val();
            const provision = $(this).data('provision');
            if (value && provision !== undefined) {
                provisionData.specialSupports[value] = provision;
            }
        });
    }

    // Panggil inisialisasi
    initializeProvisionData();

    console.log('📦 Provision Data Loaded:', provisionData);

    // 🔹 Fungsi untuk update Termination Date
    function updateTerminationDate(section, index) {
        const sectionName = section === 'contracts' ? 'Contracts' : 'PreviousContract';
        
        // Ambil Billing Date
        const billingDateInput = $(`input[name="${sectionName}[${index}].BillingDate"]`);
        const billingDate = billingDateInput.val();
        
        if (!billingDate) {
            console.log('⚠️ Billing Date kosong, skip update termination');
            return;
        }
        
        // Ambil Package yang dipilih
        const packageSelect = $(`.package-select[data-section="${section}"][data-index="${index}"]`);
        const packageId = packageSelect.val();
        
        if (!packageId) {
            console.log('⚠️ Package belum dipilih, skip update termination');
            return;
        }
        
        // Ambil DurationYear dari package via AJAX
        $.ajax({
            url: '/RegisterUnit/GetPackageDuration',
            type: 'GET',
            data: { packageId: packageId },
            success: function (durationYear) {
                // Hitung Termination Year
                const billingYear = new Date(billingDate).getFullYear();
                const terminationYear = billingYear + (durationYear || 0);
                
                // Update input Termination Date
                const terminationInput = $(`input[name="${sectionName}[${index}].TerminationDate"]`);
                terminationInput.val(terminationYear);
                
                console.log(`📅 Termination Date updated: ${billingYear} + ${durationYear} = ${terminationYear}`);
            },
            error: function () {
                console.error('❌ Failed to get package duration');
            }
        });
    }

    // 🔹 Fungsi untuk menghitung Amount Provision
    function calculateAmountProvision(section, index) {
        let packageProvision = 0;
        let defaultSupportProvision = 0;
        let specialSupportProvision = 0;

        // Ambil provision dari Package
        const packageSelect = $(`.package-select[data-section="${section}"][data-index="${index}"]`);
        const packageId = packageSelect.val();
        if (packageId && provisionData.packages[packageId]) {
            packageProvision = parseFloat(provisionData.packages[packageId]) || 0;
        }

        // Ambil provision dari Default Support (multiple)
        const defaultSelect = $(`.default-support-select[data-section="${section}"][data-index="${index}"]`);
        const defaultIds = defaultSelect.val() || [];
        defaultIds.forEach(id => {
            if (provisionData.defaultSupports[id]) {
                defaultSupportProvision += parseFloat(provisionData.defaultSupports[id]) || 0;
            }
        });

        // Ambil provision dari Special Support (multiple)
        const specialSelect = $(`.special-support-select[data-section="${section}"][data-index="${index}"]`);
        const specialIds = specialSelect.val() || [];
        console.log(`🔍 Special Support IDs selected:`, specialIds);
        specialIds.forEach(id => {
            const provision = provisionData.specialSupports[id];
            console.log(`  - ID: ${id}, Provision from data: ${provision}`);
            if (provision !== undefined && provision !== null) {
                specialSupportProvision += parseFloat(provision) || 0;
            }
        });

        // Total Amount Provision
        const totalProvision = packageProvision + defaultSupportProvision + specialSupportProvision;

        // Update input Amount Provision
        const amountInput = $(`input[name="${section === 'contracts' ? 'Contracts' : 'PreviousContract'}[${index}].AmountProvision"]`);
        amountInput.val(totalProvision.toFixed(2));

        console.log(`📊 Calculation for ${section}[${index}]:`, {
            package: packageProvision,
            defaultSupport: defaultSupportProvision,
            specialSupport: specialSupportProvision,
            total: totalProvision
        });
    }

    // 🔹 Event listener untuk perubahan Package, Default Support, Special Support
    $(document).on('change', '.package-select, .default-support-select, .special-support-select', function () {
        const section = $(this).data('section');
        const index = $(this).data('index');
        console.log('🔄 Change detected:', { section, index, element: $(this).attr('class') });
        calculateAmountProvision(section, index);
        
        // Jika package berubah, update termination date juga
        if ($(this).hasClass('package-select')) {
            updateTerminationDate(section, index);
        }
    });

    // 🔹 Event listener untuk perubahan Billing Date
    $(document).on('change', 'input[name*="BillingDate"]', function () {
        const name = $(this).attr('name');
        const match = name.match(/(\w+)\[(\d+)\]\.BillingDate/);
        if (match) {
            const section = match[1].toLowerCase() === 'contracts' ? 'contracts' : 'previous';
            const index = parseInt(match[2]);
            updateTerminationDate(section, index);
        }
    });

    // 🔹 Trigger kalkulasi awal untuk semua contract yang sudah ada
    $('.package-select').each(function() {
        const section = $(this).data('section');
        const index = $(this).data('index');
        if (section !== undefined && index !== undefined) {
            calculateAmountProvision(section, index);
        }
    });

    // 🔹 Saat programme type diganti → update package list
    $('.programme-select').on('change', function () {
        const programmeTypeId = $(this).val();
        const section = $(this).data('section');
        const index = $(this).data('index');

        const packageSelect = $(`.package-select[data-section="${section}"][data-index="${index}"]`);
        const defaultSelect = $(`.default-support-select[data-section="${section}"][data-index="${index}"]`);
        const specialSelect = $(`.special-support-select[data-section="${section}"][data-index="${index}"]`);

        // Ambil data dari unit-data
        const unitData = $('#unit-data');
        const modelId = unitData.data('model-id');
        const type = unitData.data('type');
        const customerId = unitData.data('customer-id');

        $.ajax({
            url: '/RegisterUnit/GetDropdownsByProgramme',
            type: 'GET',
            data: {
                programmeTypeId: programmeTypeId,
                modelId: modelId,
                modelType: type,
                customerId: customerId
            },
            success: function (data) {
                // Simpan provision data untuk packages
                provisionData.packages = {};
                data.packages.forEach(x => {
                    provisionData.packages[x.value] = x.provision;
                });

                // Simpan provision data untuk default supports
                provisionData.defaultSupports = {};
                data.defaultSupports.forEach(x => {
                    provisionData.defaultSupports[x.value] = x.provision;
                });

                // Simpan provision data untuk special supports
                provisionData.specialSupports = {};
                data.specialSupports.forEach(x => {
                    provisionData.specialSupports[x.value] = x.provision;
                });

                // Isi ulang Packages
                packageSelect.empty().append('<option value="">-- Select Package --</option>');
                data.packages.forEach(x => {
                    packageSelect.append(`<option value="${x.value}" data-provision="${x.provision}">${x.text}</option>`);
                });

                // Isi ulang Default Support
                defaultSelect.empty();
                data.defaultSupports.forEach(x => {
                    defaultSelect.append(`<option value="${x.value}" data-provision="${x.provision}">${x.text}</option>`);
                });
                defaultSelect.trigger('change.select2');

                // Isi ulang Special Support
                specialSelect.empty();
                data.specialSupports.forEach(x => {
                    specialSelect.append(`<option value="${x.value}" data-provision="${x.provision}">${x.text}</option>`);
                });
                specialSelect.trigger('change.select2');

                // Reset Amount Provision
                calculateAmountProvision(section, index);
            }
        });
    });

    // 🔹 Kumpulkan data dari semua field Contracts & PreviousContracts
    function collectContractData(sectionName) {
        let contracts = [];

        $(`[name^='${sectionName}']`).each(function () {
            let name = $(this).attr("name");
            let match = name.match(/\[(\d+)\]\.(\w+)/);
            if (!match) return;

            let index = parseInt(match[1]);
            let field = match[2];
            if (!contracts[index]) contracts[index] = {};

            let val = $(this).val();

            // 🔸 Konversi nilai sesuai tipe data
            if (["AmountProvision", "AdminFee"].includes(field)) {
                val = parseFloat(val) || 0;
            } else if (["TerminationDate"].includes(field)) {
                val = parseInt(val) || null;
            } else if (field === "DefaultSupportId") {
                // Multiple select - return array dengan provision
                const ids = Array.isArray(val) ? val : (val ? [val] : []);
                const supports = ids.map(id => ({
                    SupportId: id,
                    Provision: parseFloat(provisionData.defaultSupports[id]) || 0
                }));
                contracts[index]["DefaultSupports"] = supports;
                console.log(`🔍 DefaultSupports for contract[${index}]:`, supports);
                return true; // Skip assignment karena sudah di-set
            } else if (field === "SpecialSupportId") {
                // Multiple select - return array dengan provision
                const ids = Array.isArray(val) ? val : (val ? [val] : []);
                const supports = ids.map(id => ({
                    SupportId: id,
                    Provision: parseFloat(provisionData.specialSupports[id]) || 0
                }));
                contracts[index]["SpecialSupports"] = supports;
                console.log(`🔍 SpecialSupports for contract[${index}]:`, supports);
                return true; // Skip assignment karena sudah di-set
            } else if (field.endsWith("Id")) {
                val = val || null; // GUID
            } else if (field.endsWith("Date")) {
                val = val || null; // Date string
            }

            contracts[index][field] = val;
        });

        return contracts;
    }

    // 🔹 Tombol Save
    $("#btn-save-unit").on("click", function (e) {
        e.preventDefault();

        let other = String($("#unit-data").data("isother")).toLowerCase() === "true";
        let unitId = $("#unit-data").data("id");
        let shippingDate = $("#ShippingDate").val();
        let deliveryDate = $("#DeliveryDate").val();
        let customerName = $("#CustomerName").val();

        let isValid = true;
        $("#ShippingDate, #DeliveryDate, #CustomerName").removeClass("is-invalid");

        if (other !== true) {
            if (!deliveryDate) {
                $("#DeliveryDate").addClass("is-invalid");
                isValid = false;
            }
        } else {
            if (!shippingDate) {
                $("#ShippingDate").addClass("is-invalid");
                isValid = false;
            }
        }

        if (!customerName) {
            $("#CustomerName").addClass("is-invalid");
            isValid = false;
        }

        if (!isValid) return;

        const { previousContracts, contracts } = {
            previousContracts: collectContractData("PreviousContract"),
            contracts: collectContractData("Contracts")
        };

        const requestData = {
            RegisterUnitId: unitId,
            CustomerName: customerName,
            DeliveryDate: deliveryDate || null,
            ShippingDate: shippingDate || null,
            PreviousContracts: previousContracts,
            Contracts: contracts
        };

        console.log("📦 Data yang dikirim ke server:", requestData);
        console.log("📦 Contracts Detail:", JSON.stringify(contracts, null, 2));

        // 🔹 Konfirmasi simpan
        Swal.fire({
            title: "Are you sure?",
            text: "Do you want to save these changes?",
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Yes, save it",
            cancelButtonText: "Cancel",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/RegisterUnit/Edit",
                    type: "POST",
                    data: JSON.stringify(requestData),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: "success",
                                title: "Saved",
                                text: response.message
                            }).then(() => window.location.href = "/RegisterUnit");
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Failed",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Oops...",
                            text: "An error occurred while saving data!"
                        });
                    }
                });
            }
        });
    });
});
