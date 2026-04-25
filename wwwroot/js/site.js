
// =============================
// BOTAO HOME
// ============================


document.addEventListener("DOMContentLoaded", () => {

    const btnTop = document.getElementById("btnTop");
    const navbar = document.getElementById("navbar");
    const topbar = document.getElementById("topbar");

    window.addEventListener("scroll", () => {

        const scrollY = window.scrollY;

        // NAVBAR
        if (scrollY > 80) {

            if (topbar) {
                topbar.classList.add("opacity-0", "h-0", "overflow-hidden");
            }

            if (navbar) {
                navbar.classList.add("fixed", "top-0", "left-0", "z-50");
                navbar.classList.add("bg-black/80", "backdrop-blur-md", "shadow-lg");
            }

            document.body.style.paddingTop = navbar ? navbar.offsetHeight + "px" : "0px";

        } else {

            if (topbar) {
                topbar.classList.remove("opacity-0", "h-0", "overflow-hidden");
            }

            if (navbar) {
                navbar.classList.remove("fixed", "top-0", "left-0", "z-50");
                navbar.classList.remove("bg-black/80", "backdrop-blur-md", "shadow-lg");
            }

            document.body.style.paddingTop = "0px";
        }

        // BOTÃO
        if (btnTop) {
            if (scrollY > window.innerHeight * 0.5) {
                btnTop.classList.add("show");
            } else {
                btnTop.classList.remove("show");
            }
        }

    });

    // CLICK
    if (btnTop) {
        btnTop.addEventListener("click", () => {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        });
    }

});

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

