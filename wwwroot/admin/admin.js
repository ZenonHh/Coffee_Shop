(function () {
    'use strict';

    var search = document.getElementById('tableSearch');
    var globalSearch = document.querySelector('.global-search');
    var toast = document.getElementById('adminToast');
    var toastTimeout;
    var surveyStatusFilter = document.getElementById('surveyStatusFilter');

    function showToast(message) {
        if (!toast) {
            return;
        }
        toast.textContent = message;
        toast.classList.add('visible');
        window.clearTimeout(toastTimeout);
        toastTimeout = window.setTimeout(function () {
            toast.classList.remove('visible');
        }, 2800);
    }

    function filterRows() {
        var query = search ? search.value.trim().toLocaleLowerCase('vi') : '';
        var status = surveyStatusFilter ? surveyStatusFilter.value : '';

        document.querySelectorAll('[data-search-row]').forEach(function (row) {
            var matchesQuery = row.textContent.toLocaleLowerCase('vi').includes(query);
            var matchesStatus = !status || !row.dataset.status || row.dataset.status === status;
            var table = row.closest('table');
            var localSearch = table && Array.from(document.querySelectorAll('[data-account-search]'))
                .find(function (input) { return input.dataset.accountSearch === table.id; });
            var localQuery = localSearch ? localSearch.value.trim().toLocaleLowerCase('vi') : '';
            var matchesLocalQuery = row.textContent.toLocaleLowerCase('vi').includes(localQuery);
            row.hidden = !matchesQuery || !matchesStatus || !matchesLocalQuery;
        });
    }

    if (search) {
        search.addEventListener('input', filterRows);
    }
    document.querySelectorAll('[data-account-search]').forEach(function (input) {
        input.addEventListener('input', filterRows);
    });
    function focusGlobalSearch() {
        if (!search) {
            return;
        }
        if (window.matchMedia('(max-width: 700px)').matches && globalSearch) {
            globalSearch.classList.add('search-open');
        }
        search.focus();
    }
    if (globalSearch) {
        globalSearch.addEventListener('click', function (event) {
            if (window.matchMedia('(max-width: 700px)').matches && !globalSearch.classList.contains('search-open')) {
                event.preventDefault();
                focusGlobalSearch();
            }
        });
    }
    if (surveyStatusFilter) {
        surveyStatusFilter.addEventListener('change', filterRows);
    }

    document.querySelectorAll('[id$="SearchFocus"]').forEach(function (button) {
        button.addEventListener('click', function () {
            focusGlobalSearch();
        });
    });

    var sidebar = document.getElementById('sidebar');
    var menuToggle = document.getElementById('menuToggle');
    var backdrop = document.getElementById('sidebarBackdrop');

    function closeSidebar() {
        if (sidebar && backdrop && menuToggle) {
            sidebar.classList.remove('open');
            backdrop.classList.remove('visible');
            menuToggle.setAttribute('aria-expanded', 'false');
        }
    }

    if (sidebar && backdrop && menuToggle) {
        menuToggle.addEventListener('click', function () {
            var isOpen = sidebar.classList.toggle('open');
            backdrop.classList.toggle('visible', isOpen);
            menuToggle.setAttribute('aria-expanded', String(isOpen));
        });
        backdrop.addEventListener('click', closeSidebar);
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                closeSidebar();
                if (globalSearch) {
                    globalSearch.classList.remove('search-open');
                }
            }
            if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
                event.preventDefault();
                if (search) {
                    search.focus();
                }
            }
        });
    }

    var productDialog = document.getElementById('productDialog');
    var openProductDialog = document.getElementById('openProductDialog');
    if (productDialog && openProductDialog) {
        openProductDialog.addEventListener('click', function () {
            productDialog.showModal();
        });
    }
    if (productDialog) {
        productDialog.querySelectorAll('.dialog-close, .dialog-cancel').forEach(function (button) {
            button.addEventListener('click', function () {
                productDialog.close();
            });
        });
    }

    ['exportButton', 'customerExportButton'].forEach(function (id) {
        var button = document.getElementById(id);
        if (button) {
            button.addEventListener('click', function () {
                showToast('Chức năng xuất báo cáo sẽ được bổ sung khi kết nối dữ liệu.');
            });
        }
    });

    var dateButton = document.getElementById('dateButton');
    if (dateButton) {
        dateButton.addEventListener('click', function () {
            showToast('Đang hiển thị dữ liệu mẫu ngày 30/09/2026.');
        });
    }

    document.querySelectorAll('.row-more').forEach(function (button) {
        button.addEventListener('click', function () {
            if (button.classList.contains('survey-details-button')) {
                showToast('Chi tiết phản hồi sẽ khả dụng khi kết nối dữ liệu khảo sát.');
                return;
            }
            showToast('Các thao tác quản lý sẽ hoạt động khi kết nối dữ liệu.');
        });
    });

    document.querySelectorAll('.feedback-action').forEach(function (button) {
        button.addEventListener('click', function () {
            showToast('Trạng thái phản hồi sẽ được lưu khi kết nối dữ liệu khảo sát.');
        });
    });

    document.querySelectorAll('.order-id-cell').forEach(function (cell) {
        var preview = cell.querySelector('.order-preview');
        if (!preview) return;
        cell.addEventListener('mouseenter', function () {
            preview.classList.add('is-visible');
            var rect = cell.getBoundingClientRect();
            var width = 330;
            var left = Math.min(Math.max(10, rect.left), window.innerWidth - width - 10);
            var top = rect.bottom + 8;
            if (top + preview.offsetHeight > window.innerHeight - 10) top = Math.max(10, rect.top - preview.offsetHeight - 8);
            preview.style.left = left + 'px';
            preview.style.top = top + 'px';
        });
        cell.addEventListener('mouseleave', function () { preview.classList.remove('is-visible'); });
    });

    var purchaseFilterForm = document.getElementById('purchaseFilterForm');
    if (purchaseFilterForm) {
        var filterMode = document.getElementById('purchaseFilterMode');
        var filterSections = purchaseFilterForm.querySelectorAll('[data-filter-options]');
        var fromDate = document.getElementById('purchaseFromDate');
        var toDate = document.getElementById('purchaseToDate');

        function setFilterMode(mode) {
            filterMode.value = mode;
            purchaseFilterForm.querySelectorAll('[data-filter-mode]').forEach(function (button) {
                button.classList.toggle('active', button.dataset.filterMode === mode);
            });
            filterSections.forEach(function (section) {
                section.hidden = section.dataset.filterOptions !== mode;
            });
        }

        purchaseFilterForm.querySelectorAll('[data-filter-mode]').forEach(function (button) {
            button.addEventListener('click', function () {
                setFilterMode(button.dataset.filterMode);
                if (button.dataset.filterMode === 'all') {
                    purchaseFilterForm.submit();
                }
            });
        });

        purchaseFilterForm.querySelectorAll('[data-purchase-quarter]').forEach(function (button) {
            button.addEventListener('click', function () {
                document.getElementById('purchaseQuarter').value = button.dataset.purchaseQuarter;
                setFilterMode('quarter');
                purchaseFilterForm.submit();
            });
        });
        purchaseFilterForm.querySelectorAll('[data-purchase-months]').forEach(function (button) {
            button.addEventListener('click', function () {
                document.getElementById('purchaseMonths').value = button.dataset.purchaseMonths;
                setFilterMode('months');
                purchaseFilterForm.submit();
            });
        });

        var yearSelect = document.getElementById('purchaseYear');
        if (yearSelect) {
            yearSelect.addEventListener('change', function () {
                setFilterMode('quarter');
                purchaseFilterForm.submit();
            });
        }
        var sortSelect = document.getElementById('purchaseSort');
        if (sortSelect) {
            sortSelect.addEventListener('change', function () {
                purchaseFilterForm.submit();
            });
        }
        purchaseFilterForm.addEventListener('submit', function (event) {
            if (filterMode.value !== 'custom') return;
            var start = fromDate.value;
            var end = toDate.value;
            fromDate.setCustomValidity(!start ? 'Vui lòng chọn ngày bắt đầu.' : '');
            toDate.setCustomValidity(!end ? 'Vui lòng chọn ngày kết thúc.' : '');
            if (start && end && start > end) {
                toDate.setCustomValidity('Đến ngày phải bằng hoặc sau Từ ngày.');
            }
            if (!purchaseFilterForm.checkValidity()) {
                event.preventDefault();
                purchaseFilterForm.reportValidity();
            }
        });

        var detailPanel = document.getElementById('purchaseDetailPanel');
        var purchaseRows = purchaseFilterForm.parentElement.querySelectorAll('.purchase-select-row');
        var activeRequest;

        function loadPurchaseDetail(row) {
            if (activeRequest) activeRequest.abort();
            activeRequest = new AbortController();
            purchaseRows.forEach(function (item) {
                item.classList.toggle('is-selected', item === row);
                item.setAttribute('aria-pressed', String(item === row));
            });
            detailPanel.innerHTML = '<div class="purchase-detail-loading" role="status">Đang tải chi tiết phiếu nhập #' +
                row.dataset.purchaseId + '…</div>';
            fetch(row.dataset.detailsUrl, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                signal: activeRequest.signal
            }).then(function (response) {
                if (!response.ok) throw new Error('Không thể tải chi tiết phiếu nhập.');
                return response.text();
            }).then(function (html) {
                detailPanel.innerHTML = html;
            }).catch(function (error) {
                if (error.name === 'AbortError') return;
                detailPanel.innerHTML = '<div class="purchase-detail-load-error" role="alert">' +
                    error.message + ' Vui lòng chọn lại phiếu hoặc tải lại trang.</div>';
            });
        }

        purchaseRows.forEach(function (row) {
            row.addEventListener('click', function () { loadPurchaseDetail(row); });
            row.addEventListener('keydown', function (event) {
                if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault();
                    loadPurchaseDetail(row);
                }
            });
        });
        if (purchaseRows.length) loadPurchaseDetail(purchaseRows[0]);
    }
}());
