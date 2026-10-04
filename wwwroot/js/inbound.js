document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("inboundForm");

    if (!form) {
        return;
    }

    const warehouseSelect =
        document.getElementById("WarehouseId");

    const linesBody =
        document.getElementById("inboundLinesBody");

    const addLineButton =
        document.getElementById("btnAddInboundLine");

    let lineIndex = linesBody
        ? linesBody.querySelectorAll("tr.inbound-line").length
        : 0;


    // =========================================================
    // THÊM DÒNG
    // =========================================================

    if (addLineButton) {

        addLineButton.addEventListener("click", function () {

            addInboundLine();

        });

    }


    function addInboundLine() {

        if (!linesBody) {
            return;
        }

        const index = lineIndex++;

        const row = document.createElement("tr");

        row.className = "inbound-line";

        row.innerHTML = `
            <td>
                <input type="hidden"
                       name="Lines[${index}].ProductId"
                       class="product-id" />

                <input type="text"
                       class="form-control form-control-sm product-keyword"
                       placeholder="Barcode / Mã SP / ISBN" />

                <div class="product-result small mt-1"></div>
            </td>

            <td>
                <div class="product-display text-muted">
                    -
                </div>

                <div class="product-meta small text-muted">
                </div>
            </td>

            <td>
                <input type="number"
                       name="Lines[${index}].ExpectedQty"
                       class="form-control form-control-sm expected-qty"
                       min="0"
                       value="0" />
            </td>

            <td>
                <input type="number"
                       name="Lines[${index}].ReceivedQty"
                       class="form-control form-control-sm received-qty"
                       min="0"
                       value="0" />
            </td>

            <td>
                <input type="hidden"
                       name="Lines[${index}].LocationId"
                       class="location-id" />

                <input type="text"
                       class="form-control form-control-sm location-keyword"
                       placeholder="Barcode / Mã Bin" />

                <div class="location-result small mt-1"></div>
            </td>

            <td>
                <input type="text"
                       class="form-control form-control-sm location-display"
                       readonly
                       placeholder="Chưa chọn Bin" />
            </td>

            <td>
                <input type="text"
                       name="Lines[${index}].Note"
                       class="form-control form-control-sm"
                       maxlength="500" />
            </td>

            <td class="text-center">
                <button type="button"
                        class="btn btn-sm btn-outline-danger btn-remove-line"
                        title="Xóa dòng">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;

        linesBody.appendChild(row);

        bindLineEvents(row);
    }


    // =========================================================
    // BIND EVENT CHO DÒNG
    // =========================================================

    function bindLineEvents(row) {

        const productInput =
            row.querySelector(".product-keyword");

        const locationInput =
            row.querySelector(".location-keyword");

        const removeButton =
            row.querySelector(".btn-remove-line");

        const receivedInput =
            row.querySelector(".received-qty");

        const expectedInput =
            row.querySelector(".expected-qty");


        if (productInput) {

            productInput.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Enter") {

                        event.preventDefault();

                        searchProduct(row);

                    }

                }
            );

        }


        if (locationInput) {

            locationInput.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Enter") {

                        event.preventDefault();

                        searchLocation(row);

                    }

                }
            );

        }


        if (removeButton) {

            removeButton.addEventListener(
                "click",
                function () {

                    row.remove();

                }
            );

        }


        if (expectedInput) {

            expectedInput.addEventListener(
                "change",
                function () {

                    validateQuantity(
                        expectedInput
                    );

                }
            );

        }


        if (receivedInput) {

            receivedInput.addEventListener(
                "change",
                function () {

                    validateQuantity(
                        receivedInput
                    );

                }
            );

        }

    }


    // =========================================================
    // TÌM PRODUCT
    // =========================================================

    async function searchProduct(row) {

        const input =
            row.querySelector(".product-keyword");

        const resultBox =
            row.querySelector(".product-result");

        const productId =
            row.querySelector(".product-id");

        const productDisplay =
            row.querySelector(".product-display");

        const productMeta =
            row.querySelector(".product-meta");

        if (!input) {
            return;
        }

        const keyword =
            input.value.trim();

        if (!keyword) {

            showMessage(
                resultBox,
                "Vui lòng nhập Barcode, mã sản phẩm hoặc ISBN.",
                "text-danger"
            );

            return;
        }


        showMessage(
            resultBox,
            "Đang tìm...",
            "text-muted"
        );


        try {

            const response =
                await fetch(
                    `/Inbound/SearchProduct?keyword=${encodeURIComponent(keyword)}`
                );

            if (!response.ok) {

                throw new Error(
                    "Không thể tìm kiếm sản phẩm."
                );

            }


            const result =
                await response.json();


            if (!result.success ||
                !result.data ||
                result.data.length === 0) {

                clearProduct(row);

                showMessage(
                    resultBox,
                    "Không tìm thấy sản phẩm",
                    "text-danger"
                );

                return;
            }


            // Nếu tìm thấy chính xác một sản phẩm
            if (result.data.length === 1) {

                selectProduct(
                    row,
                    result.data[0]
                );

                return;
            }


            // Có nhiều kết quả
            renderProductResults(
                row,
                result.data
            );

        }
        catch (error) {

            console.error(error);

            showMessage(
                resultBox,
                "Không thể tìm kiếm sản phẩm.",
                "text-danger"
            );

        }

    }


    function renderProductResults(row, products) {

        const resultBox =
            row.querySelector(".product-result");

        resultBox.innerHTML = "";

        products.forEach(function (product) {

            const button =
                document.createElement("button");

            button.type = "button";

            button.className =
                "btn btn-sm btn-outline-secondary me-1 mb-1";

            button.textContent =
                `${product.productCode} - ${product.name}`;

            button.addEventListener(
                "click",
                function () {

                    selectProduct(
                        row,
                        product
                    );

                }
            );

            resultBox.appendChild(button);

        });

    }


    function selectProduct(row, product) {

        const productId =
            row.querySelector(".product-id");

        const productDisplay =
            row.querySelector(".product-display");

        const productMeta =
            row.querySelector(".product-meta");

        const productInput =
            row.querySelector(".product-keyword");

        const resultBox =
            row.querySelector(".product-result");


        productId.value =
            product.id;

        productInput.value =
            product.barcode ||
            product.productCode;

        productDisplay.textContent =
            product.name;

        productMeta.textContent =
            [
                product.productCode,
                product.isbn
                    ? `ISBN: ${product.isbn}`
                    : null,
                product.productType,
                product.unit
            ]
            .filter(Boolean)
            .join(" | ");

        resultBox.innerHTML = "";

    }


    function clearProduct(row) {

        row.querySelector(".product-id").value = "";

        row.querySelector(".product-display")
            .textContent = "-";

        row.querySelector(".product-meta")
            .textContent = "";

    }


    // =========================================================
    // TÌM BIN
    // =========================================================

    async function searchLocation(row) {

        const warehouseId =
            warehouseSelect
                ? warehouseSelect.value
                : "";

        const input =
            row.querySelector(".location-keyword");

        const resultBox =
            row.querySelector(".location-result");

        if (!warehouseId) {

            showMessage(
                resultBox,
                "Vui lòng chọn kho nhận trước.",
                "text-danger"
            );

            return;
        }


        const keyword =
            input.value.trim();

        if (!keyword) {

            showMessage(
                resultBox,
                "Vui lòng nhập Barcode hoặc mã vị trí.",
                "text-danger"
            );

            return;
        }


        showMessage(
            resultBox,
            "Đang tìm...",
            "text-muted"
        );


        try {

            const response =
                await fetch(
                    `/Inbound/SearchLocation?warehouseId=${encodeURIComponent(warehouseId)}&keyword=${encodeURIComponent(keyword)}`
                );


            if (!response.ok) {

                throw new Error(
                    "Không thể tìm kiếm vị trí."
                );

            }


            const result =
                await response.json();


            if (!result.success ||
                !result.data ||
                result.data.length === 0) {

                clearLocation(row);

                showMessage(
                    resultBox,
                    "Không tìm thấy vị trí",
                    "text-danger"
                );

                return;
            }


            if (result.data.length === 1) {

                selectLocation(
                    row,
                    result.data[0]
                );

                return;
            }


            renderLocationResults(
                row,
                result.data
            );

        }
        catch (error) {

            console.error(error);

            showMessage(
                resultBox,
                "Không thể tìm kiếm vị trí.",
                "text-danger"
            );

        }

    }


    function renderLocationResults(row, locations) {

        const resultBox =
            row.querySelector(".location-result");

        resultBox.innerHTML = "";

        locations.forEach(function (location) {

            const button =
                document.createElement("button");

            button.type = "button";

            button.className =
                "btn btn-sm btn-outline-secondary me-1 mb-1";

            button.textContent =
                `${location.code} - ${location.name}`;

            button.addEventListener(
                "click",
                function () {

                    selectLocation(
                        row,
                        location
                    );

                }
            );

            resultBox.appendChild(button);

        });

    }


    function selectLocation(row, location) {

        const locationId =
            row.querySelector(".location-id");

        const locationInput =
            row.querySelector(".location-keyword");

        const locationDisplay =
            row.querySelector(".location-display");

        const resultBox =
            row.querySelector(".location-result");


        locationId.value =
            location.id;

        locationInput.value =
            location.barcode ||
            location.code;

        locationDisplay.value =
            `${location.code} - ${location.name}`;

        resultBox.innerHTML = "";

    }


    function clearLocation(row) {

        row.querySelector(".location-id").value = "";

        row.querySelector(".location-display").value = "";

    }


    // =========================================================
    // VALIDATE QUANTITY
    // =========================================================

    function validateQuantity(input) {

        const value =
            parseInt(input.value, 10);

        if (Number.isNaN(value) || value < 0) {

            input.value = 0;

        }

    }


    // =========================================================
    // MESSAGE
    // =========================================================

    function showMessage(element, message, cssClass) {

        if (!element) {
            return;
        }

        element.className =
            `small mt-1 ${cssClass}`;

        element.textContent =
            message;

    }


    // =========================================================
    // BIND CÁC DÒNG CÓ SẴN
    // =========================================================

    if (linesBody) {

        linesBody
            .querySelectorAll("tr.inbound-line")
            .forEach(function (row) {

                bindLineEvents(row);

            });

    }

});