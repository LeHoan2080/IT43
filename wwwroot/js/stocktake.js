document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("stocktakeForm");
    const warehouseSelect = document.getElementById("warehouseId");
    const linesContainer = document.getElementById("stocktakeLines");
    const addLineButton = document.getElementById("btnAddLine");
    const emptyMessage = document.getElementById("emptyLinesMessage");

    if (!form || !linesContainer) {
        return;
    }

    // =========================================================
    // CONSTANTS
    // =========================================================

    const productSearchUrl = "/Stocktake/SearchProduct";
    const locationSearchUrl = "/Stocktake/SearchLocation";

    let searchTimer = null;


    // =========================================================
    // HELPERS
    // =========================================================

    function getWarehouseId() {
        if (!warehouseSelect) {
            return 0;
        }

        return parseInt(warehouseSelect.value || "0");
    }


    function escapeHtml(value) {
        if (value === null || value === undefined) {
            return "";
        }

        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }


    function updateEmptyMessage() {
        if (!emptyMessage) {
            return;
        }

        const rows =
            linesContainer.querySelectorAll(".stocktake-line");

        if (rows.length === 0) {
            emptyMessage.classList.remove("d-none");
        } else {
            emptyMessage.classList.add("d-none");
        }
    }


    function reindexLines() {

        const rows =
            linesContainer.querySelectorAll(".stocktake-line");

        rows.forEach(function (row, index) {

            const inputs =
                row.querySelectorAll(
                    "input[name], textarea[name]"
                );

            inputs.forEach(function (input) {

                const name =
                    input.getAttribute("name");

                if (!name) {
                    return;
                }

                const newName =
                    name.replace(
                        /Lines\[\d+\]/g,
                        `Lines[${index}]`
                    );

                input.setAttribute(
                    "name",
                    newName
                );
            });

        });
    }


    function calculateDifference(row) {

        const systemInput =
            row.querySelector(".system-qty");

        const countedInput =
            row.querySelector(".counted-qty");

        const differenceInput =
            row.querySelector(".difference-qty");

        if (!systemInput ||
            !countedInput ||
            !differenceInput) {
            return;
        }

        const systemQty =
            parseInt(systemInput.value || "0");

        const countedQty =
            parseInt(countedInput.value || "0");

        const difference =
            countedQty - systemQty;

        differenceInput.value =
            difference;
    }


    function updateRowProductDisplay(
        row,
        product
    ) {

        const productInput =
            row.querySelector(".product-search");

        const productIdInput =
            row.querySelector(".product-id");

        if (!productInput ||
            !productIdInput) {
            return;
        }

        productIdInput.value =
            product.id;

        productInput.value =
            `${product.productCode} - ${product.name}`;

        productInput.dataset.productId =
            product.id;

        productInput.dataset.barcode =
            product.barcode || "";

        productInput.dataset.isbn =
            product.isbn || "";

        const barcodeText =
            row.querySelector(
                ".product-barcode-text"
            );

        if (barcodeText) {
            barcodeText.textContent =
                product.barcode || "-";
        }
    }


    function updateRowLocationDisplay(
        row,
        location
    ) {

        const locationInput =
            row.querySelector(".location-search");

        const locationIdInput =
            row.querySelector(".location-id");

        if (!locationInput ||
            !locationIdInput) {
            return;
        }

        locationIdInput.value =
            location.id;

        locationInput.value =
            `${location.code} - ${location.name}`;

        locationInput.dataset.locationId =
            location.id;

        locationInput.dataset.barcode =
            location.barcode || "";

        const barcodeText =
            row.querySelector(
                ".location-barcode-text"
            );

        if (barcodeText) {
            barcodeText.textContent =
                location.barcode || "-";
        }
    }


    function setSystemQuantity(
        row,
        quantity
    ) {

        const systemInput =
            row.querySelector(".system-qty");

        const countedInput =
            row.querySelector(".counted-qty");

        if (!systemInput) {
            return;
        }

        systemInput.value =
            Number.isFinite(quantity)
                ? quantity
                : 0;

        if (countedInput &&
            !countedInput.dataset.userEdited) {

            countedInput.value = "0";
        }

        calculateDifference(row);
    }


    // =========================================================
    // ADD LINE
    // =========================================================

    function createLineRow() {

        const row =
            document.createElement("tr");

        row.className =
            "stocktake-line";

        row.innerHTML = `
            <input type="hidden"
                   name="Lines[0].Id"
                   value="0" />

            <input type="hidden"
                   name="Lines[0].ProductId"
                   class="product-id"
                   value="0" />

            <input type="hidden"
                   name="Lines[0].LocationId"
                   class="location-id"
                   value="0" />

            <td>

                <input type="text"
                       class="form-control product-search"
                       placeholder="Barcode / mã / tên / ISBN..."
                       autocomplete="off" />

                <div class="search-results product-results"></div>

                <div class="small text-muted mt-1">
                    Barcode:
                    <span class="product-barcode-text">-</span>
                </div>

            </td>

            <td>

                <input type="text"
                       class="form-control location-search"
                       placeholder="Barcode / mã Bin..."
                       autocomplete="off" />

                <div class="search-results location-results"></div>

                <div class="small text-muted mt-1">
                    Barcode:
                    <span class="location-barcode-text">-</span>
                </div>

            </td>

            <td>

                <input type="number"
                       class="form-control text-end system-qty"
                       name="Lines[0].SystemQty"
                       value="0"
                       readonly />

            </td>

            <td>

                <input type="number"
                       class="form-control text-end counted-qty"
                       name="Lines[0].CountedQty"
                       value="0"
                       min="0" />

            </td>

            <td>

                <input type="number"
                       class="form-control text-end difference-qty"
                       value="0"
                       readonly />

            </td>

            <td>

                <input type="text"
                       class="form-control"
                       name="Lines[0].Note"
                       maxlength="500"
                       placeholder="Ghi chú..." />

            </td>

            <td class="text-center">

                <button type="button"
                        class="btn btn-sm btn-outline-danger btn-remove-line">

                    <i class="bi bi-trash"></i>

                </button>

            </td>
        `;

        linesContainer.appendChild(row);

        reindexLines();

        updateEmptyMessage();

        const productInput =
            row.querySelector(".product-search");

        if (productInput) {
            productInput.focus();
        }

        return row;
    }


    if (addLineButton) {

        addLineButton.addEventListener(
            "click",
            function () {

                createLineRow();

            }
        );
    }


    // =========================================================
    // REMOVE LINE
    // =========================================================

    linesContainer.addEventListener(
        "click",
        function (event) {

            const removeButton =
                event.target.closest(
                    ".btn-remove-line"
                );

            if (!removeButton) {
                return;
            }

            const row =
                removeButton.closest(
                    ".stocktake-line"
                );

            if (!row) {
                return;
            }

            row.remove();

            reindexLines();

            updateEmptyMessage();
        }
    );


    // =========================================================
    // COUNTED QUANTITY
    // =========================================================

    linesContainer.addEventListener(
        "input",
        function (event) {

            if (!event.target.classList.contains(
                    "counted-qty")) {
                return;
            }

            const input =
                event.target;

            input.dataset.userEdited =
                "true";

            const row =
                input.closest(
                    ".stocktake-line"
                );

            if (row) {
                calculateDifference(row);
            }
        }
    );


    // =========================================================
    // PRODUCT SEARCH
    // =========================================================

    linesContainer.addEventListener(
        "input",
        function (event) {

            const input =
                event.target;

            if (!input.classList.contains(
                    "product-search")) {
                return;
            }

            const row =
                input.closest(
                    ".stocktake-line"
                );

            if (!row) {
                return;
            }

            clearTimeout(searchTimer);

            const keyword =
                input.value.trim();

            if (keyword.length < 1) {

                hideSearchResults(
                    row,
                    ".product-results"
                );

                return;
            }

            searchTimer =
                setTimeout(
                    function () {

                        searchProducts(
                            row,
                            keyword
                        );

                    },
                    250
                );
        }
    );


    async function searchProducts(
        row,
        keyword
    ) {

        const results =
            row.querySelector(
                ".product-results"
            );

        if (!results) {
            return;
        }

        try {

            const response =
                await fetch(
                    `${productSearchUrl}?keyword=${encodeURIComponent(keyword)}`
                );

            if (!response.ok) {
                throw new Error(
                    "Không thể tìm kiếm sản phẩm."
                );
            }

            const products =
                await response.json();

            renderProductResults(
                row,
                products
            );

        }
        catch (error) {

            console.error(error);

            results.innerHTML = `
                <div class="search-result-empty">
                    Không thể tìm kiếm sản phẩm.
                </div>
            `;

            results.style.display =
                "block";
        }
    }


    function renderProductResults(
        row,
        products
    ) {

        const results =
            row.querySelector(
                ".product-results"
            );

        if (!results) {
            return;
        }

        if (!products ||
            products.length === 0) {

            results.innerHTML = `
                <div class="search-result-empty">
                    Không tìm thấy sản phẩm
                </div>
            `;

            results.style.display =
                "block";

            return;
        }

        results.innerHTML =
            products.map(function (product) {

                return `
                    <button type="button"
                            class="search-result-item product-result-item"
                            data-id="${product.id}">

                        <div class="fw-semibold">
                            ${escapeHtml(product.productCode)}
                            -
                            ${escapeHtml(product.name)}
                        </div>

                        <div class="small text-muted">

                            Barcode:
                            ${escapeHtml(product.barcode || "-")}

                            &nbsp; | &nbsp;

                            ISBN:
                            ${escapeHtml(product.isbn || "-")}

                        </div>

                    </button>
                `;

            }).join("");

        results.style.display =
            "block";
    }


    // =========================================================
    // SELECT PRODUCT
    // =========================================================

    linesContainer.addEventListener(
        "click",
        async function (event) {

            const resultButton =
                event.target.closest(
                    ".product-result-item"
                );

            if (!resultButton) {
                return;
            }

            event.preventDefault();

            const row =
                resultButton.closest(
                    ".stocktake-line"
                );

            if (!row) {
                return;
            }

            const productId =
                parseInt(
                    resultButton.dataset.id
                );

            const productInput =
                row.querySelector(
                    ".product-search"
                );

            if (!productInput) {
                return;
            }

            try {

                const response =
                    await fetch(
                        `${productSearchUrl}?keyword=${encodeURIComponent(productInput.value)}`
                    );

                if (!response.ok) {
                    throw new Error();
                }

                const products =
                    await response.json();

                const product =
                    products.find(
                        x => x.id === productId
                    );

                if (!product) {
                    return;
                }

                updateRowProductDisplay(
                    row,
                    product
                );

                hideSearchResults(
                    row,
                    ".product-results"
                );

                await refreshSystemQuantity(
                    row
                );

                const locationInput =
                    row.querySelector(
                        ".location-search"
                    );

                if (locationInput) {
                    locationInput.focus();
                }

            }
            catch (error) {

                console.error(error);

            }
        }
    );


    // =========================================================
    // PRODUCT SCANNER
    // =========================================================

    linesContainer.addEventListener(
        "keydown",
        async function (event) {

            if (event.key !== "Enter") {
                return;
            }

            const input =
                event.target;

            if (!input.classList.contains(
                    "product-search")) {
                return;
            }

            event.preventDefault();

            const keyword =
                input.value.trim();

            if (!keyword) {
                return;
            }

            const row =
                input.closest(
                    ".stocktake-line"
                );

            if (!row) {
                return;
            }

            try {

                const response =
                    await fetch(
                        `${productSearchUrl}?keyword=${encodeURIComponent(keyword)}`
                    );

                if (!response.ok) {
                    throw new Error();
                }

                const products =
                    await response.json();

                if (!products ||
                    products.length === 0) {

                    showInlineError(
                        row,
                        ".product-search",
                        "Không tìm thấy sản phẩm"
                    );

                    return;
                }

                /*
                 * Scanner barcode thường trả về đúng 1 sản phẩm.
                 * Nếu có nhiều kết quả, lấy kết quả khớp
                 * chính xác Barcode/ProductCode/ISBN trước.
                 */

                const exact =
                    products.find(function (product) {

                        return (
                            product.barcode === keyword ||
                            product.productCode === keyword ||
                            product.isbn === keyword
                        );

                    });

                const product =
                    exact || products[0];

                updateRowProductDisplay(
                    row,
                    product
                );

                hideSearchResults(
                    row,
                    ".product-results"
                );

                await refreshSystemQuantity(
                    row
                );

                const locationInput =
                    row.querySelector(
                        ".location-search"
                    );

                if (locationInput) {
                    locationInput.focus();
                }

            }
            catch (error) {

                console.error(error);

                showInlineError(
                    row,
                    ".product-search",
                    "Không thể tìm sản phẩm"
                );
            }
        }
    );


    // =========================================================
    // LOCATION SEARCH
    // =========================================================

    linesContainer.addEventListener(
        "input",
        function (event) {

            const input =
                event.target;

            if (!input.classList.contains(
                    "location-search")) {
                return;
            }

            const row =
                input.closest(
                    ".stocktake-line"
                );

            if (!row) {
                return;
            }

            clearTimeout(searchTimer);

            const keyword =
                input.value.trim();

            if (keyword.length < 1) {

                hideSearchResults(
                    row,
                    ".location-results"
                );

                return;
            }

            searchTimer =
                setTimeout(
                    function () {

                        searchLocations(
                            row,
                            keyword
                        );

                    },
                    250
                );
        }
    );


    async function searchLocations(
        row,
        keyword
    ) {

        const warehouseId =
            getWarehouseId();

        const results =
            row.querySelector(
                ".location-results"
            );

        if (!results) {
            return;
        }

        if (warehouseId <= 0) {

            results.innerHTML = `
                <div class="search-result-empty">
                    Vui lòng chọn kho trước.
                </div>
            `;

            results.style.display =
                "block";

            return;
        }

        try {

            const response =
                await fetch(
                    `${locationSearchUrl}?warehouseId=${warehouseId}&keyword=${encodeURIComponent(keyword)}`
                );

            if (!response.ok) {
                throw new Error();
            }

            const locations =
                await response.json();

            renderLocationResults(
                row,
                locations
            );

        }
        catch (error) {

            console.error(error);

            results.innerHTML = `
                <div class="search-result-empty">
                    Không thể tìm kiếm vị trí.
                </div>
            `;

            results.style.display =
                "block";
        }
    }


    function renderLocationResults(
        row,
        locations
    ) {

        const results =
            row.querySelector(
                ".location-results"
            );

        if (!results) {
            return;
        }

        if (!locations ||
            locations.length === 0) {

            results.innerHTML = `
                <div class="search-result-empty">
                    Không tìm thấy vị trí
                </div>
            `;

            results.style.display =
                "block";

            return;
        }

        results.innerHTML =
            locations.map(function (location) {

                return `
                    <button type="button"
                            class="search-result-item location-result-item"
                            data-id="${location.id}">

                        <div class="fw-semibold">

                            ${escapeHtml(location.code)}
                            -
                            ${escapeHtml(location.name)}

                        </div>

                        <div class="small text-muted">

                            Barcode:
                            ${escapeHtml(location.barcode || "-")}

                            &nbsp; | &nbsp;

                            Tồn:
                            ${Number(location.stockQuantity || 0).toLocaleString("vi-VN")}

                        </div>

                    </button>
                `;

            }).join("");

        results.style.display =
            "block";
    }


    // =========================================================
    // SELECT LOCATION
    // =========================================================

    linesContainer.addEventListener(
        "click",
        async function (event) {

            const resultButton =
                event.target.closest(
                    ".location-result-item"
                );

            if (!resultButton) {
                return;
            }

            event.preventDefault();

            const row =
                resultButton.closest(
                    ".stocktake-line"
                );

            if (!row) {
                return;
            }

            const locationId =
                parseInt(
                    resultButton.dataset.id
                );

            const locationInput =
                row.querySelector(
                    ".location-search"
                );

            if (!locationInput) {
                return;
            }

            const keyword =
                locationInput.value.trim();

            try {

                const response =
                    await fetch(
                        `${locationSearchUrl}?warehouseId=${getWarehouseId()}&keyword=${encodeURIComponent(keyword)}`
                    );

                if (!response.ok) {
                    throw new Error();
                }

                const locations =
                    await response.json();

                const location =
                    locations.find(
                        x => x.id === locationId
                    );

                if (!location) {
                    return;
                }

                updateRowLocationDisplay(
                    row,
                    location
                );

                hideSearchResults(
                    row,
                    ".location-results"
                );

                await refreshSystemQuantity(
                    row
                );

                const countedInput =
                    row.querySelector(
                        ".counted-qty"
                    );

                if (countedInput) {
                    countedInput.focus();
                    countedInput.select();
                }

            }
            catch (error) {

                console.error(error);

            }
        }
    );


    // =========================================================
    // LOCATION SCANNER
    // =========================================================

    linesContainer.addEventListener(
        "keydown",
        async function (event) {

            if (event.key !== "Enter") {
                return;
            }

            const input =
                event.target;

            if (!input.classList.contains(
                    "location-search")) {
                return;
            }

            event.preventDefault();

            const keyword =
                input.value.trim();

            if (!keyword) {
                return;
            }

            const row =
                input.closest(
                    ".stocktake-line"
                );

            if (!row) {
                return;
            }

            const warehouseId =
                getWarehouseId();

            if (warehouseId <= 0) {

                showInlineError(
                    row,
                    ".location-search",
                    "Vui lòng chọn kho trước"
                );

                return;
            }

            try {

                const response =
                    await fetch(
                        `${locationSearchUrl}?warehouseId=${warehouseId}&keyword=${encodeURIComponent(keyword)}`
                    );

                if (!response.ok) {
                    throw new Error();
                }

                const locations =
                    await response.json();

                if (!locations ||
                    locations.length === 0) {

                    showInlineError(
                        row,
                        ".location-search",
                        "Không tìm thấy vị trí"
                    );

                    return;
                }

                const exact =
                    locations.find(function (location) {

                        return (
                            location.barcode === keyword ||
                            location.code === keyword
                        );

                    });

                const location =
                    exact || locations[0];

                updateRowLocationDisplay(
                    row,
                    location
                );

                hideSearchResults(
                    row,
                    ".location-results"
                );

                await refreshSystemQuantity(
                    row
                );

                const countedInput =
                    row.querySelector(
                        ".counted-qty"
                    );

                if (countedInput) {
                    countedInput.focus();
                    countedInput.select();
                }

            }
            catch (error) {

                console.error(error);

                showInlineError(
                    row,
                    ".location-search",
                    "Không thể tìm vị trí"
                );
            }
        }
    );


    // =========================================================
    // REFRESH SYSTEM QUANTITY
    // =========================================================

    async function refreshSystemQuantity(row) {

        /*
         * SearchLocation trả về StockQuantity.
         * Để giữ đúng API hiện tại, tìm lại Bin theo
         * warehouse + location display/barcode.
         */

        const productIdInput =
            row.querySelector(".product-id");

        const locationIdInput =
            row.querySelector(".location-id");

        if (!productIdInput ||
            !locationIdInput) {
            return;
        }

        const productId =
            parseInt(
                productIdInput.value || "0"
            );

        const locationId =
            parseInt(
                locationIdInput.value || "0"
            );

        if (productId <= 0 ||
            locationId <= 0) {

            setSystemQuantity(
                row,
                0
            );

            return;
        }

        const locationInput =
            row.querySelector(
                ".location-search"
            );

        const keyword =
            locationInput
                ? locationInput.value.trim()
                : "";

        const warehouseId =
            getWarehouseId();

        if (warehouseId <= 0) {
            return;
        }

        try {

            const response =
                await fetch(
                    `${locationSearchUrl}?warehouseId=${warehouseId}&keyword=${encodeURIComponent(keyword)}`
                );

            if (!response.ok) {
                return;
            }

            const locations =
                await response.json();

            const location =
                locations.find(
                    x => x.id === locationId
                );

            if (location) {

                setSystemQuantity(
                    row,
                    Number(location.stockQuantity || 0)
                );
            }
        }
        catch (error) {

            console.error(error);
        }
    }


    // =========================================================
    // CHANGE WAREHOUSE
    // =========================================================

    if (warehouseSelect) {

        warehouseSelect.addEventListener(
            "change",
            function () {

                const rows =
                    linesContainer.querySelectorAll(
                        ".stocktake-line"
                    );

                rows.forEach(function (row) {

                    const locationInput =
                        row.querySelector(
                            ".location-search"
                        );

                    const locationIdInput =
                        row.querySelector(
                            ".location-id"
                        );

                    const systemInput =
                        row.querySelector(
                            ".system-qty"
                        );

                    if (locationInput) {
                        locationInput.value = "";
                        locationInput.dataset.locationId = "";
                    }

                    if (locationIdInput) {
                        locationIdInput.value = "0";
                    }

                    if (systemInput) {
                        systemInput.value = "0";
                    }

                    calculateDifference(row);
                });
            }
        );
    }


    // =========================================================
    // HIDE SEARCH RESULTS
    // =========================================================

    function hideSearchResults(
        row,
        selector
    ) {

        const results =
            row.querySelector(selector);

        if (!results) {
            return;
        }

        results.innerHTML = "";

        results.style.display =
            "none";
    }


    // =========================================================
    // INLINE ERROR
    // =========================================================

    function showInlineError(
        row,
        selector,
        message
    ) {

        const input =
            row.querySelector(selector);

        if (!input) {
            return;
        }

        input.classList.add(
            "is-invalid"
        );

        let errorElement =
            input.parentElement
                .querySelector(
                    ".stocktake-inline-error"
                );

        if (!errorElement) {

            errorElement =
                document.createElement(
                    "div"
                );

            errorElement.className =
                "stocktake-inline-error text-danger small mt-1";

            input.parentElement.appendChild(
                errorElement
            );
        }

        errorElement.textContent =
            message;

        setTimeout(
            function () {

                input.classList.remove(
                    "is-invalid"
                );

                if (errorElement) {
                    errorElement.remove();
                }

            },
            3000
        );
    }


    // =========================================================
    // CLOSE SEARCH RESULTS WHEN CLICKING OUTSIDE
    // =========================================================

    document.addEventListener(
        "click",
        function (event) {

            if (event.target.closest(
                    ".product-search") ||
                event.target.closest(
                    ".location-search") ||
                event.target.closest(
                    ".search-results")) {
                return;
            }

            document
                .querySelectorAll(
                    ".search-results"
                )
                .forEach(function (element) {

                    element.style.display =
                        "none";

                });
        }
    );


    // =========================================================
    // FORM VALIDATION
    // =========================================================

    form.addEventListener(
        "submit",
        function (event) {

            const rows =
                linesContainer.querySelectorAll(
                    ".stocktake-line"
                );

            if (rows.length === 0) {

                event.preventDefault();

                alert(
                    "Phiếu kiểm kê phải có ít nhất một sản phẩm."
                );

                return;
            }

            let valid =
                true;

            rows.forEach(function (row) {

                const productId =
                    parseInt(
                        row.querySelector(
                            ".product-id"
                        )?.value || "0"
                    );

                const locationId =
                    parseInt(
                        row.querySelector(
                            ".location-id"
                        )?.value || "0"
                    );

                const countedQty =
                    parseInt(
                        row.querySelector(
                            ".counted-qty"
                        )?.value || "0"
                    );

                if (productId <= 0) {

                    valid = false;

                    showInlineError(
                        row,
                        ".product-search",
                        "Vui lòng chọn sản phẩm"
                    );
                }

                if (locationId <= 0) {

                    valid = false;

                    showInlineError(
                        row,
                        ".location-search",
                        "Vui lòng chọn Bin"
                    );
                }

                if (countedQty < 0) {

                    valid = false;

                    showInlineError(
                        row,
                        ".counted-qty",
                        "Số lượng thực tế không được âm"
                    );
                }
            });

            if (!valid) {
                event.preventDefault();
            }

        }
    );


    // =========================================================
    // INITIALIZATION
    // =========================================================

    document
        .querySelectorAll(
            ".stocktake-line"
        )
        .forEach(function (row) {

            calculateDifference(row);

        });

    reindexLines();

    updateEmptyMessage();

});