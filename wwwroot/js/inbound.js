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

    const productSearchUrl =
        form.dataset.productSearchUrl;

    const locationSearchUrl =
        form.dataset.locationSearchUrl;

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

    form.addEventListener("submit", renumberLines);


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
                       name="Lines[${index}].ProductBarcode"
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
                       name="Lines[${index}].LocationBarcode"
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

        let productSearchTimer;
        let locationSearchTimer;

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
                "input",
                function () {

                    clearTimeout(productSearchTimer);
                    hideSearchDropdown();
                    clearProduct(row);

                    productSearchTimer =
                        setTimeout(
                            function () {
                                searchProduct(row);
                            },
                            250
                        );

                }
            );

            productInput.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Enter") {

                        event.preventDefault();
                        clearTimeout(productSearchTimer);

                        searchProduct(row, true);

                    }

                }
            );

        }


        if (locationInput) {

            locationInput.addEventListener(
                "input",
                function () {

                    clearTimeout(locationSearchTimer);
                    hideSearchDropdown();
                    clearLocation(row);

                    locationSearchTimer =
                        setTimeout(
                            function () {
                                searchLocation(row);
                            },
                            250
                        );

                }
            );

            locationInput.addEventListener(
                "keydown",
                function (event) {

                    if (event.key === "Enter") {

                        event.preventDefault();
                        clearTimeout(locationSearchTimer);

                        searchLocation(row, true);

                    }

                }
            );

        }


        if (removeButton) {

            removeButton.addEventListener(
                "click",
                function () {

                    row.remove();
                    renumberLines();

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

    async function searchProduct(row, selectSingle = false) {

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

            hideSearchDropdown();

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
                    `${productSearchUrl}?keyword=${encodeURIComponent(keyword)}`
                );

            if (!response.ok) {

                throw new Error(
                    "Không thể tìm kiếm sản phẩm."
                );

            }


            const result =
                await response.json();

            if (input.value.trim() !== keyword) {
                return;
            }


            const products =
                Array.isArray(result)
                    ? result
                    : result.data ?? result.Data ?? [];

            if ((!Array.isArray(result) &&
                 !(result.success ?? result.Success)) ||
                !Array.isArray(products) ||
                products.length === 0) {

                hideSearchDropdown();
                clearProduct(row);

                showMessage(
                    resultBox,
                    "Không tìm thấy sản phẩm đang hoạt động phù hợp.",
                    "text-danger"
                );

                return;
            }


            // Nếu tìm thấy chính xác một sản phẩm
            if (selectSingle && products.length === 1) {

                selectProduct(
                    row,
                    products[0]
                );

                return;
            }


            // Có nhiều kết quả
            renderProductResults(
                row,
                products
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

        hideSearchDropdown();
        resultBox.textContent = "";

        const input =
            row.querySelector(".product-keyword");

        const dropdown =
            createSearchDropdown(input);

        products.forEach(function (product) {

            const button =
                document.createElement("button");

            button.type = "button";

            button.className =
                "list-group-item list-group-item-action";

            button.textContent =
                `${product.productCode ?? product.ProductCode} - ${product.name ?? product.Name} | Barcode: ${product.barcode ?? product.Barcode ?? "-"} | ISBN: ${product.isbn ?? product.ISBN ?? "-"}`;

            button.addEventListener(
                "click",
                function () {

                    selectProduct(
                        row,
                        product
                    );
                    hideSearchDropdown();

                }
            );

            dropdown.appendChild(button);

        });

        positionSearchDropdown(input, dropdown);
        document.body.appendChild(dropdown);

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

        hideSearchDropdown();


        productId.value =
            product.id ?? product.Id;

        productInput.value =
            product.barcode ||
            product.Barcode ||
            product.productCode ||
            product.ProductCode;

        productDisplay.textContent =
            product.name ?? product.Name;

        productMeta.textContent =
            [
                product.productCode ?? product.ProductCode,
                (product.isbn ?? product.ISBN)
                    ? `ISBN: ${product.isbn ?? product.ISBN}`
                    : null,
                product.productType ?? product.ProductType,
                product.unit ?? product.Unit
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

    async function searchLocation(row, selectSingle = false) {

        const warehouseId =
            warehouseSelect
                ? warehouseSelect.value
                : "";

        const input =
            row.querySelector(".location-keyword");

        const resultBox =
            row.querySelector(".location-result");

        if (!warehouseId) {

            hideSearchDropdown();

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

            hideSearchDropdown();

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
                    `${locationSearchUrl}?warehouseId=${encodeURIComponent(warehouseId)}&keyword=${encodeURIComponent(keyword)}`
                );


            if (!response.ok) {

                throw new Error(
                    "Không thể tìm kiếm vị trí."
                );

            }


            const result =
                await response.json();

            if (input.value.trim() !== keyword) {
                return;
            }


            const locations =
                Array.isArray(result)
                    ? result
                    : result.data ?? result.Data ?? [];

            if ((!Array.isArray(result) &&
                 !(result.success ?? result.Success)) ||
                !Array.isArray(locations) ||
                locations.length === 0) {

                hideSearchDropdown();
                clearLocation(row);

                showMessage(
                    resultBox,
                    "Không tìm thấy Bin đang hoạt động phù hợp trong kho đã chọn.",
                    "text-danger"
                );

                return;
            }


            if (selectSingle && locations.length === 1) {

                selectLocation(
                    row,
                    locations[0]
                );

                return;
            }


            renderLocationResults(
                row,
                locations
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

        hideSearchDropdown();
        resultBox.textContent = "";

        const input =
            row.querySelector(".location-keyword");

        const dropdown =
            createSearchDropdown(input);

        locations.forEach(function (location) {

            const button =
                document.createElement("button");

            button.type = "button";

            button.className =
                "list-group-item list-group-item-action";

            button.textContent =
                `${location.code ?? location.Code} - ${location.name ?? location.Name} | Barcode: ${location.barcode ?? location.Barcode ?? "-"}`;

            button.addEventListener(
                "click",
                function () {

                    selectLocation(
                        row,
                        location
                    );
                    hideSearchDropdown();

                }
            );

            dropdown.appendChild(button);

        });

        positionSearchDropdown(input, dropdown);
        document.body.appendChild(dropdown);

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

        hideSearchDropdown();


        locationId.value =
            location.id ?? location.Id;

        locationInput.value =
            location.barcode ||
            location.Barcode ||
            location.code ||
            location.Code;

        locationDisplay.value =
            `${location.code ?? location.Code} - ${location.name ?? location.Name}`;

        resultBox.innerHTML = "";

    }


    function clearLocation(row) {

        row.querySelector(".location-id").value = "";

        row.querySelector(".location-display").value = "";

    }


    function createSearchDropdown(input) {

        const dropdown =
            document.createElement("div");

        dropdown.className =
            "inbound-search-dropdown";

        positionSearchDropdown(input, dropdown);

        return dropdown;

    }


    function positionSearchDropdown(input, dropdown) {

        const rect =
            input.getBoundingClientRect();

        dropdown.style.position =
            "absolute";
        dropdown.style.zIndex =
            "9999";
        dropdown.style.width =
            `${input.offsetWidth}px`;
        dropdown.style.left =
            `${rect.left + window.scrollX}px`;
        dropdown.style.top =
            `${rect.bottom + window.scrollY}px`;

    }


    function hideSearchDropdown() {

        document
            .querySelectorAll(".inbound-search-dropdown")
            .forEach(function (dropdown) {
                dropdown.remove();
            });

    }


    if (warehouseSelect) {

        warehouseSelect.addEventListener(
            "change",
            function () {

                hideSearchDropdown();

                if (linesBody) {
                    linesBody
                        .querySelectorAll("tr.inbound-line")
                        .forEach(function (row) {
                            clearLocation(row);
                            row.querySelector(
                                ".location-keyword"
                            ).value = "";
                        });
                }

            }
        );

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

        const resultType =
            element.classList.contains("product-result")
                ? "product-result"
                : element.classList.contains("location-result")
                    ? "location-result"
                    : "";

        element.className =
            `${resultType} small mt-1 ${cssClass}`.trim();

        element.textContent =
            message;

    }


    function renumberLines() {

        if (!linesBody) {
            return;
        }

        linesBody
            .querySelectorAll("tr.inbound-line")
            .forEach(function (row, index) {

                row
                    .querySelectorAll("[name], [id], [data-valmsg-for]")
                    .forEach(function (element) {

                        ["name", "data-valmsg-for"].forEach(function (attribute) {

                            const value =
                                element.getAttribute(attribute);

                            if (value) {

                                element.setAttribute(
                                    attribute,
                                    value.replace(
                                        /Lines\[\d+\]/g,
                                        `Lines[${index}]`
                                    )
                                );

                            }

                        });

                        const id =
                            element.getAttribute("id");

                        if (id) {

                            element.id = id.replace(
                                /Lines_(\d+)__/g,
                                `Lines_${index}__`
                            );

                        }

                    });

            });

        lineIndex =
            linesBody.querySelectorAll("tr.inbound-line").length;

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