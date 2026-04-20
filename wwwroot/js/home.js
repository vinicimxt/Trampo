document.addEventListener('DOMContentLoaded', function () {

    // -------------------------------------------------------
    // Navbar shadow no scroll
    // -------------------------------------------------------
    var nav = document.querySelector('.navbar');
    if (nav) {
        window.addEventListener('scroll', function () {
            nav.classList.toggle('shadow', window.scrollY > 20);
        });
    }

    // -------------------------------------------------------
    // Efeito 3D no hero-visual ao mover o mouse
    // -------------------------------------------------------
    var visual = document.querySelector('.hero-visual');
    if (visual) {
        document.addEventListener('mousemove', function (e) {
            var x = (window.innerWidth  / 2 - e.clientX) / 30;
            var y = (window.innerHeight / 2 - e.clientY) / 30;
            visual.style.transform = 'rotateY(' + x + 'deg) rotateX(' + y + 'deg)';
        });
    }

    // -------------------------------------------------------
    // Scroll reveal — fade-up
    // -------------------------------------------------------
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

    // -------------------------------------------------------
    // Tab switcher — Como Funciona (cliente / profissional)
    // -------------------------------------------------------
    window.switchTab = function (tabId, btn) {
        document.querySelectorAll('.tab-content').forEach(function (t) { t.classList.remove('active'); });
        document.querySelectorAll('.tab-btn').forEach(function (b) { b.classList.remove('active'); });

        var target = document.getElementById('tab-' + tabId);
        if (target) { target.classList.add('active'); }
        if (btn)    { btn.classList.add('active'); }

        if (target) {
            target.querySelectorAll('.fade-up').forEach(function (el) {
                el.classList.remove('visible');
                setTimeout(function () { el.classList.add('visible'); }, 80);
            });
        }
    };

});

