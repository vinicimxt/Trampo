// =============================
// CAMPOS DINÂMICOS (cadastro)
// =============================
function mostrarCampos() {
    var tipo = document.getElementById("tipo").value;
    var campos = document.getElementById("camposProfissional");

    if (campos) {
        campos.style.display = (tipo === "profissional") ? "block" : "none";
    }
}

// =============================
// NAVBAR / MENU MOBILE
// =============================
const menuToggle = document.getElementById("menuToggle");
const navMenu = document.getElementById("navLinks");

if (menuToggle && navMenu) {
    menuToggle.addEventListener("click", () => {
        navMenu.classList.toggle("active");
    });
}

// =============================
// SCROLL EFFECT
// =============================
const navbar = document.getElementById("navbar");

if (navbar) {
    window.addEventListener("scroll", () => {
        navbar.classList.toggle("scrolled", window.scrollY > 20);
    });
}

// =============================
// NAV INDICATOR (hover)
// =============================
const items = document.querySelectorAll(".nav-item");
const navLinks = document.querySelector(".nav-links");

if (items.length && navLinks) {
    const indicator = document.createElement("div");
    indicator.classList.add("nav-indicator");
    navLinks.appendChild(indicator);

    items.forEach(item => {
        item.addEventListener("mouseenter", () => {
            const rect = item.getBoundingClientRect();
            const parentRect = navLinks.getBoundingClientRect();

            indicator.style.width = rect.width + "px";
            indicator.style.left = (rect.left - parentRect.left) + "px";
        });
    });

    navLinks.addEventListener("mouseleave", () => {
        indicator.style.width = "0";
    });
}