// SUBCATEGORIAS DINÂMICAS
document.getElementById("categoria").addEventListener("change", function () {
    let categoriaId = this.value;

    fetch('/Servico/GetSubcategorias?categoriaId=' + categoriaId)
        .then(res => res.json())
        .then(data => {
            let sub = document.getElementById("subcategoria");

            sub.innerHTML = '<option value="">Selecione a subcategoria</option>';

            data.forEach(s => {
                sub.innerHTML += `<option value="${s.id}">${s.nome}</option>`;
            });
        });
});

// CONTROLE DE CAMPOS
function toggleCampos() {
    let tipo = document.getElementById("atendimento").value.toLowerCase();

    let linkDiv = document.getElementById("linkOnlineDiv");
    let localDiv = document.getElementById("localDiv");

    linkDiv.classList.toggle("hidden", tipo !== "online");
    localDiv.classList.toggle("hidden", tipo !== "local");
}

// EVENTOS
document.getElementById("atendimento").addEventListener("change", toggleCampos);

document.addEventListener("DOMContentLoaded", function () {
    toggleCampos();
});

// ===============================
// VALIDAÇÃO NOME
// ===============================
const nome = document.getElementById("nome");
const nomeMsg = document.getElementById("nomeMsg");

nome.addEventListener("input", () => {
    if (nome.value.length < 3) {
        nome.classList.add("input-error");
        nome.classList.remove("input-valid");
        nomeMsg.innerText = "Mínimo 3 caracteres";
    } else {
        nome.classList.remove("input-error");
        nome.classList.add("input-valid");
        nomeMsg.innerText = "";
    }
});

// ===============================
// CONTADOR DESCRIÇÃO
// ===============================
const desc = document.getElementById("descricao");
const contador = document.getElementById("contador");

desc.addEventListener("input", () => {
    contador.innerText = desc.value.length;
});

// ===============================
// PREVIEW AO VIVO
// ===============================
const previewNome = document.getElementById("previewNome");
const previewDesc = document.getElementById("previewDesc");
const previewCategoria = document.getElementById("previewCategoria");
const previewAtendimento = document.getElementById("previewAtendimento");

nome.addEventListener("input", () => {
    previewNome.innerText = nome.value || "Nome do serviço";
});

desc.addEventListener("input", () => {
    previewDesc.innerText = desc.value || "Descrição aparecerá aqui...";
});

document.getElementById("categoria").addEventListener("change", function () {
    let texto = this.options[this.selectedIndex].text;
    previewCategoria.innerText = texto;
});

document.getElementById("atendimento").addEventListener("change", function () {
    previewAtendimento.innerText = this.value;
});

// ===============================
// LOADING NO BOTÃO
// ===============================
const form = document.getElementById("formServico");
const btn = document.getElementById("btnSubmit");
const btnText = document.getElementById("btnText");
const loader = document.getElementById("btnLoader");

form.addEventListener("submit", () => {
    btn.disabled = true;
    btnText.innerText = "Criando...";
    loader.classList.remove("hidden");
});

// ===============================
// SUBCATEGORIAS (mantido)
// ===============================
document.getElementById("categoria").addEventListener("change", function () {
    let categoriaId = this.value;

    fetch('/Servico/GetSubcategorias?categoriaId=' + categoriaId)
        .then(res => res.json())
        .then(data => {
            let sub = document.getElementById("subcategoria");

            sub.innerHTML = '<option value="">Selecione a subcategoria</option>';

            data.forEach(s => {
                sub.innerHTML += `<option value="${s.id}">${s.nome}</option>`;
            });
        });
});

// ===============================
// CAMPOS DINÂMICOS
// ===============================
function toggleCampos() {
    let tipo = document.getElementById("atendimento").value.toLowerCase();

    document.getElementById("linkOnlineDiv")
        .classList.toggle("hidden", tipo !== "online");

    document.getElementById("localDiv")
        .classList.toggle("hidden", tipo !== "local");
}

document.getElementById("atendimento")
    .addEventListener("change", toggleCampos);

document.addEventListener("DOMContentLoaded", toggleCampos);