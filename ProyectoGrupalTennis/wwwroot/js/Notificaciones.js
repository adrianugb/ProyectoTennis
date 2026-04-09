function mostrarVistaNotificaciones(idVista) {
    const vistas = document.querySelectorAll(".vista-notificaciones");
    vistas.forEach(vista => vista.style.display = "none");

    const vistaSeleccionada = document.getElementById(idVista);
    if (vistaSeleccionada) {
        vistaSeleccionada.style.display = "block";
    }
}

document.addEventListener("DOMContentLoaded", function () {
    mostrarVistaNotificaciones("vista-admin");
});

function mostrarEventosAdmin() {
    document.getElementById("eventosAdmin").style.display = "block";
    document.getElementById("historialAdmin").style.display = "none";
}

function mostrarHistorialAdmin() {
    document.getElementById("historialAdmin").style.display = "block";
    document.getElementById("eventosAdmin").style.display = "none";
}

function enviarNotificacionAdmin() {
    alert("Notificación enviada correctamente");
}