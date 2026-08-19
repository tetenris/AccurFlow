// Notification functionality
(function () {
    let notificationBadge = null;
    let notificationPanel = null;

    // Initialize notification system
    function initNotifications() {
        // Add badge to notification icon
        const notificationIcon = document.querySelector('#kt_menu_item_wow');
        if (notificationIcon && !document.querySelector('.notification-badge')) {
            const badge = document.createElement('span');
            badge.className = 'notification-badge position-absolute translate-middle badge rounded-pill bg-danger';
            badge.style.cssText = 'top: 8px; right: 8px; font-size: 10px;';
            badge.textContent = '0';
            badge.style.display = 'none';
            notificationIcon.appendChild(badge);
            notificationBadge = badge;
        }

        // Get notification panel
        notificationPanel = document.querySelector('#kt_topbar_notifications_1 .scroll-y');

        // Load initial notifications
        loadNotifications();
        updateBadgeCount();

        // Poll for new notifications every 30 seconds
        setInterval(updateBadgeCount, 30000);
    }

    // Update badge count
    function updateBadgeCount() {
        fetch('/Notification/GetUnreadCount')
            .then(response => response.json())
            .then(data => {
                if (data.success && notificationBadge) {
                    const count = data.count || 0;
                    notificationBadge.textContent = count;
                    notificationBadge.style.display = count > 0 ? 'block' : 'none';
                }
            })
            .catch(error => console.error('Error fetching unread count:', error));
    }

    // Load notifications
    function loadNotifications() {
        fetch('/Notification/GetNotifications?page=1&size=10')
            .then(response => response.json())
            .then(data => {
                if (data.success && notificationPanel) {
                    renderNotifications(data.data);
                }
            })
            .catch(error => console.error('Error loading notifications:', error));
    }

    // Render notifications
    function renderNotifications(notifications) {
        if (!notificationPanel) return;

        if (!notifications || notifications.length === 0) {
            notificationPanel.innerHTML = `
                <div class="text-center text-gray-600 py-10">
                    <i class="ki-duotone ki-notification-status fs-3x mb-5">
                        <span class="path1"></span>
                        <span class="path2"></span>
                        <span class="path3"></span>
                        <span class="path4"></span>
                    </i>
                    <p>No notifications</p>
                </div>
            `;
            return;
        }

        let html = '';
        notifications.forEach(notification => {
            const isRead = notification.isRead ? 'bg-light-primary' : 'bg-white';
            const readClass = notification.isRead ? 'opacity-75' : '';

            // Get icon based on notification type
            const icon = getNotificationIcon(notification.title);

            html += `
                <div class="d-flex flex-stack py-4 ${isRead} cursor-pointer notification-item" 
                     data-notification-id="${notification.notificationUserId}"
                     data-notification-title="${escapeHtml(notification.title)}"
                     data-notification-message="${escapeHtml(notification.message)}"
                     data-notification-link="${notification.link}"
                     data-notification-date="${notification.createdAt}"
                     onclick="showNotificationDetail(this)">
                    <div class="d-flex align-items-center w-100">
                        <div class="symbol symbol-35px me-4">
                            <span class="symbol-label ${icon.bgClass}">
                                ${icon.svg}
                            </span>
                        </div>
                        <div class="mb-0 me-2 flex-grow-1">
                            <div class="fs-6 text-gray-800 fw-bold ${readClass}">
                                ${notification.title}
                            </div>
                            <div class="text-gray-400 fs-8 ${readClass}">${formatDate(notification.createdAt)}</div>
                        </div>
                    </div>
                </div>
            `;
        });

        notificationPanel.innerHTML = html;
    }

    // Get notification icon based on title
    function getNotificationIcon(title) {
        if (title.includes('Weekly') || title.includes('Check')) {
            return {
                bgClass: 'bg-light-info',
                svg: `<svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M10 2C5.58172 2 2 5.58172 2 10C2 14.4183 5.58172 18 10 18C14.4183 18 18 14.4183 18 10C18 5.58172 14.4183 2 10 2ZM10 16C6.68629 16 4 13.3137 4 10C4 6.68629 6.68629 4 10 4C13.3137 4 16 6.68629 16 10C16 13.3137 13.3137 16 10 16ZM11 10V6H9V11H14V9H11V10Z" fill="#3E97FF"/>
                </svg>`
            };
        } else if (title.includes('Reminder') || title.includes('Pending')) {
            return {
                bgClass: 'bg-light-warning',
                svg: `<svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M10 2C5.58172 2 2 5.58172 2 10C2 14.4183 5.58172 18 10 18C14.4183 18 18 14.4183 18 10C18 5.58172 14.4183 2 10 2ZM10 16C6.68629 16 4 13.3137 4 10C4 6.68629 6.68629 4 10 4C13.3137 4 16 6.68629 16 10C16 13.3137 13.3137 16 10 16ZM11 6H9V11L13.5 13.5L14.5 11.7L11 9.8V6Z" fill="#FFC700"/>
                </svg>`
            };
        } else {
            return {
                bgClass: 'bg-light-primary',
                svg: `<i class="ki-duotone ki-notification-status fs-2 text-primary">
                    <span class="path1"></span>
                    <span class="path2"></span>
                    <span class="path3"></span>
                    <span class="path4"></span>
                </i>`
            };
        }
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Show notification detail in modal
    window.showNotificationDetail = function (element) {
        const notificationId = element.dataset.notificationId;
        const title = element.dataset.notificationTitle;
        const message = element.dataset.notificationMessage;
        const link = element.dataset.notificationLink;
        const date = element.dataset.notificationDate;

        // Mark as read
        markAsRead(notificationId);

        // Show modal
        showNotificationModal(title, message, link, date);
    };

    // Mark notification as read
    function markAsRead(notificationId) {
        fetch('/Notification/MarkAsRead', {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(notificationId)
        })
            .then(() => {
                updateBadgeCount();
                loadNotifications();
            })
            .catch(error => console.error('Error marking as read:', error));
    }

    // Show notification modal
    function showNotificationModal(title, message, link, date) {
        // Create modal if not exists
        let modal = document.getElementById('notificationDetailModal');
        if (!modal) {
            modal = createNotificationModal();
            document.body.appendChild(modal);
        }

        // Update modal content
        document.getElementById('notificationModalTitle').textContent = title;
        document.getElementById('notificationModalDate').textContent = formatFullDate(date);
        document.getElementById('notificationModalMessage').innerHTML = message;

        const viewButton = document.getElementById('notificationModalViewButton');
        if (link && link !== '#' && link !== '') {
            viewButton.style.display = 'inline-block';
            viewButton.onclick = function () {
                window.location.href = link;
            };
        } else {
            viewButton.style.display = 'none';
        }

        // Show modal using Bootstrap
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    // Create notification modal
    function createNotificationModal() {
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.id = 'notificationDetailModal';
        modal.tabIndex = -1;
        modal.setAttribute('aria-labelledby', 'notificationModalLabel');
        modal.setAttribute('aria-hidden', 'true');

        modal.innerHTML = `
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header border-0 pb-0">
                        <h5 class="modal-title fw-bold" id="notificationModalLabel">Notification</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body pt-2">
                        <div class="mb-3">
                            <h6 class="fw-bold mb-2" id="notificationModalTitle"></h6>
                            <p class="text-muted fs-7 mb-3" id="notificationModalDate"></p>
                        </div>
                        <div class="notification-message-content" id="notificationModalMessage"></div>
                    </div>
                    <div class="modal-footer border-0 pt-0">
                        <button type="button" class="btn btn-primary" id="notificationModalViewButton">
                            View Pending
                        </button>
                    </div>
                </div>
            </div>
        `;

        return modal;
    }

    // Format full date
    function formatFullDate(dateString) {
        const date = new Date(dateString);
        const options = { day: '2-digit', month: 'short', year: 'numeric' };
        return 'Date: ' + date.toLocaleDateString('en-GB', options).replace(/ /g, ' ');
    }

    // Format date
    function formatDate(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = Math.floor((now - date) / 1000); // difference in seconds

        if (diff < 60) return 'Just now';
        if (diff < 3600) return Math.floor(diff / 60) + ' minutes ago';
        if (diff < 86400) return Math.floor(diff / 3600) + ' hours ago';
        if (diff < 604800) return Math.floor(diff / 86400) + ' days ago';

        return date.toLocaleDateString();
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initNotifications);
    } else {
        initNotifications();
    }
})();
