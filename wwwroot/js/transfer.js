document.addEventListener("DOMContentLoaded", function () {

    const form =
        document.getElementById("transferForm");

    const tbody =
        document.getElementById("transferLinesBody");

    const btnAddLine =
        document.getElementById("btnAddLine");

    const sourceWarehouse =
        document.getElementById("sourceWarehouse");

    const destinationWarehouse =
        document.getElementById("destinationWarehouse");

    const searchTimers = new WeakMap();

    function hideSearchResults(results) {
        if (!results) {
            return;
        }

        results.style.display = "none";
        results.innerHTML = "";
    }

    function positionSearchResults(input, results) {
        if (!input || !results || results.style.display === "none") {
            return;
        }

        const bounds = input.getBoundingClientRect();
        const left = Math.max(8, Math.min(bounds.left, window.innerWidth - 200));
        const width = Math.min(
            Math.max(bounds.width, 200),
            window.innerWidth - left - 8
        );
        const spaceBelow = Math.max(0, window.innerHeight - bounds.bottom - 8);
        const spaceAbove = Math.max(0, bounds.top - 8);
        const placeBelow = spaceBelow >= 80 || spaceBelow >= spaceAbove;
        const availableHeight = placeBelow ? spaceBelow : spaceAbove;
        const maxHeight = Math.max(0, Math.min(280, availableHeight));
        const popupHeight = Math.min(maxHeight, results.scrollHeight);

        results.style.left = `${left}px`;
        results.style.width = `${width}px`;
        results.style.maxHeight = `${maxHeight}px`;
        results.style.top = placeBelow
            ? `${bounds.bottom}px`
            : `${Math.max(8, bounds.top - popupHeight)}px`;
    }

    function showSearchMessage(input, results, message) {
        results.classList.add("transfer-search-dropdown");
        results.innerHTML = `
            <div class="search-result-empty">${escapeHtml(message)}</div>
        `;
        results.style.display = "block";
        positionSearchResults(input, results);
    }

    document.addEventListener("click", function (event) {
        if (event.target.closest(".transfer-search-dropdown") ||
            event.target.closest(".product-search") ||
            event.target.closest(".source-location-search") ||
            event.target.closest(".destination-location-search") ||
            event.target.closest(".btn-search-product") ||
            event.target.closest(".btn-search-source-location") ||
            event.target.closest(".btn-search-destination-location")) {
            return;
        }

        document.querySelectorAll(".transfer-search-dropdown").forEach(hideSearchResults);
    });

    function scheduleSearch(input, callback) {
        clearTimeout(searchTimers.get(input));

        const keyword = input.value;
        if (!keyword.trim()) {
            hideSearchResults(input.closest(".input-group")?.nextElementSibling);
            return;
        }

        searchTimers.set(
            input,
            setTimeout(() => callback(keyword), 250)
        );
    }

    window.addEventListener("scroll", function () {
        document
            .querySelectorAll(".transfer-search-dropdown")
            .forEach(function (results) {
                const row = results.closest(".transfer-line");
                if (!row) {
                    return;
                }

                const input = results.classList.contains("product-results")
                    ? row.querySelector(".product-search")
                    : results.classList.contains("source-location-results")
                        ? row.querySelector(".source-location-search")
                        : row.querySelector(".destination-location-search");

                positionSearchResults(input, results);
            });
    }, true);

    window.addEventListener("resize", function () {
        document
            .querySelectorAll(".transfer-search-dropdown")
            .forEach(function (results) {
                const row = results.closest(".transfer-line");
                if (!row) {
                    return;
                }

                const input = results.classList.contains("product-results")
                    ? row.querySelector(".product-search")
                    : results.classList.contains("source-location-results")
                        ? row.querySelector(".source-location-search")
                        : row.querySelector(".destination-location-search");

                positionSearchResults(input, results);
            });
    });

    // =========================================================
    // ADD LINE
    // =========================================================

    if (btnAddLine) {

        btnAddLine.addEventListener(
            "click",
            function () {

                addLine();

            });

    }


    // =========================================================
    // ADD LINE FUNCTION
    // =========================================================

    function addLine() {

        const index =
            tbody.querySelectorAll(
                ".transfer-line").length;

        const tr =
            document.createElement("tr");

        tr.className =
            "transfer-line";

        tr.innerHTML = `

            <td>

                <input type="hidden"
                       name="Lines[${index}].Id"
                       value="0" />

                <input type="hidden"
                       name="Lines[${index}].ProductId"
                       class="product-id"
                       value="0" />

                <div class="input-group">

                    <input type="text"
                           class="form-control product-search"
                           placeholder="Quét / nhập barcode..." />

                    <button type="button"
                            class="btn btn-outline-secondary btn-search-product">

                        <i class="bi bi-search"></i>

                    </button>

                </div>

                <div class="product-results"></div>

            </td>


            <td>

                <input type="text"
                       name="Lines[${index}].ProductBarcode"
                       class="form-control product-barcode"
                       readonly />

            </td>


            <td>

                <input type="hidden"
                       name="Lines[${index}].SourceLocationId"
                       class="source-location-id"
                       value="0" />

                <div class="input-group">

                    <input type="text"
                           class="form-control source-location-search"
                           placeholder="Quét Barcode Bin..." />

                    <button type="button"
                            class="btn btn-outline-secondary btn-search-source-location">

                        <i class="bi bi-search"></i>

                    </button>

                </div>

                <div class="source-location-results"></div>

            </td>


            <td>

                <input type="hidden"
                       name="Lines[${index}].DestinationLocationId"
                       class="destination-location-id"
                       value="0" />

                <div class="input-group">

                    <input type="text"
                           class="form-control destination-location-search"
                           placeholder="Quét Barcode Bin..." />

                    <button type="button"
                            class="btn btn-outline-secondary btn-search-destination-location">

                        <i class="bi bi-search"></i>

                    </button>

                </div>

                <div class="destination-location-results"></div>

            </td>


            <td class="text-end">

                <span class="source-stock">
                    0
                </span>

            </td>


            <td>

                <input type="number"
                       name="Lines[${index}].Quantity"
                       class="form-control quantity text-end"
                       min="1"
                       value="1" />

            </td>


            <td class="text-center">

                <button type="button"
                        class="btn btn-sm btn-outline-danger btn-remove-line">

                    <i class="bi bi-trash"></i>

                </button>

            </td>

        `;

        tbody.appendChild(tr);

        bindLineEvents(tr);

    }


    // =========================================================
    // BIND EXISTING LINES
    // =========================================================

    tbody
        .querySelectorAll(".transfer-line")
        .forEach(function (row) {

            bindLineEvents(row);

        });


    // =========================================================
    // LINE EVENTS
    // =========================================================

    function bindLineEvents(row) {

        const removeButton =
            row.querySelector(
                ".btn-remove-line");

        if (removeButton) {

            removeButton.addEventListener(
                "click",
                function () {

                    removeLine(row);

                });

        }


        const productInput =
            row.querySelector(
                ".product-search");

        if (productInput) {

            productInput.addEventListener(
                "input",
                function () {
                    row.querySelector(".product-id").value = "0";
                    row.querySelector(".product-barcode").value = "";
                    row.querySelector(".source-location-id").value = "0";
                    row.querySelector(".source-location-search").value = "";
                    row.querySelector(".source-stock").textContent = "0";
                    hideSearchResults(row.querySelector(".product-results"));
                    hideSearchResults(row.querySelector(".source-location-results"));
                    scheduleSearch(productInput, function (keyword) {
                        searchProduct(row, keyword);
                    });
                });

            productInput.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Enter") {

                        event.preventDefault();

                        searchProduct(
                            row,
                            productInput.value,
                            true);

                    }

                });

        }


        const productButton =
            row.querySelector(
                ".btn-search-product");

        if (productButton) {

            productButton.addEventListener(
                "click",
                function () {

                    searchProduct(
                        row,
                        productInput.value);

                });

        }


        const sourceInput =
            row.querySelector(
                ".source-location-search");

        if (sourceInput) {

            sourceInput.addEventListener(
                "input",
                function () {
                    row.querySelector(".source-location-id").value = "0";
                    row.querySelector(".source-stock").textContent = "0";
                    hideSearchResults(row.querySelector(".source-location-results"));
                    scheduleSearch(sourceInput, function (keyword) {
                        searchSourceLocation(row, keyword);
                    });
                });

            sourceInput.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Enter") {

                        event.preventDefault();

                        searchSourceLocation(
                            row,
                            sourceInput.value,
                            true);

                    }

                });

        }


        const sourceButton =
            row.querySelector(
                ".btn-search-source-location");

        if (sourceButton) {

            sourceButton.addEventListener(
                "click",
                function () {

                    searchSourceLocation(
                        row,
                        sourceInput.value);

                });

        }


        const destinationInput =
            row.querySelector(
                ".destination-location-search");

        if (destinationInput) {

            destinationInput.addEventListener(
                "input",
                function () {
                    row.querySelector(".destination-location-id").value = "0";
                    hideSearchResults(row.querySelector(".destination-location-results"));
                    scheduleSearch(destinationInput, function (keyword) {
                        searchDestinationLocation(row, keyword);
                    });
                });

            destinationInput.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Enter") {

                        event.preventDefault();

                        searchDestinationLocation(
                            row,
                            destinationInput.value,
                            true);

                    }

                });

        }


        const destinationButton =
            row.querySelector(
                ".btn-search-destination-location");

        if (destinationButton) {

            destinationButton.addEventListener(
                "click",
                function () {

                    searchDestinationLocation(
                        row,
                        destinationInput.value);

                });

        }


        const quantity =
            row.querySelector(
                ".quantity");

        if (quantity) {

            quantity.addEventListener(
                "input",
                function () {

                    validateQuantity(row);

                });

        }

    }


    // =========================================================
    // REMOVE LINE
    // =========================================================

    function removeLine(row) {

        const rows =
            tbody.querySelectorAll(
                ".transfer-line");

        if (rows.length <= 1) {

            clearLine(row);

            return;

        }

        row.remove();

        reindexLines();

    }


    // =========================================================
    // CLEAR LINE
    // =========================================================

    function clearLine(row) {

        row.querySelector(
            ".product-id").value = "0";

        row.querySelector(
            ".product-search").value = "";

        row.querySelector(
            ".product-barcode").value = "";

        row.querySelector(
            ".source-location-id").value = "0";

        row.querySelector(
            ".source-location-search").value = "";

        row.querySelector(
            ".destination-location-id").value = "0";

        row.querySelector(
            ".destination-location-search").value = "";

        row.querySelector(
            ".source-stock").textContent = "0";

        row.querySelector(
            ".quantity").value = "1";

    }


    // =========================================================
    // REINDEX
    // =========================================================

    function reindexLines() {

        tbody
            .querySelectorAll(".transfer-line")
            .forEach(function (row, index) {

                row.querySelectorAll(
                    "[name]"
                ).forEach(function (input) {

                    input.name =
                        input.name.replace(
                            /Lines\[\d+\]/,
                            `Lines[${index}]`);

                });

            });

    }


    // =========================================================
    // SEARCH PRODUCT
    // =========================================================

    async function searchProduct(
        row,
        keyword,
        selectSingleResult = false) {

        keyword =
            keyword.trim();

        if (!keyword) {

            return;

        }

        try {

            const response =
                await fetch(
                    `/Transfer/SearchProduct?keyword=${encodeURIComponent(keyword)}`);

            if (!response.ok) {

                throw new Error(
                    "Không thể tìm sản phẩm.");

            }

            const products =
                await response.json();

            const input = row.querySelector(".product-search");
            if (!row.isConnected || input.value.trim() !== keyword) {
                return;
            }

            const results =
                row.querySelector(
                    ".product-results");

            results.innerHTML = "";

            if (!Array.isArray(products) || !products.length) {
                showSearchMessage(input, results, "Không tìm thấy sản phẩm");
                return;
            }

            if (selectSingleResult && products.length === 1) {
                selectProduct(row, products[0]);
                return;
            }

            products.forEach(function (product) {

                const button =
                    document.createElement("button");

                button.type =
                    "button";

                button.className =
                    "list-group-item list-group-item-action";

                button.innerHTML = `
                    <strong>
                        ${escapeHtml(product.productCode)}
                    </strong>
                    -
                    ${escapeHtml(product.name)}
                    <br>
                    <small class="text-muted">
                        Barcode:
                        ${escapeHtml(product.barcode || "—")}
                        |
                        ISBN:
                        ${escapeHtml(product.isbn || "—")}
                    </small>
                `;

                button.addEventListener(
                    "click",
                    function () {
                        selectProduct(
                            row,
                            product);
                    });

                results.appendChild(button);

            });

            results.classList.add(
                "list-group");
            results.classList.add("transfer-search-dropdown");
            results.style.display = "block";
            positionSearchResults(input, results);

        }
        catch (error) {

            console.error(error);

            showSearchMessage(
                row.querySelector(".product-search"),
                row.querySelector(".product-results"),
                "Không thể tìm sản phẩm.");

        }

    }


    // =========================================================
    // SELECT PRODUCT
    // =========================================================

    function selectProduct(
        row,
        product) {

        row.querySelector(
            ".product-id").value =
                product.id;

        row.querySelector(
            ".product-search").value =
                `${product.productCode} - ${product.name}`;

        row.querySelector(
            ".product-barcode").value =
                product.barcode || "";

        row.querySelector(
            ".product-results").innerHTML = "";
        hideSearchResults(row.querySelector(".product-results"));

        row.querySelector(
            ".source-location-id").value =
                "0";

        row.querySelector(
            ".source-location-search").value =
                "";

        row.querySelector(
            ".source-stock").textContent =
                "0";

    }


    // =========================================================
    // SEARCH SOURCE LOCATION
    // =========================================================

    async function searchSourceLocation(
        row,
        keyword,
        selectSingleResult = false) {

        const warehouseId =
            sourceWarehouse
                ? sourceWarehouse.value
                : "";

        const productId =
            row.querySelector(
                ".product-id").value;


        if (!warehouseId) {
            showSearchMessage(
                row.querySelector(".source-location-search"),
                row.querySelector(".source-location-results"),
                "Vui lòng chọn kho nguồn trước.");
            return;

        }


        if (!productId ||
            productId === "0") {
            showSearchMessage(
                row.querySelector(".source-location-search"),
                row.querySelector(".source-location-results"),
                "Vui lòng chọn sản phẩm trước.");
            return;

        }


        try {

            const url =
                `/Transfer/SearchSourceLocation`
                + `?warehouseId=${warehouseId}`
                + `&productId=${productId}`
                + `&keyword=${encodeURIComponent(keyword || "")}`;


            const response =
                await fetch(url);

            if (!response.ok) {

                throw new Error(
                    "Không thể tìm Bin nguồn.");

            }


            const locations =
                await response.json();

            const input = row.querySelector(".source-location-search");
            if (!row.isConnected || input.value.trim() !== keyword.trim()) {
                return;
            }

            const results =
                row.querySelector(
                    ".source-location-results");

            results.innerHTML = "";


            if (!Array.isArray(locations) || !locations.length) {
                showSearchMessage(input, results, "Không tìm thấy Bin nguồn có tồn");
                return;
            }

            if (selectSingleResult && locations.length === 1) {
                selectSourceLocation(row, locations[0]);
                return;
            }


            locations.forEach(function (location) {

                const button =
                    document.createElement("button");

                button.type =
                    "button";

                button.className =
                    "list-group-item list-group-item-action";

                button.innerHTML = `
                    <strong>
                        ${escapeHtml(location.code)}
                    </strong>
                    -
                    ${escapeHtml(location.name)}
                    <br>
                    <small class="text-muted">
                        Barcode:
                        ${escapeHtml(location.barcode || "—")}
                        |
                        Tồn:
                        ${location.stockQuantity}
                    </small>
                `;

                button.addEventListener(
                    "click",
                    function () {

                        selectSourceLocation(
                            row,
                            location);

                    });

                results.appendChild(button);

            });

            results.classList.add(
                "list-group");
            results.classList.add("transfer-search-dropdown");
            results.style.display = "block";
            positionSearchResults(input, results);

        }
        catch (error) {

            console.error(error);

            showSearchMessage(
                row.querySelector(".source-location-search"),
                row.querySelector(".source-location-results"),
                "Không thể tìm Bin nguồn.");

        }

    }


    // =========================================================
    // SELECT SOURCE LOCATION
    // =========================================================

    function selectSourceLocation(
        row,
        location) {

        row.querySelector(
            ".source-location-id").value =
                location.id;

        row.querySelector(
            ".source-location-search").value =
                `${location.code} - ${location.name}`;

        row.querySelector(
            ".source-stock").textContent =
                location.stockQuantity;

        row.querySelector(
            ".source-location-results").innerHTML =
                "";
        hideSearchResults(row.querySelector(".source-location-results"));

        validateQuantity(row);

    }


    // =========================================================
    // SEARCH DESTINATION LOCATION
    // =========================================================

    async function searchDestinationLocation(
        row,
        keyword,
        selectSingleResult = false) {

        const warehouseId =
            destinationWarehouse
                ? destinationWarehouse.value
                : "";


        if (!warehouseId) {
            showSearchMessage(
                row.querySelector(".destination-location-search"),
                row.querySelector(".destination-location-results"),
                "Vui lòng chọn kho đích trước.");
            return;

        }


        try {

            const url =
                `/Transfer/SearchDestinationLocation`
                + `?warehouseId=${warehouseId}`
                + `&keyword=${encodeURIComponent(keyword || "")}`;


            const response =
                await fetch(url);

            if (!response.ok) {

                throw new Error(
                    "Không thể tìm Bin đích.");

            }


            const locations =
                await response.json();

            const input = row.querySelector(".destination-location-search");
            if (!row.isConnected || input.value.trim() !== keyword.trim()) {
                return;
            }

            const results =
                row.querySelector(
                    ".destination-location-results");

            results.innerHTML = "";


            if (!Array.isArray(locations) || !locations.length) {
                showSearchMessage(input, results, "Không tìm thấy Bin đích");
                return;
            }

            if (selectSingleResult && locations.length === 1) {
                selectDestinationLocation(row, locations[0]);
                return;
            }


            locations.forEach(function (location) {

                const button =
                    document.createElement("button");

                button.type =
                    "button";

                button.className =
                    "list-group-item list-group-item-action";

                button.innerHTML = `
                    <strong>
                        ${escapeHtml(location.code)}
                    </strong>
                    -
                    ${escapeHtml(location.name)}
                    <br>
                    <small class="text-muted">
                        Barcode:
                        ${escapeHtml(location.barcode || "—")}
                    </small>
                `;

                button.addEventListener(
                    "click",
                    function () {

                        selectDestinationLocation(
                            row,
                            location);

                    });

                results.appendChild(button);

            });

            results.classList.add(
                "list-group");
            results.classList.add("transfer-search-dropdown");
            results.style.display = "block";
            positionSearchResults(input, results);

        }
        catch (error) {

            console.error(error);

            showSearchMessage(
                row.querySelector(".destination-location-search"),
                row.querySelector(".destination-location-results"),
                "Không thể tìm Bin đích.");

        }

    }


    // =========================================================
    // SELECT DESTINATION
    // =========================================================

    function selectDestinationLocation(
        row,
        location) {

        row.querySelector(
            ".destination-location-id").value =
                location.id;

        row.querySelector(
            ".destination-location-search").value =
                `${location.code} - ${location.name}`;

        row.querySelector(
            ".destination-location-results").innerHTML =
                "";
        hideSearchResults(row.querySelector(".destination-location-results"));

    }


    // =========================================================
    // VALIDATE QUANTITY
    // =========================================================

    function validateQuantity(row) {

        const stock =
            parseInt(
                row.querySelector(
                    ".source-stock").textContent,
                10) || 0;

        const quantity =
            parseInt(
                row.querySelector(
                    ".quantity").value,
                10) || 0;


        const input =
            row.querySelector(
                ".quantity");


        if (quantity > stock &&
            stock > 0) {

            input.classList.add(
                "is-invalid");

        }
        else {

            input.classList.remove(
                "is-invalid");

        }

    }


    // =========================================================
    // WAREHOUSE CHANGE
    // =========================================================

    if (sourceWarehouse) {

        sourceWarehouse.addEventListener(
            "change",
            function () {

                tbody
                    .querySelectorAll(
                        ".transfer-line")
                    .forEach(function (row) {

                        row.querySelector(
                            ".source-location-id").value =
                                "0";

                        row.querySelector(
                            ".source-location-search").value =
                                "";

                        row.querySelector(
                            ".source-stock").textContent =
                                "0";

                    });

            });

    }


    if (destinationWarehouse) {

        destinationWarehouse.addEventListener(
            "change",
            function () {

                tbody
                    .querySelectorAll(
                        ".transfer-line")
                    .forEach(function (row) {

                        row.querySelector(
                            ".destination-location-id").value =
                                "0";

                        row.querySelector(
                            ".destination-location-search").value =
                                "";

                    });

            });

    }


    // =========================================================
    // FORM VALIDATION
    // =========================================================

    if (form) {

        form.addEventListener(
            "submit",
            function (event) {

                reindexLines();

                const sourceId =
                    sourceWarehouse
                        ? sourceWarehouse.value
                        : "";

                const destinationId =
                    destinationWarehouse
                        ? destinationWarehouse.value
                        : "";


                if (!sourceId) {

                    event.preventDefault();

                    alert(
                        "Vui lòng chọn kho nguồn.");

                    return;

                }


                if (!destinationId) {

                    event.preventDefault();

                    alert(
                        "Vui lòng chọn kho đích.");

                    return;

                }


                if (sourceId ===
                    destinationId) {

                    event.preventDefault();

                    alert(
                        "Kho nguồn và kho đích không được giống nhau.");

                    return;

                }


                const rows =
                    tbody.querySelectorAll(
                        ".transfer-line");


                if (rows.length === 0) {

                    event.preventDefault();

                    alert(
                        "Phiếu phải có ít nhất một sản phẩm.");

                    return;

                }


                let valid = true;


                rows.forEach(function (row) {

                    const productId =
                        row.querySelector(
                            ".product-id").value;

                    const sourceLocationId =
                        row.querySelector(
                            ".source-location-id").value;

                    const destinationLocationId =
                        row.querySelector(
                            ".destination-location-id").value;

                    const quantity =
                        parseInt(
                            row.querySelector(
                                ".quantity").value,
                            10) || 0;


                    if (!productId ||
                        productId === "0") {

                        valid = false;

                    }


                    if (!sourceLocationId ||
                        sourceLocationId === "0") {

                        valid = false;

                    }


                    if (!destinationLocationId ||
                        destinationLocationId === "0") {

                        valid = false;

                    }


                    if (quantity <= 0) {

                        valid = false;

                    }

                });


                if (!valid) {

                    event.preventDefault();

                    alert(
                        "Vui lòng kiểm tra đầy đủ sản phẩm, Bin và số lượng.");

                }

            });

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

});