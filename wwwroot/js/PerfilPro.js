document.addEventListener('DOMContentLoaded', function () {

    /* -----------------------------------------------
       Highlight do stat de pendentes
       — pisca suavemente se houver pedidos pendentes
    ----------------------------------------------- */
    var statVals = document.querySelectorAll('.stat-item-val');
    statVals.forEach(function (el) {
        if (el.style.color === 'var(--red)' || el.textContent.trim() !== '0' && el.style.color) {
            el.style.animation = 'pulse-val 2.5s ease infinite';
        }
    });

    /* -----------------------------------------------

       (ex: tooltip, prefetch de rota, analytics)
    ----------------------------------------------- */
    document.querySelectorAll('.perfil-card').forEach(function (card) {

        card.addEventListener('mouseenter', function () {
            /* gancho futuro: prefetch ou analytics */
        });

        card.addEventListener('mouseleave', function () {
            /* gancho futuro */
        });
    });

});