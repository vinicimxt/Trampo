document.addEventListener('DOMContentLoaded', function () {

    /* -----------------------------------------------
       NAVBAR — shadow no scroll
    ----------------------------------------------- */
    var nav = document.querySelector('.navbar');
    if (nav) {
        window.addEventListener('scroll', function () {
            nav.classList.toggle('scrolled', window.scrollY > 20);
        });
    }

    /* -----------------------------------------------
       HAMBURGER MENU
    ----------------------------------------------- */
    var menuToggle = document.getElementById('menuToggle');
    var navLinks = document.getElementById('navLinks');
    var navActions = document.getElementById('navActions');

    if (menuToggle) {
        menuToggle.addEventListener('click', function () {
            var isOpen = menuToggle.classList.toggle('open');
            if (navLinks) navLinks.classList.toggle('open', isOpen);
            if (navActions) navActions.classList.toggle('open', isOpen);
            // troca ícone
            menuToggle.querySelector('.material-symbols-rounded').textContent =
                isOpen ? 'close' : 'menu';
        });
    }

    // fecha menu ao clicar num link
    document.querySelectorAll('#navLinks a, #navActions a').forEach(function (link) {
        link.addEventListener('click', function () {
            if (menuToggle) menuToggle.classList.remove('open');
            if (navLinks) navLinks.classList.remove('open');
            if (navActions) navActions.classList.remove('open');
            if (menuToggle) menuToggle.querySelector('.material-symbols-rounded').textContent = 'menu';
        });
    });

    /* -----------------------------------------------
       EFEITO 3D no hero-visual
    ----------------------------------------------- */
    var visual = document.querySelector('.hero-visual');
    if (visual) {
        document.addEventListener('mousemove', function (e) {
            var x = (window.innerWidth / 2 - e.clientX) / 30;
            var y = (window.innerHeight / 2 - e.clientY) / 30;
            visual.style.transform = 'rotateY(' + x + 'deg) rotateX(' + y + 'deg)';
        });
    }

    /* -----------------------------------------------
       SCROLL REVEAL — fade-up
    ----------------------------------------------- */
    var fadeEls = document.querySelectorAll('.fade-up');
    if (fadeEls.length > 0) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });

        fadeEls.forEach(function (el) { observer.observe(el); });
    }

    /* -----------------------------------------------
       TAB SWITCHER — Como Funciona
    ----------------------------------------------- */
    window.switchTab = function (tabId, btn) {
        document.querySelectorAll('.tab-content').forEach(function (t) {
            t.classList.remove('active');
        });
        document.querySelectorAll('.tab-btn').forEach(function (b) {
            b.classList.remove('active');
        });

        var target = document.getElementById('tab-' + tabId);
        if (target) target.classList.add('active');
        if (btn) btn.classList.add('active');

        if (target) {
            target.querySelectorAll('.fade-up').forEach(function (el) {
                el.classList.remove('visible');
                setTimeout(function () { el.classList.add('visible'); }, 80);
            });
        }
    };

});