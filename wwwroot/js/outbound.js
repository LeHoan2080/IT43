document.addEventListener("DOMContentLoaded", function () {

    const tableBody =
        document.getElementById("outboundLinesBody");

    const btnAddLine =
        document.getElementById("btnAddLine");

    const emptyMessage =
        document.getElementById("outboundEmptyMessage");

    if (!tableBody) {
        return;
    }


    // =========================================================
    // CSRF
    // =========================================================

    function getAntiForgeryToken() {

        const input =
            document.querySelector(
                'input[name="__RequestVerificationToken"]'
            );

        return input ? input.value : "";
    }


    // =========================================================
    // UPDATE EMPTY MESSAGE
    // =========================================================

    function updateEmptyMessage() {

        if (!emptyMessage) {
            return;
        }

        const rows =
            tableBody.querySelectorAll(".outbound-line");

        emptyMessage.style.display =
            rows.length === 0
                ? "block"
                : "none";
    }


    // =========================================================
    // REINDEX
    // =========================================================

    function reindexLines() {

        const rows =
            tableBody.querySelectorAll(".outbound-line");

        rows.forEach(function (row, index) {

            row.querySelectorAll(
                "input[name], select[name], textarea[name]"
            ).forEach(function (element) {

                const name =
                    element.getAttribute("name");

                if (!name) {
                    return;
                }

                const newName =
                    name.replace(
                        /Lines\[\d+\]/,
                        "Lines[" + index + "]"
                    );

                element.setAttribute(
                    "name",
                    newName
                );
            });

        });
    }


    // =========================================================
    // ADD LINE
    // =========================================================

    function addLine() {

        const index =
            tableBody.querySelectorAll(
                ".outbound-line"
            ).length;

        const row =
            document.createElement("tr");

        row.className =
            "outbound-line";

        row.innerHTML = `

            <td>

                <input type="hidden"
                       name="Lines[${index}].Id"
                       value="0" />

                <input type="hidden"
                       name="Lines[${index}].ProductId"
                       class="line-product-id"
                       value="0" />

                <div class="input-group">

                    <input type="text"
                           class="form-control product-search"
                           placeholder="Barcode / mã / ISBN / tên"
                           autocomplete="off" />

                    <button type="button"
                            class="btn btn-outline-secondary btn-scan-product">

                        <i class="bi bi-upc-scan"></i>

                    </button>

                </div>

            </td>


            <td>

                <span class="line-product-barcode">
                    -
                </span>

            </td>


            <td>

                <input type="hidden"
                       name="Lines[${index}].LocationId"
                       class="line-location-id"
                       value="0" />

                <div class="input-group">

                    <input type="text"
                           class="form-control location-search"
                           placeholder="Barcode / mã Bin"
                           autocomplete="off" />

                    <button type="button"
                            class="btn btn-outline-secondary btn-scan-location">

                        <i class="bi bi-upc-scan"></i>

                    </button>

                </div>

            </td>


            <td class="text-end">

                <span class="line-bin-stock">
                    0
                </span>

            </td>


            <td>

                <input type="number"
                       name="Lines[${index}].RequestedQty"
                       class="form-control text-end line-requested-qty"
                       value="1"
                       min="1" />

            </td>


            <td>

                <input type="number"
                       name="Lines[${index}].PickedQty"
                       class="form-control text-end line-picked-qty"
                       value="0"
                       min="0"
                       disabled />

            </td>


            <td class="text-center">

                <button type="button"
                        class="btn btn-sm btn-outline-danger btn-remove-line">

                    <i class="bi bi-trash"></i>

                </button>

            </td>

        `;

        tableBody.appendChild(row);

        updateEmptyMessage();

        bindRowEvents(row);
    }


    // =========================================================
    // REMOVE LINE
    // =========================================================

    function removeLine(row) {

        if (!row) {
            return;
        }

        row.remove();

        reindexLines();

        updateEmptyMessage();
    }


    // =========================================================
    // SEARCH PRODUCT
    // =========================================================

    async function searchProduct(input) {

        const keyword =
            input.value.trim();

        if (keyword.length < 1) {
            hideDropdowns();
            return;
        }

        const row =
            input.closest(".outbound-line");

        if (!row) {
            return;
        }

        try {

            const response =
                await fetch(
                    "/Outbound/SearchProduct?keyword="
                    + encodeURIComponent(keyword)
                );

            if (!response.ok) {
                return;
            }

            const data =
                await response.json();

            showProductDropdown(
                input,
                row,
                data
            );

        }
        catch (error) {

            console.error(
                "Product search error:",
                error
            );

        }
    }


    // =========================================================
    // PRODUCT DROPDOWN
    // =========================================================

    function showProductDropdown(
        input,
        row,
        products
    ) {

        hideDropdowns();

        if (!products || products.length === 0) {
            return;
        }

        const dropdown =
            document.createElement("div");

        dropdown.className =
            "outbound-search-dropdown";

        products.forEach(function (product) {

            const item =
                document.createElement("button");

            item.type = "button";

            item.className =
                "list-group-item list-group-item-action";

            item.innerHTML = `

                <div class="fw-semibold">
                    ${escapeHtml(product.productCode)}
                    - ${escapeHtml(product.name)}
                </div>

                <small class="text-muted">

                    Barcode:
                    ${escapeHtml(product.barcode || "-")}

                    &nbsp; | &nbsp;

                    ISBN:
                    ${escapeHtml(product.isbn || "-")}

                </small>

            `;

            item.addEventListener(
                "click",
                function () {

                    selectProduct(
                        row,
                        product
                    );

                    dropdown.remove();

                }
            );

            dropdown.appendChild(item);

        });


        positionDropdown(
            input,
            dropdown
        );

        document.body.appendChild(
            dropdown
        );
    }


    // =========================================================
    // SELECT PRODUCT
    // =========================================================

    function selectProduct(
        row,
        product
    ) {

        const productId =
            row.querySelector(
                ".line-product-id"
            );

        const productInput =
            row.querySelector(
                ".product-search"
            );

        const barcode =
            row.querySelector(
                ".line-product-barcode"
            );

        const locationInput =
            row.querySelector(
                ".location-search"
            );

        const locationId =
            row.querySelector(
                ".line-location-id"
            );

        const stock =
            row.querySelector(
                ".line-bin-stock"
            );


        productId.value =
            product.id;

        productInput.value =
            product.productCode
            + " - "
            + product.name;

        barcode.textContent =
            product.barcode || "-";


        // reset bin
        locationId.value = "0";

        locationInput.value = "";

        stock.textContent = "0";
    }


    // =========================================================
    // SEARCH LOCATION
    // =========================================================

    async function searchLocation(input) {

        const keyword =
            input.value.trim();

        const row =
            input.closest(".outbound-line");

        if (!row) {
            return;
        }

        const productId =
            row.querySelector(
                ".line-product-id"
            ).value;

        const warehouse =
            document.querySelector(
                '[name="WarehouseId"]'
            );

        const warehouseId =
            warehouse
                ? warehouse.value
                : "";

        if (!warehouseId ||
            !productId ||
            productId === "0") {

            return;
        }

        if (keyword.length < 1) {
            hideDropdowns();
            return;
        }


        try {

            const response =
                await fetch(
                    "/Outbound/SearchLocation"
                    + "?warehouseId="
                    + encodeURIComponent(warehouseId)
                    + "&productId="
                    + encodeURIComponent(productId)
                    + "&keyword="
                    + encodeURIComponent(keyword)
                );

            if (!response.ok) {
                return;
            }

            const data =
                await response.json();

            showLocationDropdown(
                input,
                row,
                data
            );

        }
        catch (error) {

            console.error(
                "Location search error:",
                error
            );

        }
    }


    // =========================================================
    // LOCATION DROPDOWN
    // =========================================================

    function showLocationDropdown(
        input,
        row,
        locations
    ) {

        hideDropdowns();

        if (!locations || locations.length === 0) {
            return;
        }

        const dropdown =
            document.createElement("div");

        dropdown.className =
            "outbound-search-dropdown";


        locations.forEach(function (location) {

            const item =
                document.createElement("button");

            item.type = "button";

            item.className =
                "list-group-item list-group-item-action";

            item.innerHTML = `

                <div class="fw-semibold">

                    ${escapeHtml(location.code)}
                    -
                    ${escapeHtml(location.name)}

                </div>

                <small class="text-muted">

                    Barcode:
                    ${escapeHtml(location.barcode || "-")}

                    &nbsp; | &nbsp;

                    Tồn:
                    ${location.stockQuantity}

                </small>

            `;

            item.addEventListener(
                "click",
                function () {

                    selectLocation(
                        row,
                        location
                    );

                    dropdown.remove();

                }
            );

            dropdown.appendChild(item);

        });


        positionDropdown(
            input,
            dropdown
        );

        document.body.appendChild(
            dropdown
        );
    }


    // =========================================================
    // SELECT LOCATION
    // =========================================================

    function selectLocation(
        row,
        location
    ) {

        const locationId =
            row.querySelector(
                ".line-location-id"
            );

        const locationInput =
            row.querySelector(
                ".location-search"
            );

        const stock =
            row.querySelector(
                ".line-bin-stock"
            );

        locationId.value =
            location.id;

        locationInput.value =
            location.code
            + " - "
            + location.name;

        stock.textContent =
            location.stockQuantity;
    }


    // =========================================================
    // DROPDOWN POSITION
    // =========================================================

    function positionDropdown(
        input,
        dropdown
    ) {

        dropdown.style.position =
            "absolute";

        dropdown.style.zIndex =
            "9999";

        dropdown.style.width =
            input.offsetWidth + "px";

        const rect =
            input.getBoundingClientRect();

        dropdown.style.left =
            (
                rect.left
                + window.scrollX
            ) + "px";

        dropdown.style.top =
            (
                rect.bottom
                + window.scrollY
            ) + "px";

    }


    // =========================================================
    // HIDE DROPDOWNS
    // =========================================================

    function hideDropdowns() {

        document
            .querySelectorAll(
                ".outbound-search-dropdown"
            )
            .forEach(function (element) {

                element.remove();

            });
    }


    // =========================================================
    // BIND ROW
    // =========================================================

    function bindRowEvents(row) {

        const removeButton =
            row.querySelector(
                ".btn-remove-line"
            );

        if (removeButton) {

            removeButton.addEventListener(
                "click",
                function () {

                    removeLine(row);

                }
            );

        }


        const productInput =
            row.querySelector(
                ".product-search"
            );

        if (productInput) {

            let timer = null;

            productInput.addEventListener(
                "input",
                function () {

                    clearTimeout(timer);

                    timer =
                        setTimeout(
                            function () {

                                searchProduct(
                                    productInput
                                );

                            },
                            250
                        );

                }
            );

        }


        const locationInput =
            row.querySelector(
                ".location-search"
            );

        if (locationInput) {

            let timer = null;

            locationInput.addEventListener(
                "input",
                function () {

                    clearTimeout(timer);

                    timer =
                        setTimeout(
                            function () {

                                searchLocation(
                                    locationInput
                                );

                            },
                            250
                        );

                }
            );

        }


        const productScanButton =
            row.querySelector(
                ".btn-scan-product"
            );

        if (productScanButton) {

            productScanButton.addEventListener(
                "click",
                function () {

                    const input =
                        row.querySelector(
                            ".product-search"
                        );

                    if (input) {

                        input.focus();

                        input.select();

                    }

                }
            );

        }


        const locationScanButton =
            row.querySelector(
                ".btn-scan-location"
            );

        if (locationScanButton) {

            locationScanButton.addEventListener(
                "click",
                function () {

                    const input =
                        row.querySelector(
                            ".location-search"
                        );

                    if (input) {

                        input.focus();

                        input.select();

                    }

                }
            );

        }

    }


    // =========================================================
    // INITIALIZE EXISTING ROWS
    // =========================================================

    tableBody
        .querySelectorAll(
            ".outbound-line"
        )
        .forEach(function (row) {

            bindRowEvents(row);

        });


    // =========================================================
    // ADD BUTTON
    // =========================================================

    if (btnAddLine) {

        btnAddLine.addEventListener(
            "click",
            function () {

                addLine();

            }
        );

    }


    // =========================================================
    // START PICKING
    // =========================================================

    const btnStartPicking =
        document.getElementById(
            "btnStartPicking"
        );

    if (btnStartPicking) {

        btnStartPicking.addEventListener(
            "click",
            async function () {

                const id =
                    btnStartPicking.dataset.id;

                if (!id) {
                    return;
                }

                if (!confirm(
                    "Bạn có chắc muốn bắt đầu lấy hàng?"
                )) {
                    return;
                }

                await postAction(
                    "/Outbound/StartPicking",
                    id
                );

            }
        );

    }


    // =========================================================
    // COMPLETE
    // =========================================================

    const btnComplete =
        document.getElementById(
            "btnCompleteOutbound"
        );

    if (btnComplete) {

        btnComplete.addEventListener(
            "click",
            async function () {

                const id =
                    btnComplete.dataset.id;

                if (!id) {
                    return;
                }


                if (!validatePickedQuantities()) {
                    return;
                }


                if (!confirm(
                    "Bạn có chắc muốn hoàn tất phiếu xuất?"
                )) {
                    return;
                }

                await postAction(
                    "/Outbound/Complete",
                    id
                );

            }
        );

    }


    // =========================================================
    // CANCEL
    // =========================================================

    const btnCancel =
        document.getElementById(
            "btnCancelOutbound"
        );

    if (btnCancel) {

        btnCancel.addEventListener(
            "click",
            async function () {

                const id =
                    btnCancel.dataset.id;

                if (!id) {
                    return;
                }


                if (!confirm(
                    "Bạn có chắc muốn hủy phiếu xuất?"
                )) {
                    return;
                }

                await postAction(
                    "/Outbound/Cancel",
                    id
                );

            }
        );

    }


    // =========================================================
    // VALIDATE PICKED QUANTITY
    // =========================================================

    function validatePickedQuantities() {

        const rows =
            tableBody.querySelectorAll(
                ".outbound-line"
            );

        if (rows.length === 0) {

            alert(
                "Phiếu xuất phải có ít nhất một sản phẩm."
            );

            return false;
        }


        for (const row of rows) {

            const productId =
                Number(
                    row.querySelector(
                        ".line-product-id"
                    )?.value || 0
                );

            const locationId =
                Number(
                    row.querySelector(
                        ".line-location-id"
                    )?.value || 0
                );

            const requested =
                Number(
                    row.querySelector(
                        ".line-requested-qty"
                    )?.value || 0
                );

            const picked =
                Number(
                    row.querySelector(
                        ".line-picked-qty"
                    )?.value || 0
                );

            const stock =
                Number(
                    row.querySelector(
                        ".line-bin-stock"
                    )?.textContent || 0
                );


            if (productId <= 0) {

                alert(
                    "Vui lòng chọn sản phẩm."
                );

                return false;
            }


            if (locationId <= 0) {

                alert(
                    "Vui lòng chọn vị trí Bin."
                );

                return false;
            }


            if (requested <= 0) {

                alert(
                    "Số lượng yêu cầu phải lớn hơn 0."
                );

                return false;
            }


            if (picked < 0) {

                alert(
                    "Số lượng đã lấy không được âm."
                );

                return false;
            }


            if (picked > requested) {

                alert(
                    "Số lượng đã lấy không được lớn hơn số lượng yêu cầu."
                );

                return false;
            }


            if (picked > stock) {

                alert(
                    "Số lượng tồn không đủ."
                );

                return false;
            }

        }


        return true;
    }


    // =========================================================
    // POST ACTION
    // =========================================================

    async function postAction(
        url,
        id
    ) {

        try {

            const formData =
                new FormData();

            formData.append(
                "id",
                id
            );

            formData.append(
                "__RequestVerificationToken",
                getAntiForgeryToken()
            );


            const response =
                await fetch(
                    url,
                    {
                        method: "POST",
                        body: formData
                    }
                );


            if (response.redirected) {

                window.location.href =
                    response.url;

                return;
            }


            if (!response.ok) {

                const text =
                    await response.text();

                alert(
                    text || "Thao tác thất bại."
                );

                return;
            }


            window.location.reload();

        }
        catch (error) {

            console.error(error);

            alert(
                "Không thể thực hiện thao tác."
            );

        }

    }


    // =========================================================
    // ESCAPE HTML
    // =========================================================

    function escapeHtml(value) {

        if (value === null ||
            value === undefined) {

            return "";
        }

        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }


    // =========================================================
    // CLOSE DROPDOWN
    // =========================================================

    document.addEventListener(
        "click",
        function (event) {

            if (!event.target.closest(
                ".product-search"
            ) &&
            !event.target.closest(
                ".location-search"
            ) &&
            !event.target.closest(
                ".outbound-search-dropdown"
            )) {

                hideDropdowns();

            }

        }
    );

});