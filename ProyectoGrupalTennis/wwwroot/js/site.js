// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function mostrarVista(idVista) {
    const vistas = document.querySelectorAll('.vista-modulo');

    vistas.forEach(vista => {
        vista.style.display = 'none';
    });

    const vistaSeleccionada = document.getElementById(idVista);
    if (vistaSeleccionada) {
        vistaSeleccionada.style.display = 'block';
    }
}