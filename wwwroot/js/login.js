document.addEventListener("DOMContentLoaded", () => {

    const form = document.getElementById("loginForm");
    const email = document.getElementById("email");
    const senha = document.getElementById("senha");
    const btn = document.getElementById("btnLogin");

    const emailError = document.getElementById("emailError");
    const senhaError = document.getElementById("senhaError");

    // 👁 mostrar senha
    document.getElementById("toggleSenha").addEventListener("click", () => {
        senha.type = senha.type === "password" ? "text" : "password";
    });

    form.addEventListener("submit", (e) => {
        let valid = true;

        // reset
        emailError.textContent = "";
        senhaError.textContent = "";
        email.classList.remove("input-error");
        senha.classList.remove("input-error");

        // email
        if (!email.value.includes("@")) {
            emailError.textContent = "Email inválido";
            email.classList.add("input-error");
            valid = false;
        }

        // senha
        if (senha.value.length < 6) {
            senhaError.textContent = "Mínimo 6 caracteres";
            senha.classList.add("input-error");
            valid = false;
        }

        if (!valid) {
            e.preventDefault();
            return;
        }

        // loading
        btn.textContent = "Entrando...";
        btn.disabled = true;
    });

});