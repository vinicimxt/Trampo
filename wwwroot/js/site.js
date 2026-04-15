
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