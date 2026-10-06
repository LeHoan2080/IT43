document.addEventListener("DOMContentLoaded", function () {
    /* =====================================================
       SIDEBAR
       ===================================================== */

    const sidebarToggle =
        document.getElementById("sidebarToggle");

    const sidebar =
        document.getElementById("erpSidebar");


    if (sidebarToggle && sidebar) {
        const isMobile = window.matchMedia("(max-width: 767.98px)").matches;
        sidebarToggle.setAttribute(
            "aria-expanded",
            String(isMobile
                ? sidebar.classList.contains("collapsed")
                : !sidebar.classList.contains("collapsed")));

        sidebarToggle.addEventListener("click", function () {

            sidebar.classList.toggle("collapsed");
            const isOpen = sidebar.classList.contains("collapsed");
            const isMobile = window.matchMedia("(max-width: 767.98px)").matches;

            document.body.classList.toggle(
                "erp-sidebar-open",
                isMobile && isOpen);
            sidebarToggle.setAttribute(
                "aria-expanded",
                String(isMobile ? isOpen : !isOpen));

        });

    }

    document.addEventListener("click", function (event) {
        if (!sidebar || !sidebarToggle ||
            !document.body.classList.contains("erp-sidebar-open")) {
            return;
        }

        if (!sidebar.contains(event.target) &&
            !sidebarToggle.contains(event.target)) {
            sidebar.classList.remove("collapsed");
            document.body.classList.remove("erp-sidebar-open");
            sidebarToggle.setAttribute("aria-expanded", "false");
        }
    });

    window.addEventListener("resize", function () {
        const isMobile = window.matchMedia("(max-width: 767.98px)").matches;
        if (!isMobile) {
            document.body.classList.remove("erp-sidebar-open");
        }

        const isExpanded = sidebar?.classList.contains("collapsed")
            ? isMobile
            : !isMobile;
        sidebarToggle?.setAttribute("aria-expanded", String(isExpanded));
    });


    /* =====================================================
       INVENTORY SEARCH
       ===================================================== */

    const searchInput =
        document.getElementById("inventorySearch");

    const inventoryRows =
        document.querySelectorAll(".inventory-row");


    if (searchInput && inventoryRows.length > 0) {

        searchInput.addEventListener("input", function () {

            const keyword =
                searchInput.value
                    .trim()
                    .toLowerCase();


            inventoryRows.forEach(function (row) {

                const searchData =
                    row.dataset.search?.toLowerCase() ?? "";


                if (searchData.includes(keyword)) {

                    row.style.display = "";

                }
                else {

                    row.style.display = "none";

                }

            });

        });

    }


    /* =====================================================
       SELECT ALL
       ===================================================== */

    const selectAll =
        document.getElementById("selectAll");

    const rowCheckboxes =
        document.querySelectorAll(".row-checkbox");


    if (selectAll) {

        selectAll.addEventListener("change", function () {

            rowCheckboxes.forEach(function (checkbox) {

                checkbox.checked =
                    selectAll.checked;

            });

        });

    }

});
