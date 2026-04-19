window.addEventListener("scroll", () => {
    const nav = document.querySelector(".navbar");
    nav.classList.toggle("shadow", window.scrollY > 20);
});

const visual = document.querySelector('.hero-visual');

document.addEventListener('mousemove', (e) => {
    const x = (window.innerWidth / 2 - e.clientX) / 30;
    const y = (window.innerHeight / 2 - e.clientY) / 30;

    visual.style.transform = `rotateY(${x}deg) rotateX(${y}deg)`;
});

// -------------------------------------------------------
// Scroll reveal — fade-up
// -------------------------------------------------------
const fadeEls = document.querySelectorAll('.fade-up');
const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('visible');
            observer.unobserve(entry.target);
        }
    });
}, { threshold: 0.12 });

fadeEls.forEach(el => observer.observe(el));


// -------------------------------------------------------
// Tab switcher — Como Funciona (cliente / profissional)
// -------------------------------------------------------
function switchTab(tabId, btn) {
    // Esconde todas as abas
    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    // Remove active de todos os botões
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));

    // Ativa aba e botão clicados
    document.getElementById('tab-' + tabId).classList.add('active');
    btn.classList.add('active');

    // Re-aciona fade-up nos steps do novo tab
    const newFades = document.querySelectorAll('#tab-' + tabId + ' .fade-up');
    newFades.forEach(el => {
        el.classList.remove('visible');
        setTimeout(() => el.classList.add('visible'), 80);
    });
}