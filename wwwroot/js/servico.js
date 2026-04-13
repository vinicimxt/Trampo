function atualizarCampos() {

    let tipo = document.getElementById("atendimento").value;

    let linkDiv = document.getElementById("linkOnlineDiv");
    let enderecoDiv = document.getElementById("enderecoDiv");

    // reset tudo
    if (linkDiv) linkDiv.style.display = "none";
    if (enderecoDiv) enderecoDiv.style.display = "none";

    // regras
    if (tipo === "Online") {
        if (linkDiv) linkDiv.style.display = "block";
    }

    if (tipo === "Domicilio") {
        if (enderecoDiv) enderecoDiv.style.display = "block";
    }
}

// roda ao carregar
document.addEventListener("DOMContentLoaded", function () {
    atualizarCampos();
});