// Card perfil

document.querySelectorAll('.perfil-card').forEach(card => {
    card.addEventListener('mouseenter', () => {
        card.style.transform = "translateY(-6px) scale(1.01)";
    });

    card.addEventListener('mouseleave', () => {
        card.style.transform = "";
    });
});