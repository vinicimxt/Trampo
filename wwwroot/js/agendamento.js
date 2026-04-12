console.log("JS carregou");

document.addEventListener("DOMContentLoaded", function () {

    let container = document.getElementById("pagina");

    if (!container) {
        console.log("Elemento #pagina não encontrado");
        return;
    }

    let atendimento = container.getAttribute("data-atendimento");

    console.log("Atendimento:", atendimento);

    verificarAtendimento(atendimento);
});

function verificarAtendimento(tipo) {

    if (!tipo) return;

    tipo = tipo.toLowerCase().trim();

    let enderecoDiv = document.getElementById("enderecoDiv");

    if (!enderecoDiv) return;

    // 🔥 só mostra endereço se for DOMICILIO
    if (tipo.includes("domicilio")) {
        enderecoDiv.style.display = "block";
    } else {
        enderecoDiv.style.display = "none";
    }
}

function selecionarHora(hora, elemento) {
    document.getElementById("horaSelecionada").value = hora;

    // remove seleção anterior
    document.querySelectorAll(".slot").forEach(el => {
        el.classList.remove("selecionado");
    });

    // marca o selecionado
    elemento.classList.add("selecionado");
}

