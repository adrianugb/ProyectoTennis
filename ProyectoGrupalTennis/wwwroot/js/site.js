// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Vista de módulos
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

// Cambiar contraseña
function togglePassword(inputId, icon) {
    const input = document.getElementById(inputId);

    if (input.type === "password") {
        input.type = "text";
        icon.classList.remove("fa-eye");
        icon.classList.add("fa-eye-slash");
    } else {
        input.type = "password";
        icon.classList.remove("fa-eye-slash");
        icon.classList.add("fa-eye");
    }
}