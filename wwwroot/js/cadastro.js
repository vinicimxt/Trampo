/* =========================
   MOSTRAR CAMPOS PROFISSIONAL
========================= */
function mostrarCampos() {
    const tipo = document.getElementById("tipo").value;
    const campos = document.getElementById("camposProfissional");

    campos.style.display = (tipo === "profissional") ? "block" : "none";
}

/* =========================
   TELEFONE (máscara)
========================= */
const tel = document.getElementById("telefone");

if (tel) {
    tel.addEventListener("input", () => {
        let v = tel.value.replace(/\D/g, "");

        if (v.length > 11) v = v.slice(0, 11);

        v = v.replace(/^(\d{2})(\d)/g, "($1) $2");
        v = v.replace(/(\d{5})(\d)/, "$1-$2");

        tel.value = v;
    });
}

/* =========================
   CPF / CNPJ
========================= */
const documento = document.getElementById("documento");
const tipoDoc = document.querySelector("select[name='tipoDocumento']");

function aplicarMascaraDocumento() {
    let v = documento.value.replace(/\D/g, "");

    if (tipoDoc.value === "CPF") {
        // limita CPF (11)
        v = v.slice(0, 11);

        // máscara CPF
        v = v.replace(/(\d{3})(\d)/, "$1.$2");
        v = v.replace(/(\d{3})(\d)/, "$1.$2");
        v = v.replace(/(\d{3})(\d{1,2})$/, "$1-$2");

        documento.maxLength = 14; // 000.000.000-00
    } else {
        // limita CNPJ (14)
        v = v.slice(0, 14);

        // máscara CNPJ
        v = v.replace(/^(\d{2})(\d)/, "$1.$2");
        v = v.replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3");
        v = v.replace(/\.(\d{3})(\d)/, ".$1/$2");
        v = v.replace(/(\d{4})(\d)/, "$1-$2");

        documento.maxLength = 18; // 00.000.000/0000-00
    }

    documento.value = v;
}

/* EVENTOS */
if (documento) {
    documento.addEventListener("input", aplicarMascaraDocumento);
}

if (tipoDoc) {
    tipoDoc.addEventListener("change", () => {
        documento.value = ""; // limpa ao trocar tipo
        aplicarMascaraDocumento();
    });
}

/* =========================
   VALIDAR CPF
========================= */
function validarCPF(cpf) {
    const erro = document.getElementById("erroDocumento");

    erro.textContent = "CPF inválido";
    cpf = cpf.replace(/\D/g, "");

    if (cpf.length !== 11 || /^(\d)\1+$/.test(cpf))
        return false;

    let soma = 0;
    let resto;

    for (let i = 1; i <= 9; i++)
        soma += parseInt(cpf.substring(i - 1, i)) * (11 - i);

    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;

    if (resto !== parseInt(cpf.substring(9, 10)))
        return false;

    soma = 0;

    for (let i = 1; i <= 10; i++)
        soma += parseInt(cpf.substring(i - 1, i)) * (12 - i);

    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;

    return resto === parseInt(cpf.substring(10, 11));
}

/* =========================
   VALIDAR CNPJ
========================= */
function validarCNPJ(cnpj) {
    cnpj = cnpj.replace(/\D/g, "");

    if (cnpj.length !== 14 || /^(\d)\1+$/.test(cnpj))
        return false;

    let tamanho = cnpj.length - 2;
    let numeros = cnpj.substring(0, tamanho);
    let digitos = cnpj.substring(tamanho);

    let soma = 0;
    let pos = tamanho - 7;

    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }

    let resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);

    if (resultado != digitos.charAt(0))
        return false;

    tamanho = tamanho + 1;
    numeros = cnpj.substring(0, tamanho);

    soma = 0;
    pos = tamanho - 7;

    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }

    resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);

    return resultado == digitos.charAt(1);
}

const documento = document.getElementById("documento");
const tipoDoc = document.querySelector("select[name='tipoDocumento']");
const docMsg = document.getElementById("docMsg");
const btn = document.querySelector("button");

/* =========================
   VALIDAR EM TEMPO REAL
========================= */
function validarDocumentoRealtime() {
    const valor = documento.value;

    if (valor.length < 5) {
        resetEstado(documento, docMsg);
        return;
    }

    let valido = false;

    if (tipoDoc.value === "CPF") {
        valido = validarCPF(valor);
    } else {
        valido = validarCNPJ(valor);
    }

    if (valido) {
        setSucesso(documento, docMsg, "Documento válido");
    } else {
        setErro(documento, docMsg, "Documento inválido");
    }

    controlarBotao();
}

/* =========================
   ESTADOS VISUAIS
========================= */
function setErro(input, msgEl, msg) {
    input.classList.add("input-error");
    input.classList.remove("input-success");

    msgEl.textContent = msg;
    msgEl.classList.remove("success");
}

function setSucesso(input, msgEl, msg) {
    input.classList.remove("input-error");
    input.classList.add("input-success");

    msgEl.textContent = msg;
    msgEl.classList.add("success");
}

function resetEstado(input, msgEl) {
    input.classList.remove("input-error", "input-success");
    msgEl.textContent = "";
}

/* =========================
   CONTROLE BOTÃO
========================= */
function controlarBotao() {
    const tipoUsuario = document.getElementById("tipo").value;

    if (tipoUsuario !== "profissional") {
        btn.disabled = false;
        return;
    }

    const valido = documento.classList.contains("input-success");

    btn.disabled = !valido;
}

/* =========================
   EVENTOS
========================= */
if (documento) {
    documento.addEventListener("input", validarDocumentoRealtime);
}

if (tipoDoc) {
    tipoDoc.addEventListener("change", () => {
        documento.value = "";
        resetEstado(documento, docMsg);
        controlarBotao();
    });
}
/* =========================
   VALIDAÇÃO
========================= */
const form = document.querySelector("form");

form.addEventListener("submit", function (e) {

    const senha = document.getElementById("senha");
    const btn = form.querySelector("button");
    const documento = document.getElementById("documento");
    const tipoDoc = document.querySelector("select[name='tipoDocumento']");
    const tipoUsuario = document.getElementById("tipo");

    /* SENHA */
    if (senha.value.length < 6) {
        alert("Senha precisa ter no mínimo 6 caracteres");
        e.preventDefault();
        return;
    }

    /* VALIDA DOCUMENTO SE FOR PROFISSIONAL */
    if (tipoUsuario.value === "profissional") {

        const valor = documento.value;

        if (tipoDoc.value === "CPF") {
            if (!validarCPF(valor)) {
                alert("CPF inválido");
                e.preventDefault();
                return;
            }
        } else {
            if (!validarCNPJ(valor)) {
                alert("CNPJ inválido");
                e.preventDefault();
                return;
            }
        }
    }

    btn.textContent = "Cadastrando...";
    btn.disabled = true;
});

const senha = document.getElementById("senha");

senha.addEventListener("input", () => {
    if (senha.value.length >= 6) {
        senha.classList.add("input-success");
        senha.classList.remove("input-error");
    } else {
        senha.classList.add("input-error");
        senha.classList.remove("input-success");
    }
});