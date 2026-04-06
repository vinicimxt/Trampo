
function mostrarCampos() {
    var tipo = document.getElementById("tipo").value;
    var campos = document.getElementById("camposProfissional");

    campos.style.display = (tipo === "profissional") ? "block" : "none";
}