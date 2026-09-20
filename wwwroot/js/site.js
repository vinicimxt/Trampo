
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
                    }

                } else {

                    if (topbar) {
                        topbar.classList.remove("opacity-0", "h-0", "overflow-hidden");
                    }

                    if (navbar) {
                        navbar.classList.remove("scrolled");
                    }

                }

                // BOTÃO TOP
                if (btnTop) {

                    if (scrollY > window.innerHeight * 0.5) {
                        btnTop.classList.add("show");
                    } else {
                        btnTop.classList.remove("show");
                    }

                }

                ticking = false;

            });

            ticking = true;
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
    toast.setAttribute("role", type === "error" ? "alert" : "status");
    toast.className = `toast toast-${type}`;

    for (const [classe, texto] of [
        ["toast-icon", icons[type] || "ℹ️"],
        ["toast-text", message],
        ["toast-close", "×"]]) {
        const span = document.createElement("span");
        span.className = classe;
        span.textContent = texto;
        toast.appendChild(span);
    }
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
    toggle.addEventListener("keydown", e => {
        if (e.key === "Enter" || e.key === " ") { e.preventDefault(); toggle.click(); }
        if (e.key === "Escape") { dropdown.classList.remove("open"); toggle.setAttribute("aria-expanded", "false"); }
    });
    toggle.addEventListener("click", () => {
        dropdown.classList.toggle("open");
        toggle.setAttribute("aria-expanded", dropdown.classList.contains("open"));

        if (dropdown.classList.contains("open")) {
            fetch('/Notificacao/Ultimas')
                .then(res => {
                    if (!res.ok || res.redirected) throw new Error("Sessão expirada ou falha ao carregar notificações.");
                    return res.text();
                })
                .then(html => {
                    list.innerHTML = html;
                    carregado = true;
                })
                .catch(() => { list.textContent = "Não foi possível carregar as notificações. Tente abrir novamente."; });
        }
    });
}
document.addEventListener("click", function (e) {
    if (!e.target.closest(".notif-wrapper")) {
        const dropdown = document.getElementById("notifDropdown");
        if (dropdown) dropdown.classList.remove("open");
        toggle?.setAttribute("aria-expanded", "false");
    }
});

function atualizarContador() {
    if (!document.getElementById("notifToggle")) return;
    fetch('/Notificacao/Contador')
        .then(res => {
                    if (!res.ok || res.redirected) throw new Error("Sessão expirada ou falha ao carregar notificações.");
                    return res.text();
                })
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
        }).catch(() => { /* Mantém o último contador em falhas temporárias. */ });
}

setInterval(atualizarContador, 10000);


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
        document.body.style.overflow = "";
    }
});

// ESC fecha tudo
document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
        document.querySelectorAll(".modal-overlay.open")
            .forEach(m => m.classList.remove("open"));
        document.body.style.overflow = "";
    }
});

sections.forEach(section => observer.observe(section));

// Operações MVC que antes eram links GET agora enviam formulário POST com token.
function enviarPost(url) {
    const destino = new URL(url, window.location.origin);
    if (destino.origin !== window.location.origin) return;
    const token = document.querySelector('meta[name="csrf-token"]')?.content;
    if (!token) { showToast("Recarregue a página para continuar.", "error"); return; }
    const form = document.createElement("form");
    form.method = "post";
    form.action = destino.pathname + destino.search;
    const input = document.createElement("input");
    input.type = "hidden";
    input.name = "__RequestVerificationToken";
    input.value = token;
    form.appendChild(input);
    document.body.appendChild(form);
    form.submit();
}

document.addEventListener("click", function (event) {
    const link = event.target.closest("a[href]");
    if (!link) return;
    const url = new URL(link.href, window.location.origin);
    if (url.origin !== window.location.origin) return;
    if (/^\/(Usuario\/Logout|Local\/Excluir|Servico\/(Excluir|Desativar)|Agendamento\/(Confirmar|Recusar|ConfirmarCliente)|Notificacao\/(Abrir|MarcarComoLida))(\/\d+)?\/?$/i.test(url.pathname)) {
        event.preventDefault();
        enviarPost(url.href);
    }
});
// Os modais atuais compartilham a classe open; mantém foco e navegação por teclado.
document.addEventListener("DOMContentLoaded", () => {
    const modais = Array.from(document.querySelectorAll(".modal-overlay"));
    const acionadores = new WeakMap();
    const abertos = new WeakSet();
    const focaveis = modal => Array.from(modal.querySelectorAll(
        'button:not(:disabled), a[href], input:not(:disabled):not([type="hidden"]), select:not(:disabled), textarea:not(:disabled), [tabindex="0"]'
    )).filter(el => el.getClientRects().length > 0);
    modais.forEach(modal => {
        modal.setAttribute("role", "dialog");
        modal.setAttribute("aria-modal", "true");
        modal.setAttribute("tabindex", "-1");
        const titulo = modal.querySelector(".modal-title, h3, .drawer-title");
        modal.setAttribute("aria-label", titulo?.textContent.trim() || "Confirmação");
        new MutationObserver(() => {
            const aberto = modal.classList.contains("open");
            if (aberto && !abertos.has(modal)) {
                acionadores.set(modal, document.activeElement);
                abertos.add(modal);
                (focaveis(modal)[0] || modal).focus();
            } else if (!aberto && abertos.has(modal)) {
                abertos.delete(modal);
                if (!modais.some(m => m.classList.contains("open"))) {
                    document.body.style.overflow = "";
                    acionadores.get(modal)?.focus();
                }
            }
        }).observe(modal, { attributes: true, attributeFilter: ["class"] });
    });
    document.addEventListener("keydown", e => {
        const modal = modais.find(m => m.classList.contains("open"));
        if (!modal || e.key !== "Tab") return;
        const elementos = focaveis(modal);
        const primeiro = elementos[0], ultimo = elementos[elementos.length - 1];
        if (!primeiro) { e.preventDefault(); modal.focus(); return; }
        if (e.shiftKey && (document.activeElement === primeiro || !modal.contains(document.activeElement))) {
            e.preventDefault(); ultimo.focus();
        } else if (!e.shiftKey && (document.activeElement === ultimo || !modal.contains(document.activeElement))) {
            e.preventDefault(); primeiro.focus();
        }
    });
});