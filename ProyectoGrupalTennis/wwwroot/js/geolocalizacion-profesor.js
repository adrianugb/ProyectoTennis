document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    if (typeof L === "undefined") {
        return;
    }

    const mapas =
        document.querySelectorAll(".mapa-profesor");

    mapas.forEach(function (elemento) {
        const latitud = Number.parseFloat(
            elemento.dataset.latitud
        );

        const longitud = Number.parseFloat(
            elemento.dataset.longitud
        );

        const alumno =
            elemento.dataset.alumno || "Alumno";

        const curso =
            elemento.dataset.curso || "Clase a domicilio";

        if (
            !Number.isFinite(latitud) ||
            !Number.isFinite(longitud)
        ) {
            return;
        }

        const mapa = L.map(elemento, {
            scrollWheelZoom: false
        }).setView(
            [latitud, longitud],
            16
        );

        L.tileLayer(
            "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            {
                maxZoom: 19,
                attribution:
                    '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            }
        ).addTo(mapa);

        L.marker([latitud, longitud])
            .addTo(mapa)
            .bindPopup(
                `<strong>${escaparHtml(alumno)}</strong><br>` +
                `${escaparHtml(curso)}`
            )
            .openPopup();

        window.setTimeout(function () {
            mapa.invalidateSize();
        }, 150);
    });

    function escaparHtml(valor) {
        const elemento =
            document.createElement("div");

        elemento.textContent = valor;

        return elemento.innerHTML;
    }
});