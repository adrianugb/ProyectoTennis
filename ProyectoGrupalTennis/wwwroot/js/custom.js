(function ($) {

    "use strict";

    // PRE LOADER
    $(window).on('load', function () {
        $('.preloader').fadeOut(1000); // set duration in brackets
    });

    // Respaldo: si el evento "load" tarda demasiado (conexión lenta, imágenes
    // pesadas, iframe del mapa, etc.) o no llega a disparar en algunos
    // navegadores móviles, quitamos el preloader igual para no dejar
    // la pantalla (y el asistente virtual) tapados indefinidamente.
    setTimeout(function () {
        $('.preloader').fadeOut(500);
    }, 3000);


    // Cierra el menú móvil al hacer click en un link (Bootstrap 5: la API de
    // jQuery de Collapse no existe en BS5, hay que usar bootstrap.Collapse).
    document.addEventListener('DOMContentLoaded', function () {
        var collapseEl = document.getElementById('mainNav');
        if (!collapseEl || typeof bootstrap === 'undefined') return;

        collapseEl.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                if (!collapseEl.classList.contains('show')) return;
                var instance = bootstrap.Collapse.getOrCreateInstance(collapseEl);
                instance.hide();
            });
        });
    });

})(jQuery);