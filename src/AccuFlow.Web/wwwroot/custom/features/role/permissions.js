// Role Menu Permissions Management

let currentRoleIdForPermissions = null;

function loadMenuPermissions(roleId) {
    currentRoleIdForPermissions = roleId;
    
    $.ajax({
        url: '/Role/GetRoleMenuPermissions',
        type: 'GET',
        data: { roleId: roleId || '' },
        success: function(data) {
            console.log('Permissions loaded:', data);
            if (data && data.menus) {
                renderMenuPermissions(data.menus);
            } else {
                console.error('Invalid response format:', data);
                $('#menu-permissions-container').html('<div class="alert alert-danger">Invalid response format</div>');
            }
        },
        error: function(xhr, status, error) {
            console.error('Failed to load permissions:', xhr.responseText);
            $('#menu-permissions-container').html('<div class="alert alert-danger">Failed to load permissions: ' + error + '</div>');
        }
    });
}

function renderMenuPermissions(menus) {
    let html = '';
    
    // Group menus by parent
    const parentMenus = menus.filter(m => !m.parentMenuId);
    const childMenus = menus.filter(m => m.parentMenuId);
    
    parentMenus.forEach(parent => {
        const children = childMenus.filter(c => c.parentMenuId === parent.menuId);
        
        html += `<div class="mb-4">`;
        
        // Check if parent should be checked:
        // 1. If parent has direct permission (canView = true)
        // 2. OR if ALL children have canView = true
        let parentChecked = parent.canView || false;
        
        // If parent doesn't have direct permission, check if all children are checked
        if (!parentChecked && children.length > 0) {
            const allChildrenChecked = children.every(c => c.canView);
            parentChecked = allChildrenChecked;
        }
        
        html += `<div class="d-flex align-items-center mb-2">`;
        html += `<div class="form-check me-3" style="min-width: 80px;">`;
        html += `<input class="form-check-input permission-all parent-menu-checkbox" type="checkbox" data-menu-id="${parent.menuId}" data-parent="true" ${parentChecked ? 'checked' : ''}>`;
        html += `<label class="form-check-label">All</label>`;
        html += `</div>`;
        html += `<div class="fw-bold">${parent.menuName}</div>`;
        html += `</div>`;
        
        // Child menus
        children.forEach(child => {
            if (child.availableActions && child.availableActions.length > 0) {
                html += renderMenuPermissionRow(child, true);
            }
        });
        
        html += `</div>`;
    });
    
    $('#menu-permissions-container').html(html);
    
    // Attach event handlers
    attachPermissionHandlers();
}

function renderMenuPermissionRow(menu, isChild) {
    const indent = isChild ? 'ms-4' : '';
    let html = `<div class="d-flex align-items-center mb-2 ${indent}">`;
    
    // All checkbox
    html += `<div class="form-check me-3" style="min-width: 80px;">`;
    html += `<input class="form-check-input permission-all" type="checkbox" data-menu-id="${menu.menuId}">`;
    html += `<label class="form-check-label">All</label>`;
    html += `</div>`;
    
    // Menu name
    html += `<div class="me-3" style="min-width: 200px;">${menu.menuName}</div>`;
    
    // Permission checkboxes
    const actions = menu.availableActions;
    
    if (actions.includes('view')) {
        html += `<div class="form-check me-3">`;
        html += `<input class="form-check-input permission-check" type="checkbox" data-menu-id="${menu.menuId}" data-action="view" ${menu.canView ? 'checked' : ''}>`;
        html += `<label class="form-check-label">View</label>`;
        html += `</div>`;
    }
    
    if (actions.includes('add')) {
        html += `<div class="form-check me-3">`;
        html += `<input class="form-check-input permission-check" type="checkbox" data-menu-id="${menu.menuId}" data-action="add" ${menu.canAdd ? 'checked' : ''}>`;
        html += `<label class="form-check-label">Add</label>`;
        html += `</div>`;
    }
    
    if (actions.includes('edit')) {
        html += `<div class="form-check me-3">`;
        html += `<input class="form-check-input permission-check" type="checkbox" data-menu-id="${menu.menuId}" data-action="edit" ${menu.canEdit ? 'checked' : ''}>`;
        html += `<label class="form-check-label">Edit</label>`;
        html += `</div>`;
    }
    
    if (actions.includes('delete')) {
        html += `<div class="form-check me-3">`;
        html += `<input class="form-check-input permission-check" type="checkbox" data-menu-id="${menu.menuId}" data-action="delete" ${menu.canDelete ? 'checked' : ''}>`;
        html += `<label class="form-check-label">Delete</label>`;
        html += `</div>`;
    }
    
    if (actions.includes('post')) {
        html += `<div class="form-check me-3">`;
        html += `<input class="form-check-input permission-check" type="checkbox" data-menu-id="${menu.menuId}" data-action="post" ${menu.canPost ? 'checked' : ''}>`;
        html += `<label class="form-check-label">Post</label>`;
        html += `</div>`;
    }
    
    if (actions.includes('reverse')) {
        html += `<div class="form-check me-3">`;
        html += `<input class="form-check-input permission-check" type="checkbox" data-menu-id="${menu.menuId}" data-action="reverse" ${menu.canReverse ? 'checked' : ''}>`;
        html += `<label class="form-check-label">Reverse</label>`;
        html += `</div>`;
    }
    
    html += `</div>`;
    return html;
}

function attachPermissionHandlers() {
    // Select All handler
    $('#select-all-permissions').off('change').on('change', function() {
        const isChecked = $(this).is(':checked');
        $('.permission-check').prop('checked', isChecked);
        $('.permission-all').prop('checked', isChecked);
    });
    
    // Individual "All" checkbox handler
    $('.permission-all').off('change').on('change', function() {
        const menuId = $(this).data('menu-id');
        const isChecked = $(this).is(':checked');
        const isParent = $(this).data('parent');
        
        // If this is a parent menu checkbox, toggle all children
        if (isParent) {
            // Find all child menus and toggle their checkboxes
            $(this).closest('.mb-4').find('.permission-check, .permission-all').not(this).prop('checked', isChecked);
        } else {
            // Regular menu, just toggle its own checkboxes
            $(`.permission-check[data-menu-id="${menuId}"]`).prop('checked', isChecked);
        }
    });
    
    // Individual permission handler
    $('.permission-check').off('change').on('change', function() {
        const menuId = $(this).data('menu-id');
        const allChecked = $(`.permission-check[data-menu-id="${menuId}"]`).length === 
                          $(`.permission-check[data-menu-id="${menuId}"]:checked`).length;
        $(`.permission-all[data-menu-id="${menuId}"]`).prop('checked', allChecked);
        
        // Update select all
        updateSelectAllState();
    });
}

function updateSelectAllState() {
    const totalChecks = $('.permission-check').length;
    const checkedCount = $('.permission-check:checked').length;
    $('#select-all-permissions').prop('checked', totalChecks === checkedCount && totalChecks > 0);
}

function collectPermissions() {
    const permissions = [];
    const processedMenus = new Set();
    
    // First, collect ALL parent menus (with "All" checkbox)
    $('.parent-menu-checkbox').each(function() {
        const menuId = $(this).data('menu-id');
        const isChecked = $(this).is(':checked');
        
        processedMenus.add(menuId);
        
        permissions.push({
            menuId: menuId,
            canView: isChecked,
            canAdd: isChecked,
            canEdit: isChecked,
            canDelete: isChecked,
            canPost: isChecked,
            canReverse: isChecked
        });
    });
    
    // Then, collect all child menus (with individual checkboxes)
    $('.permission-check').each(function() {
        const menuId = $(this).data('menu-id');
        
        if (!processedMenus.has(menuId)) {
            processedMenus.add(menuId);
            
            const canView = $(`.permission-check[data-menu-id="${menuId}"][data-action="view"]`).is(':checked');
            const canAdd = $(`.permission-check[data-menu-id="${menuId}"][data-action="add"]`).is(':checked');
            const canEdit = $(`.permission-check[data-menu-id="${menuId}"][data-action="edit"]`).is(':checked');
            const canDelete = $(`.permission-check[data-menu-id="${menuId}"][data-action="delete"]`).is(':checked');
            const canPost = $(`.permission-check[data-menu-id="${menuId}"][data-action="post"]`).is(':checked');
            const canReverse = $(`.permission-check[data-menu-id="${menuId}"][data-action="reverse"]`).is(':checked');
            
            permissions.push({
                menuId: menuId,
                canView: canView,
                canAdd: canAdd,
                canEdit: canEdit,
                canDelete: canDelete,
                canPost: canPost,
                canReverse: canReverse
            });
        }
    });
    
    return permissions;
}

function saveRoleWithPermissions() {
    const permissions = collectPermissions();
    
    $.ajax({
        url: '/Role/SaveRoleMenuPermissions',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            roleId: currentRoleIdForPermissions,
            permissions: permissions
        }),
        success: function() {
            // Permissions saved successfully
        },
        error: function() {
            Swal.fire('Error', 'Failed to save permissions', 'error');
        }
    });
}
