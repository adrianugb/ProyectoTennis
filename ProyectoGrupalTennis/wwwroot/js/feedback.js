(function () {
    function mostrarVista(id) {
        const vistas = document.querySelectorAll('.vista');

        vistas.forEach(function (vista) {
            vista.classList.remove('active');
        });

        const vistaObjetivo = document.getElementById(id);
        if (vistaObjetivo) {
            vistaObjetivo.classList.add('active');
        }
    }

    window.mostrarVista = mostrarVista;

    document.addEventListener('DOMContentLoaded', function () {
        const vistaActiva = document.querySelector('.vista.active');

        if (!vistaActiva) {
            const primeraVista = document.getElementById('feedback-admin');
            if (primeraVista) {
                primeraVista.classList.add('active');
            }
        }
    });
})();