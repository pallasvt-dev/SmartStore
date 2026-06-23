document.addEventListener("DOMContentLoaded", () => {
    initializeAdmin(document);
    setupAdminNavigation();
});

const initializeAdmin = (root = document) => {
    const body = document.body;
    const toggle = document.querySelector(".admin-sidebar-toggle");
    const overlay = document.querySelector(".sidebar-overlay");
    const sidebarLinks = root.querySelectorAll(".admin-sidebar a");

    const setSidebar = (isOpen) => {
        body.classList.toggle("admin-sidebar-open", isOpen);
        toggle?.setAttribute("aria-expanded", isOpen ? "true" : "false");
    };

    if (toggle?.dataset.adminToggleReady !== "true") {
        toggle?.addEventListener("click", () => setSidebar(!body.classList.contains("admin-sidebar-open")));
        if (toggle) toggle.dataset.adminToggleReady = "true";
    }

    if (overlay?.dataset.adminOverlayReady !== "true") {
        overlay?.addEventListener("click", () => setSidebar(false));
        if (overlay) overlay.dataset.adminOverlayReady = "true";
    }

    sidebarLinks.forEach((link) => {
        if (link.dataset.adminLinkReady === "true") {
            return;
        }

        link.dataset.adminLinkReady = "true";
        link.addEventListener("click", () => window.setTimeout(() => setSidebar(false), 0));
    });

    if (body.dataset.adminEscapeReady !== "true") {
        window.addEventListener("keydown", (event) => {
            if (event.key === "Escape") setSidebar(false);
        });
        body.dataset.adminEscapeReady = "true";
    }

    root.querySelectorAll(".js-admin-confirm").forEach((form) => {
        if (form.dataset.adminConfirmReady === "true") {
            return;
        }

        form.dataset.adminConfirmReady = "true";
        form.addEventListener("submit", (event) => {
            const message = form.dataset.confirmMessage
                || "Ban co chac chan muon thuc hien thao tac nay khong? Thao tac nay co the anh huong den du lieu dang hien thi.";
            if (!window.confirm(message)) event.preventDefault();
        });
    });
};

const setupAdminNavigation = () => {
    const content = document.querySelector("#adminContent");
    if (!content) {
        return;
    }

    document.addEventListener("click", async (event) => {
        const link = event.target.closest(".admin-sidebar a[href], .admin-breadcrumb a[href], #adminContent a[href]");
        if (!link || !shouldHandleAdminNavigation(link)) {
            return;
        }

        event.preventDefault();
        await navigateAdmin(new URL(link.href, window.location.href), true);
    });

    window.addEventListener("popstate", () => {
        navigateAdmin(new URL(window.location.href), false);
    });
};

const shouldHandleAdminNavigation = (link) => {
    const targetUrl = new URL(link.href, window.location.href);
    const currentUrl = new URL(window.location.href);

    return link.origin === window.location.origin
        && link.target !== "_blank"
        && !link.hasAttribute("download")
        && !link.closest("form")
        && targetUrl.pathname !== "/Account/Logout"
        && (targetUrl.pathname !== currentUrl.pathname || targetUrl.search !== currentUrl.search || targetUrl.hash !== currentUrl.hash);
};

const navigateAdmin = async (targetUrl, pushState) => {
    const content = document.querySelector("#adminContent");
    if (!content || document.body.classList.contains("admin-is-navigating")) {
        return;
    }

    document.body.classList.add("admin-is-navigating");
    document.body.classList.remove("admin-sidebar-open");
    document.querySelector(".admin-sidebar-toggle")?.setAttribute("aria-expanded", "false");

    try {
        const response = await fetch(targetUrl, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            window.location.assign(targetUrl);
            return;
        }

        const html = await response.text();
        const nextDocument = new DOMParser().parseFromString(html, "text/html");
        const nextContent = nextDocument.querySelector("#adminContent");

        if (!nextContent) {
            window.location.assign(targetUrl);
            return;
        }

        content.innerHTML = nextContent.innerHTML;
        document.title = nextDocument.title;
        updateAdminActiveMenu(nextDocument);

        if (pushState) {
            history.pushState({ adminNavigation: true }, "", targetUrl);
        }

        window.SmartStore?.initializePage?.(content);
        initializeAdmin(content);
        refreshAdminValidation(content);

        if (targetUrl.hash) {
            document.querySelector(targetUrl.hash)?.scrollIntoView({ behavior: "smooth", block: "start" });
        } else {
            window.scrollTo({ top: 0, behavior: "smooth" });
            content.focus({ preventScroll: true });
        }
    } catch {
        window.location.assign(targetUrl);
    } finally {
        document.body.classList.remove("admin-is-navigating");
    }
};

const updateAdminActiveMenu = (nextDocument) => {
    const currentLinks = document.querySelectorAll(".admin-sidebar a[href]");
    const nextLinks = nextDocument.querySelectorAll(".admin-sidebar a[href]");

    currentLinks.forEach((link, index) => {
        const nextLink = nextLinks[index];
        link.classList.toggle("active", nextLink?.classList.contains("active") === true);
    });
};

const refreshAdminValidation = (root) => {
    if (!window.jQuery?.validator?.unobtrusive) {
        return;
    }

    window.jQuery(root).find("form").each((_index, form) => {
        window.jQuery(form).removeData("validator");
        window.jQuery(form).removeData("unobtrusiveValidation");
        window.jQuery.validator.unobtrusive.parse(form);
    });
};
