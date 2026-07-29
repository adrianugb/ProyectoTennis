// REWORK UI — Slider del hero (sin dependencias, liviano) + animación de scroll
(function () {
    "use strict";

    /* ---------- Hero slider ---------- */
    var slider = document.querySelector('.hero-slider');
    if (slider) {
        var slides = Array.prototype.slice.call(slider.querySelectorAll('.hero-slide'));
        var dotsWrap = slider.querySelector('.hero-dots');
        var prevBtn = slider.querySelector('.hero-prev');
        var nextBtn = slider.querySelector('.hero-next');
        var current = 0;
        var timer = null;
        var AUTOPLAY_MS = 6000;

        slides.forEach(function (_, i) {
            var dot = document.createElement('button');
            dot.type = 'button';
            dot.className = 'hero-dot' + (i === 0 ? ' is-active' : '');
            dot.setAttribute('aria-label', 'Ir a la diapositiva ' + (i + 1));
            dot.addEventListener('click', function () { goTo(i); });
            dotsWrap.appendChild(dot);
        });

        var dots = Array.prototype.slice.call(dotsWrap.querySelectorAll('.hero-dot'));

        function goTo(index) {
            slides[current].classList.remove('is-active');
            dots[current].classList.remove('is-active');
            current = (index + slides.length) % slides.length;
            slides[current].classList.add('is-active');
            dots[current].classList.add('is-active');
        }

        function next() { goTo(current + 1); }
        function prev() { goTo(current - 1); }

        function startAutoplay() {
            stopAutoplay();
            timer = setInterval(next, AUTOPLAY_MS);
        }
        function stopAutoplay() {
            if (timer) clearInterval(timer);
        }

        if (nextBtn) nextBtn.addEventListener('click', function () { next(); startAutoplay(); });
        if (prevBtn) prevBtn.addEventListener('click', function () { prev(); startAutoplay(); });

        slider.addEventListener('mouseenter', stopAutoplay);
        slider.addEventListener('mouseleave', startAutoplay);

        startAutoplay();
    }

    /* ---------- Scroll reveal (reemplaza wow.js en esta página) ---------- */
    var revealEls = document.querySelectorAll('.reveal');
    if (revealEls.length) {
        if ('IntersectionObserver' in window) {
            var observer = new IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('is-visible');
                        observer.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.15 });

            revealEls.forEach(function (el) { observer.observe(el); });
        } else {
            revealEls.forEach(function (el) { el.classList.add('is-visible'); });
        }
    }
})();