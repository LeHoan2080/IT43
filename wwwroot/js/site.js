document.addEventListener("DOMContentLoaded", function () {

    if (sidebarToggle && sidebar) {

        sidebarToggle.addEventListener("click", function () {

            sidebar.classList.toggle("collapsed");

        });

    }

    /* =====================================================
       SIDEBAR
       ===================================================== */

    const sidebarToggle =
        document.getElementById("sidebarToggle");

    const sidebar =
        document.getElementById("erpSidebar");


    if (sidebarToggle && sidebar) {

        sidebarToggle.addEventListener("click", function () {

            sidebar.classList.toggle("collapsed");

        });

    }


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
