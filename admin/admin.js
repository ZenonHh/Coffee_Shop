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
            row.hidden = !matchesQuery || !matchesStatus;
        });
    }

    if (search) {
        search.addEventListener('input', filterRows);
    }
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
    var surveyDialog = document.getElementById('surveyDialog');
    var openSurveyDialog = document.getElementById('openSurveyDialog');
    var saveSurveyPreview = document.getElementById('saveSurveyPreview');

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
    if (surveyDialog && openSurveyDialog) {
        openSurveyDialog.addEventListener('click', function () {
            surveyDialog.showModal();
        });
    }
    if (saveSurveyPreview && surveyDialog) {
        saveSurveyPreview.addEventListener('click', function () {
            showToast('Đây là giao diện mẫu. Khảo sát chưa được tạo.');
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
}());
