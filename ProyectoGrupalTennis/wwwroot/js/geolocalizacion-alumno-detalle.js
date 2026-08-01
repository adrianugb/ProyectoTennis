document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const mapaElemento =
        document.getElementById("mapaAlumnoDetalle");

    if (!mapaElemento || typeof L === "undefined") {
        return;
    }

    const latitud = Number.parseFloat(
        mapaElemento.dataset.latitud
    );

    const longitud = Number.parseFloat(
        mapaElemento.dataset.longitud
    );

    const nombre =
        mapaElemento.dataset.nombre || "Mi ubicación";

    if (
        !Number.isFinite(latitud) ||
        !Number.isFinite(longitud)
    ) {
        return;
    }

    const mapa = L.map(mapaElemento, {
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
        .bindPopup(nombre)
        .openPopup();

    window.setTimeout(function () {
        mapa.invalidateSize();
    }, 150);
});