
// =============================
// BOTAO HOME
// ============================


document.addEventListener("DOMContentLoaded", () => {

    const btnTop = document.getElementById("btnTop");
    const navbar = document.getElementById("navbar");
    const topbar = document.getElementById("topbar");

    let ticking = false;

    window.addEventListener("scroll", () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {

                const scrollY = window.scrollY;

                if (scrollY > 80) {

                    if (topbar) {
                        topbar.classList.add("opacity-0", "h-0", "overflow-hidden");
                    }

                    if (navbar) {
                        navbar.classList.add("scrolled");
                        // REMOVE o blur daqui se quiser performance
                        navbar.classList.add("shadow-lg");
                    }

                } else {

                    if (topbar) {
                        topbar.classList.remove("opacity-0", "h-0", "overflow-hidden");
                    }

                    if (navbar) {
                        navbar.classList.remove("scrolled");
                    }
                }

                ticking = false;

            });

            ticking = true;
        }
    });
    // BOTÃO
    if (btnTop) {
        if (scrollY > window.innerHeight * 0.5) {
            btnTop.classList.add("show");
        } else {
            btnTop.classList.remove("show");
        }
    }

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

// TOAST All Pages
function showToast(message, type = "success") {
    const container = document.getElementById("toast-container");

    if (!container) return;

    const icons = {
        success: "✅",
        error: "⚠️",
        info: "ℹ️"
    };

    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;

    toast.innerHTML = `
        <span class="toast-icon">${icons[type] || "ℹ️"}</span>
        <span class="toast-text">${message}</span>
        <span class="toast-close">&times;</span>
    `;

    container.appendChild(toast);

    // fechar manual
    toast.querySelector(".toast-close").addEventListener("click", () => {
        removeToast(toast);
    });

    // auto remove
    setTimeout(() => {
        removeToast(toast);
    }, 3500);
}

function removeToast(toast) {
    toast.style.animation = "toastOut 0.3s ease forwards";
    setTimeout(() => toast.remove(), 300);
}

document.addEventListener("DOMContentLoaded", () => {

    const body = document.body;

    const sucesso = body.dataset.toastSucesso;
    const erro = body.dataset.toastErro;

    if (sucesso) {
        showToast(sucesso, "success");
    }

    if (erro) {
        showToast(erro, "error");
    }

});

// =================================
// DROPDOWN
// =================================

const toggle = document.getElementById("notifToggle");
const dropdown = document.getElementById("notifDropdown");
const list = document.getElementById("notifList");

let carregado = false;

if (toggle && dropdown && list) {
    toggle.addEventListener("click", () => {
        dropdown.classList.toggle("open");
        // limpa badge ao abrir
        const badge = toggle.querySelector(".notif-badge");
        if (badge) badge.remove();

        if (!carregado) {
            fetch('/Notificacao/Ultimas')
                .then(res => res.text())
                .then(html => {
                    list.innerHTML = html;
                    carregado = true;
                })
                .catch(err => console.error("Erro ao carregar notificações:", err));
        }
    });
}
document.addEventListener("click", function (e) {
    if (!e.target.closest(".notif-wrapper")) {
        const dropdown = document.getElementById("notifDropdown");
        if (dropdown) dropdown.classList.remove("open");
    }
});

function atualizarContador() {
    fetch('/Notificacao/Contador')
        .then(res => res.text())
        .then(qtd => {

            const toggle = document.getElementById("notifToggle");
            let badge = toggle.querySelector(".notif-badge");

            if (qtd > 0) {
                // cria se não existir
                if (!badge) {
                    badge = document.createElement("span");
                    badge.classList.add("notif-badge");
                    toggle.appendChild(badge);
                }

                badge.textContent = qtd;
            } else {
                // remove se zerou
                if (badge) badge.remove();
            }
        });
}

setInterval(atualizarContador, 10000);

async function abrirNotificacao(e, el) {
    e.preventDefault();

    const notifId = el.dataset.id;
    const refId = el.dataset.ref;

    if (!refId || refId === "0") {
        console.warn("Notificação sem referência válida");
        return;
    }
    try {
        await fetch('/Notificacao/MarcarComoLidaAjax', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id: notifId })
        });

        const res = await fetch('/Agendamento/DetalhesModal/' + refId);

        if (!res.ok) {
            console.error("Erro ao buscar modal:", res.status);
            return;
        }

        const html = await res.text();

        const modal = document.getElementById("modalGlobal");
        modal.querySelector(".modal-body").innerHTML = html;
        modal.classList.add("open");

        el.classList.remove("nao-lida");

        atualizarContador();

    } catch (err) {
        console.error("Erro ao abrir notificação", err);
    }
}

// =============================
// MODAL GLOBAL
// =============================

function abrirModal(id) {
    const modal = document.getElementById(id);
    if (!modal) return;

    modal.classList.add("open");
}

function fecharModal(id) {
    const modal = document.getElementById(id);
    if (!modal) return;

    modal.classList.remove("open");
}

// fecha clicando fora
document.addEventListener("click", function (e) {
    if (e.target.classList.contains("modal-overlay")) {
        e.target.classList.remove("open");
    }
});

// ESC fecha tudo
document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
        document.querySelectorAll(".modal-overlay.open")
            .forEach(m => m.classList.remove("open"));
    }
});

sections.forEach(section => observer.observe(section));
