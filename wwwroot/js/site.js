
function mostrarCampos() {
    var tipo = document.getElementById("tipo").value;
    var campos = document.getElementById("camposProfissional");

    campos.style.display = (tipo === "profissional") ? "block" : "none";
}

const toggle = document.getElementById("menuToggle");
const menu = document.getElementById("navMenu");

toggle.addEventListener("click", () => {
    menu.classList.toggle("active");
});


// SCROLL EFFECT
const navbar = document.getElementById("navbar");

window.addEventListener("scroll", () => {
    navbar.classList.toggle("scrolled", window.scrollY > 20);
});

// MENU MOBILE
const toggle = document.getElementById("menuToggle");
const nav = document.getElementById("navLinks");

toggle.addEventListener("click", () => {
    nav.classList.toggle("active");
});

// NAV INDICATOR
const items = document.querySelectorAll(".nav-item");
const nav = document.querySelector(".nav-links");

if (items.length && nav) {
    const indicator = document.createElement("div");
    indicator.classList.add("nav-indicator");
    nav.appendChild(indicator);

    items.forEach(item => {
        item.addEventListener("mouseenter", () => {
            const rect = item.getBoundingClientRect();
            const parentRect = nav.getBoundingClientRect();

            indicator.style.width = rect.width + "px";
            indicator.style.left = (rect.left - parentRect.left) + "px";
        });
    });

    nav.addEventListener("mouseleave", () => {
        indicator.style.width = "0";
    });
}