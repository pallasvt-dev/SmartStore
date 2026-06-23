document.addEventListener("DOMContentLoaded", () => {
    const body = document.body;
    const toggle = document.querySelector(".admin-sidebar-toggle");
    const overlay = document.querySelector(".sidebar-overlay");
    const sidebarLinks = document.querySelectorAll(".admin-sidebar a");

    const setSidebar = (isOpen) => {
        body.classList.toggle("admin-sidebar-open", isOpen);
        toggle?.setAttribute("aria-expanded", isOpen ? "true" : "false");
    };

    toggle?.addEventListener("click", () => setSidebar(!body.classList.contains("admin-sidebar-open")));
    overlay?.addEventListener("click", () => setSidebar(false));
    sidebarLinks.forEach((link) => link.addEventListener("click", () => setSidebar(false)));
    window.addEventListener("keydown", (event) => {
        if (event.key === "Escape") setSidebar(false);
    });

    document.querySelectorAll(".js-admin-confirm").forEach((form) => {
        form.addEventListener("submit", (event) => {
            const message = form.dataset.confirmMessage
                || "Bạn có chắc chắn muốn thực hiện thao tác này không? Thao tác này có thể ảnh hưởng đến dữ liệu đang hiển thị.";
            if (!window.confirm(message)) event.preventDefault();
        });
    });
});
