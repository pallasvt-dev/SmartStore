// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", () => {
    setupSmoothNavigation();
    initializePage(document);
});

const initializePage = (root = document) => {
    const autoDismissDelay = 3000;
    movePageAlertsToToastContainer(root);

    const alerts = root.querySelectorAll(".alert");
    const vndInputs = root.querySelectorAll(".js-vnd-input");
    const imagePasteBoxes = root.querySelectorAll(".js-image-paste-box");

    alerts.forEach((alert) => {
        if (alert.dataset.alertReady === "true") {
            return;
        }

        alert.dataset.alertReady = "true";

        const closeButton = alert.querySelector(".alert-close");
        if (closeButton) {
            closeButton.addEventListener("click", () => dismissAlert(alert));
        }

        window.setTimeout(() => dismissAlert(alert), autoDismissDelay);
    });

    vndInputs.forEach((input) => {
        if (input.dataset.vndReady === "true") {
            return;
        }

        input.dataset.vndReady = "true";
        formatVndInput(input);
        validateVndInput(input);

        input.addEventListener("input", () => {
            formatVndInput(input);
            validateVndInput(input);
        });
        input.addEventListener("blur", () => validateVndInput(input));
        input.addEventListener("focus", () => moveCaretBeforeVnd(input));
        input.addEventListener("click", () => moveCaretBeforeVnd(input));

        input.form?.addEventListener("submit", (event) => {
            const formVndInputs = input.form.querySelectorAll(".js-vnd-input");
            const isInvalid = Array.from(formVndInputs).some((field) => !validateVndInput(field));

            if (isInvalid) {
                event.preventDefault();
                input.form.reportValidity();
                return;
            }

            input.value = getVndDigits(input.value);
        }, true);
    });

    imagePasteBoxes.forEach((box) => setupImagePasteBox(box));
    setupProductVariantEditor(root);
    setupProductDetailVariantPicker(root);
    setupProductGallery(root);
    setupCartActionForms(root);
    setupCartQuantityInputs(root);

    const addToCartForms = root.querySelectorAll(".add-cart-form, .detail-cart-form");
    addToCartForms.forEach((form) => {
        if (form.dataset.cartReady === "true") {
            return;
        }

        form.dataset.cartReady = "true";

        form.addEventListener("submit", async (event) => {
            event.preventDefault();

            const formData = new FormData(form);
            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: formData,
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                });

                if (!response.ok) {
                    HTMLFormElement.prototype.submit.call(form);
                    return;
                }

                const data = await response.json();
                if (data?.message) {
                    showAlert(form, data.message);
                }

                if (Number.isFinite(data?.count)) {
                    const cartCount = document.querySelector(".cart-count");
                    if (cartCount) {
                        cartCount.textContent = data.count;
                    }
                }
            } catch {
                HTMLFormElement.prototype.submit.call(form);
            }
        });
    });
};

const setupSmoothNavigation = () => {
    const pageContent = document.querySelector("#page-content");
    if (!pageContent) {
        return;
    }

    document.addEventListener("click", async (event) => {
        const link = event.target.closest(".site-header a[href], .site-footer a[href]");
        if (!link || !shouldHandleSmoothNavigation(link)) {
            return;
        }

        const targetUrl = new URL(link.href, window.location.href);

        if (targetUrl.pathname === window.location.pathname && targetUrl.search === window.location.search && targetUrl.hash) {
            event.preventDefault();
            scrollToHash(targetUrl.hash);
            history.replaceState({ smoothNavigation: true }, "", targetUrl);
            updateActiveNavigation(targetUrl);
            collapseMobileMenu();
            return;
        }

        event.preventDefault();
        await navigateSmoothly(targetUrl, true);
    });

    window.addEventListener("popstate", () => {
        navigateSmoothly(new URL(window.location.href), false);
    });

    updateActiveNavigation(new URL(window.location.href));
};

const shouldHandleSmoothNavigation = (link) => {
    const targetUrl = new URL(link.href, window.location.href);

    return link.origin === window.location.origin
        && link.target !== "_blank"
        && !link.hasAttribute("download")
        && !link.closest("form")
        && (targetUrl.pathname !== window.location.pathname || targetUrl.search !== window.location.search || Boolean(targetUrl.hash));
};

const navigateSmoothly = async (targetUrl, pushState) => {
    const pageContent = document.querySelector("#page-content");
    if (!pageContent || document.body.classList.contains("is-page-navigating")) {
        return;
    }

    document.body.classList.add("is-page-navigating");
    collapseMobileMenu();

    try {
        const response = await fetch(targetUrl, {
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        if (!response.ok) {
            window.location.assign(targetUrl);
            return;
        }

        const html = await response.text();
        const nextDocument = new DOMParser().parseFromString(html, "text/html");
        const nextContent = nextDocument.querySelector("#page-content");

        if (!nextContent) {
            window.location.assign(targetUrl);
            return;
        }

        await waitForContentFade();
        pageContent.innerHTML = nextContent.innerHTML;
        updateSharedShell(nextDocument);
        document.title = nextDocument.title;

        if (pushState) {
            history.pushState({ smoothNavigation: true }, "", targetUrl);
        }

        await ensurePageScripts(nextDocument);
        initializePage(pageContent);
        refreshUnobtrusiveValidation(pageContent);
        updateActiveNavigation(targetUrl);

        if (targetUrl.hash) {
            scrollToHash(targetUrl.hash);
        } else {
            window.scrollTo({ top: 0, behavior: "smooth" });
            pageContent.focus({ preventScroll: true });
        }
    } catch {
        window.location.assign(targetUrl);
    } finally {
        document.body.classList.remove("is-page-navigating");
    }
};

const waitForContentFade = () => new Promise((resolve) => {
    window.setTimeout(resolve, 120);
});

const scrollToHash = (hash) => {
    const target = hash ? document.querySelector(hash) : null;

    if (target) {
        target.scrollIntoView({ behavior: "smooth", block: "start" });
    }
};

const updateActiveNavigation = (url) => {
    document.querySelectorAll(".site-header a[href]").forEach((link) => {
        const linkUrl = new URL(link.href, window.location.href);
        const isSamePage = linkUrl.pathname === url.pathname && linkUrl.search === url.search;
        const isActive = isSamePage && (url.hash ? linkUrl.hash === url.hash : !linkUrl.hash);

        link.classList.toggle("is-active", isActive);
        if (isActive) {
            link.setAttribute("aria-current", "page");
        } else {
            link.removeAttribute("aria-current");
        }
    });
};

const updateSharedShell = (nextDocument) => {
    const currentHeader = document.querySelector(".site-header");
    const nextHeader = nextDocument.querySelector(".site-header");
    const currentFooter = document.querySelector(".site-footer");
    const nextFooter = nextDocument.querySelector(".site-footer");

    if (currentHeader && nextHeader) {
        currentHeader.innerHTML = nextHeader.innerHTML;
    }

    if (currentFooter && nextFooter) {
        currentFooter.innerHTML = nextFooter.innerHTML;
    }
};

const ensurePageScripts = async (nextDocument) => {
    const scripts = Array.from(nextDocument.querySelectorAll("script[src]"))
        .filter((script) => /jquery\.validate|jquery\.validate\.unobtrusive/i.test(script.src));

    for (const script of scripts) {
        await loadScriptOnce(script.src);
    }
};

const loadScriptOnce = (src) => new Promise((resolve, reject) => {
    const scriptUrl = new URL(src, window.location.href).href;
    const existingScript = Array.from(document.scripts)
        .find((script) => new URL(script.src, window.location.href).href === scriptUrl);

    if (existingScript) {
        resolve();
        return;
    }

    const script = document.createElement("script");
    script.src = scriptUrl;
    script.onload = resolve;
    script.onerror = reject;
    document.body.appendChild(script);
});

const collapseMobileMenu = () => {
    const menu = document.querySelector("#mainMenu.show");
    const bootstrapCollapse = window.bootstrap?.Collapse;

    if (menu && bootstrapCollapse) {
        bootstrapCollapse.getOrCreateInstance(menu).hide();
    }
};

const refreshUnobtrusiveValidation = (root) => {
    if (!window.jQuery?.validator?.unobtrusive) {
        return;
    }

    window.jQuery(root).find("form").each((_index, form) => {
        window.jQuery(form).removeData("validator");
        window.jQuery(form).removeData("unobtrusiveValidation");
        window.jQuery.validator.unobtrusive.parse(form);
    });
};

const movePageAlertsToToastContainer = (root = document) => {
    const pageAlerts = root.querySelectorAll(".cart-alert:not(.toast-alert)");
    if (!pageAlerts.length) {
        return;
    }

    const container = getToastContainer();
    pageAlerts.forEach((alert) => {
        alert.classList.add("toast-alert");
        container.appendChild(alert);
    });
};

const dismissAlert = (alert) => {
    if (!alert || alert.classList.contains("is-dismissed")) {
        return;
    }

    alert.classList.add("is-dismissed");

    const removeAlert = () => {
        if (alert.parentElement) {
            alert.remove();
        }
    };

    alert.addEventListener("transitionend", removeAlert, { once: true });
    window.setTimeout(removeAlert, 250);
};

const getVndDigits = (value) => {
    const currencyValue = value.replace(/\s*(VND|₫)\s*$/i, "").trim();
    const wholeNumber = currencyValue.includes(".") && !currencyValue.includes(",")
        ? currencyValue.split(".")[0]
        : currencyValue;

    return wholeNumber.replace(/\D/g, "");
};

const formatVndInput = (input) => {
    const digits = getVndDigits(input.value);
    input.value = digits ? `${Number.parseInt(digits, 10).toLocaleString("vi-VN")} ₫` : "";
    moveCaretBeforeVnd(input);
};

const validateVndInput = (input) => {
    const digits = getVndDigits(input.value);
    const value = digits ? Number.parseInt(digits, 10) : null;
    const min = input.dataset.vndMin ? Number.parseInt(input.dataset.vndMin, 10) : null;
    const max = input.dataset.vndMax ? Number.parseInt(input.dataset.vndMax, 10) : null;

    input.setCustomValidity("");

    if (input.required && value === null) {
        input.setCustomValidity("Vui lòng nhập giá bán.");
        return false;
    }

    if (value === null) {
        return true;
    }

    if (min !== null && value < min) {
        input.setCustomValidity(`Giá bán phải từ ${min.toLocaleString("vi-VN")} ₫ trở lên.`);
        return false;
    }

    if (max !== null && value > max) {
        input.setCustomValidity(`Giá bán không được vượt quá ${max.toLocaleString("vi-VN")} ₫.`);
        return false;
    }

    return true;
};

const moveCaretBeforeVnd = (input) => {
    window.requestAnimationFrame(() => {
        const suffixStart = input.value.indexOf(" ₫");
        const caretPosition = suffixStart === -1 ? input.value.length : suffixStart;
        input.setSelectionRange(caretPosition, caretPosition);
    });
};
const setupImagePasteBox = (box) => {
    if (box.dataset.imagePasteReady === "true") {
        return;
    }

    const form = box.closest("form");
    const imageValue = form?.querySelector(".js-image-value");
    const preview = box.querySelector(".js-image-preview");
    const fileInput = box.querySelector(".js-image-file");
    const fileButton = box.querySelector(".image-file-btn");

    if (!imageValue || !preview) {
        return;
    }

    box.dataset.imagePasteReady = "true";

    const setImage = (file) => {
        if (!file?.type?.startsWith("image/")) {
            return;
        }

        const reader = new FileReader();
        reader.addEventListener("load", () => {
            imageValue.value = reader.result;
            preview.src = reader.result;

            const editPreviewImage = document.querySelector(".js-edit-preview-image");
            if (editPreviewImage) {
                editPreviewImage.src = reader.result;
            }

            box.classList.add("has-image");
        });
        reader.readAsDataURL(file);
    };

    box.addEventListener("paste", (event) => {
        const imageFile = Array.from(event.clipboardData?.files ?? [])
            .find((file) => file.type.startsWith("image/"));

        if (imageFile) {
            event.preventDefault();
            setImage(imageFile);
        }
    });

    box.addEventListener("click", () => box.focus());

    fileButton?.addEventListener("click", () => fileInput?.click());
    fileInput?.addEventListener("change", () => setImage(fileInput.files?.[0]));
};

const setupProductVariantEditor = (root = document) => {
    const editors = root.querySelectorAll(".js-variant-editor");

    editors.forEach((editor) => {
        if (editor.dataset.variantEditorReady === "true") {
            return;
        }

        const tableBody = editor.querySelector(".js-variant-table tbody");
        const template = editor.querySelector(".js-variant-template");
        const addButton = editor.querySelector(".js-add-variant");

        if (!tableBody || !template || !addButton) {
            return;
        }

        editor.dataset.variantEditorReady = "true";

        const reindexRows = () => {
            tableBody.querySelectorAll(".js-variant-row").forEach((row, index) => {
                row.querySelectorAll("[name]").forEach((field) => {
                    field.name = field.name.replace(/Variants\[\d+\]/, `Variants[${index}]`);
                });
            });
        };

        const bindRemoveButtons = () => {
            tableBody.querySelectorAll(".js-remove-variant").forEach((button) => {
                if (button.dataset.removeReady === "true") {
                    return;
                }

                button.dataset.removeReady = "true";
                button.addEventListener("click", () => {
                    const rows = tableBody.querySelectorAll(".js-variant-row");
                    if (rows.length <= 1) {
                        const row = button.closest(".js-variant-row");
                        row?.querySelectorAll("input, select").forEach((field) => {
                            if (field.type !== "hidden") {
                                field.value = "";
                            }
                        });
                        return;
                    }

                    button.closest(".js-variant-row")?.remove();
                    reindexRows();
                });
            });
        };

        addButton.addEventListener("click", () => {
            const nextIndex = tableBody.querySelectorAll(".js-variant-row").length;
            const html = template.innerHTML.replaceAll("__index__", nextIndex.toString());
            tableBody.insertAdjacentHTML("beforeend", html);
            bindRemoveButtons();
        });

        bindRemoveButtons();
        reindexRows();
    });
};

const setupProductDetailVariantPicker = (root = document) => {
    const detailBlocks = root.querySelectorAll(".js-product-detail");

    detailBlocks.forEach((block) => {
        if (block.dataset.variantPickerReady === "true") {
            return;
        }

        let variants = [];
        try {
            variants = JSON.parse(block.dataset.variants || "[]");
        } catch {
            variants = [];
        }

        const sizeSelect = block.querySelector(".js-size-select");
        const colorSelect = block.querySelector(".js-color-select");
        const variantInput = block.querySelector(".js-selected-variant");
        const skuText = block.querySelector(".js-detail-sku");
        const stockText = block.querySelector(".js-variant-stock");
        const stockSummary = block.querySelector(".js-detail-stock");
        const priceText = block.querySelector(".js-detail-price");
        const addButton = block.querySelector(".js-add-selected-variant");

        if (!sizeSelect || !colorSelect || !variantInput) {
            return;
        }

        block.dataset.variantPickerReady = "true";

        const syncPills = () => {
            block.querySelectorAll(".js-size-pill").forEach((pill) => {
                pill.classList.toggle("is-selected", pill.dataset.value === sizeSelect.value);
            });
            block.querySelectorAll(".js-color-pill").forEach((pill) => {
                pill.classList.toggle("is-selected", pill.dataset.value === colorSelect.value);
            });
        };

        const updateVariant = () => {
            const sizeId = Number.parseInt(sizeSelect.value, 10);
            const colorId = Number.parseInt(colorSelect.value, 10);
            const selected = variants.find((variant) => variant.sizeId === sizeId && variant.colorId === colorId);

            if (!selected) {
                variantInput.value = "";
                if (skuText) skuText.textContent = "-";
                if (stockText) stockText.textContent = "0";
                if (stockSummary) stockSummary.textContent = "Vui lòng chọn kích cỡ và màu sắc";
                if (addButton) addButton.disabled = true;
                syncPills();
                return;
            }

            variantInput.value = selected.id;
            if (skuText) skuText.textContent = selected.sku;
            if (stockText) stockText.textContent = selected.stock;
            if (stockSummary) stockSummary.textContent = selected.stock > 0 ? `Tồn kho: ${selected.stock}` : "Sản phẩm đã hết hàng";
            if (priceText) priceText.textContent = selected.price;
            if (addButton) addButton.disabled = selected.stock <= 0;
            syncPills();
        };

        block.querySelectorAll(".js-size-pill").forEach((pill) => {
            pill.addEventListener("click", () => {
                sizeSelect.value = pill.dataset.value;
                updateVariant();
            });
        });

        block.querySelectorAll(".js-color-pill").forEach((pill) => {
            pill.addEventListener("click", () => {
                colorSelect.value = pill.dataset.value;
                updateVariant();
            });
        });

        sizeSelect.addEventListener("change", updateVariant);
        colorSelect.addEventListener("change", updateVariant);
        updateVariant();
    });
};
const setupProductGallery = (root = document) => {
    const thumbs = root.querySelectorAll(".js-gallery-thumb");

    thumbs.forEach((thumb) => {
        if (thumb.dataset.galleryReady === "true") {
            return;
        }

        thumb.dataset.galleryReady = "true";
        thumb.addEventListener("click", () => {
            const gallery = thumb.closest(".product-gallery");
            const mainImage = gallery?.querySelector(".js-gallery-main");
            const imageUrl = thumb.dataset.imageUrl;
            if (mainImage && imageUrl) {
                mainImage.src = imageUrl;
            }
        });
    });
};

const setupCartActionForms = (root = document) => {
    const forms = root.querySelectorAll(".js-cart-action-form");

    forms.forEach((form) => {
        if (form.dataset.cartActionReady === "true") {
            return;
        }

        form.dataset.cartActionReady = "true";

        form.addEventListener("submit", async (event) => {
            event.preventDefault();

            if (form.classList.contains("is-loading")) {
                return;
            }

            const cartItem = form.closest(".js-cart-item");
            const cartPage = form.closest(".js-cart-page");
            const submitButtons = cartItem?.querySelectorAll("button") ?? form.querySelectorAll("button");

            form.classList.add("is-loading");
            cartItem?.classList.add("is-updating");
            submitButtons.forEach((button) => button.disabled = true);

            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: new FormData(form),
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                });

                if (!response.ok) {
                    HTMLFormElement.prototype.submit.call(form);
                    return;
                }

                const data = await response.json();
                updateCartUi(data, cartPage);
            } catch {
                HTMLFormElement.prototype.submit.call(form);
            } finally {
                form.classList.remove("is-loading");
                cartItem?.classList.remove("is-updating");
                submitButtons.forEach((button) => button.disabled = false);
            }
        });
    });
};

const setupCartQuantityInputs = (root = document) => {
    const inputs = root.querySelectorAll(".js-cart-qty-input");

    inputs.forEach((input) => {
        if (input.dataset.quantityReady === "true") {
            return;
        }

        input.dataset.quantityReady = "true";
        input.dataset.lastValue = input.value;

        input.addEventListener("change", () => submitQuantityInput(input));
        input.addEventListener("keydown", (event) => {
            if (event.key === "Enter") {
                event.preventDefault();
                input.blur();
                submitQuantityInput(input);
            }
        });
    });
};

const submitQuantityInput = (input) => {
    const value = Number.parseInt(input.value, 10);
    const normalizedValue = Number.isFinite(value) ? Math.min(Math.max(value, 0), 99) : 1;
    input.value = normalizedValue.toString();

    if (input.dataset.lastValue === input.value) {
        return;
    }

    input.dataset.lastValue = input.value;
    input.closest("form")?.requestSubmit();
};

const updateCartUi = (data, cartPage) => {
    if (!data) {
        return;
    }

    if (data.message) {
        showAlert(null, data.message);
    }

    const cartCount = document.querySelector(".cart-count");
    if (cartCount && Number.isFinite(data.count)) {
        cartCount.textContent = data.count;
    }

    if (data.summary) {
        setText(".js-summary-subtotal", data.summary.subTotal);
        setText(".js-summary-shipping", data.summary.shippingFee);
        setText(".js-summary-discount", data.summary.discount);
        setText(".js-summary-total", data.summary.total);
    }

    const productId = data.changedProductId?.toString();
    const row = productId ? document.querySelector(`.js-cart-item[data-product-id="${productId}"]`) : null;

    if (data.item && row) {
        const quantityInput = row.querySelector(".js-cart-qty-input");
        const lineTotal = row.querySelector(".js-line-total");

        if (quantityInput) {
            quantityInput.value = data.item.quantity;
            quantityInput.dataset.lastValue = data.item.quantity.toString();
        }

        if (lineTotal) {
            lineTotal.textContent = data.item.lineTotal;
            lineTotal.classList.add("is-value-updated");
            window.setTimeout(() => lineTotal.classList.remove("is-value-updated"), 280);
        }

        return;
    }

    if (row) {
        row.classList.add("is-removing");
        window.setTimeout(() => {
            row.remove();
            if (data.isEmpty || !cartPage?.querySelector(".js-cart-item")) {
                window.location.reload();
            }
        }, 180);
        return;
    }

    if (data.isEmpty) {
        window.location.reload();
    }
};

const setText = (selector, value) => {
    const element = document.querySelector(selector);
    if (element && value !== undefined && value !== null) {
        element.textContent = value;
        element.classList.add("is-value-updated");
        window.setTimeout(() => element.classList.remove("is-value-updated"), 280);
    }
};

const showAlert = (_form, message) => {
    const container = getToastContainer();
    const alert = document.createElement("div");
    alert.className = "alert toast-alert";
    alert.setAttribute("role", "alert");
    alert.innerHTML = `
        <i class="fa-solid fa-circle-check"></i>
        <span>${message}</span>
        <button type="button" class="alert-close" aria-label="Đóng thông báo">
            <i class="fa-solid fa-xmark"></i>
        </button>
    `;

    const closeButton = alert.querySelector(".alert-close");
    if (closeButton) {
        closeButton.addEventListener("click", () => dismissAlert(alert));
    }

    container.appendChild(alert);
    window.setTimeout(() => dismissAlert(alert), 3000);
};

const getToastContainer = () => {
    let container = document.querySelector(".toast-container");
    if (!container) {
        container = document.createElement("div");
        container.className = "toast-container";
        document.body.appendChild(container);
    }

    return container;
};

