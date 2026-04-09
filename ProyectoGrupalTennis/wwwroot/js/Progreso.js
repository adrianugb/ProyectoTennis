function mostrarVistaProgreso(idVista) {
    const vistas = document.querySelectorAll(".vista-progreso");
    vistas.forEach(function (vista) {
        vista.style.display = "none";
    });

    const vistaSeleccionada = document.getElementById(idVista);
    if (vistaSeleccionada) {
        vistaSeleccionada.style.display = "block";
    }
}

function cambiarEstado(btn) {
    if (btn.innerText.trim() === "Activo") {
        btn.innerText = "Inactivo";
        btn.classList.remove("badge-soft-success");
        btn.classList.add("badge-soft-warning");
    } else {
        btn.innerText = "Activo";
        btn.classList.remove("badge-soft-warning");
        btn.classList.add("badge-soft-success");
    }
}

function agregarCriterio() {
    const tabla = document.getElementById("tablaCriterios");

    if (!tabla) return;

    const fila = `
        <tr>
            <td>Nuevo</td>
            <td>Nueva habilidad</td>
            <td>Nuevo criterio agregado</td>
            <td>
                <span class="badge-soft-success" onclick="cambiarEstado(this)" style="cursor:pointer;">
                    Activo
                </span>
            </td>
        </tr>
    `;

    tabla.innerHTML += fila;
}

window.mostrarVistaProgreso = mostrarVistaProgreso;
window.cambiarEstado = cambiarEstado;
window.agregarCriterio = agregarCriterio;

document.addEventListener("DOMContentLoaded", function () {
    mostrarVistaProgreso("vista-admin");
});