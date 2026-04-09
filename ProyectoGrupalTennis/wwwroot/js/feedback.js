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

    function guardarConfiguracionEncuesta() {
        document.getElementById("mensajeGuardadoEncuesta").style.display = "block";
        document.getElementById("vistaPreviaEncuesta").style.display = "none";
    }

    function mostrarVistaPreviaEncuesta() {
        document.getElementById("previewTituloEncuesta").innerText =
            document.getElementById("tituloEncuesta").value || "Sin título";

        document.getElementById("previewFrecuenciaEncuesta").innerText =
            document.getElementById("frecuenciaEncuesta").value;

        document.getElementById("previewPregunta1").innerText =
            document.getElementById("pregunta1").value || "Sin contenido";

        document.getElementById("previewPregunta2").innerText =
            document.getElementById("pregunta2").value || "Sin contenido";

        document.getElementById("previewPregunta3").innerText =
            document.getElementById("pregunta3").value || "Sin contenido";

        document.getElementById("previewComentariosEncuesta").innerText =
            document.getElementById("comentariosEncuesta").value || "Sin comentarios";

        document.getElementById("vistaPreviaEncuesta").style.display = "block";
        document.getElementById("mensajeGuardadoEncuesta").style.display = "none";
    }

    function seleccionarCriterio(card) {
        const cards = document.querySelectorAll(".criterio-card");
        cards.forEach(function (c) {
            c.style.border = "none";
        });

        card.style.border = "2px solid #6f42c1";
    }

    window.mostrarVista = mostrarVista;
    window.guardarConfiguracionEncuesta = guardarConfiguracionEncuesta;
    window.mostrarVistaPreviaEncuesta = mostrarVistaPreviaEncuesta;
    window.seleccionarCriterio = seleccionarCriterio;

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