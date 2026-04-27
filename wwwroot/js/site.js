
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
                navbar.classList.add("scrolled");
                navbar.classList.add("bg-black/80", "backdrop-blur-md", "shadow-lg");
            }

            document.body.style.paddingTop = navbar ? navbar.offsetHeight + "px" : "0px";

        } else {

            if (topbar) {
                topbar.classList.remove("opacity-0", "h-0", "overflow-hidden");
            }

            if (navbar) {
                navbar.classList.remove("scrolled");
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
// NAV ACTIVE ON SCROLL
// =============================
const sections = document.querySelectorAll("section[id]");
const navItems = document.querySelectorAll(".nav-item");

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {

            const id = entry.target.getAttribute("id");

            navItems.forEach(link => {
                link.classList.remove("active");

                if (link.getAttribute("href")?.includes("#" + id)) {
                    link.classList.add("active");
                }
            });
        }
    });
}, {
    threshold: 0.6
});

sections.forEach(section => observer.observe(section));
