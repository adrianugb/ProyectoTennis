document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const mapaElemento =
        document.getElementById("mapaAlumnoDetalle");

    const botonCambiarMoneda =
        document.getElementById("btnCambiarMoneda");

    const costoZonaDetalle =
        document.getElementById("costoZonaDetalle");

    const TIPO_CAMBIO_USD = 452.51;

    let mostrarUsd = false;

    if (botonCambiarMoneda && costoZonaDetalle) {

        const costoCrc =
            Number.parseFloat(
                costoZonaDetalle.dataset.costoCrc || "0"
            );

        function actualizarMoneda() {

            if (mostrarUsd) {

                const costoUsd =
                    costoCrc / TIPO_CAMBIO_USD;

                costoZonaDetalle.textContent =
                    new Intl.NumberFormat(
                        "en-US",
                        {
                            style: "currency",
                            currency: "USD",
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        }
                    ).format(costoUsd);

                botonCambiarMoneda.innerHTML =
                    '<i class="fa fa-money me-1"></i> Ver en colones';
            }
            else {

                costoZonaDetalle.textContent =
                    new Intl.NumberFormat(
                        "es-CR",
                        {
                            style: "currency",
                            currency: "CRC",
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        }
                    ).format(costoCrc);

                botonCambiarMoneda.innerHTML =
                    '<i class="fa fa-dollar-sign me-1"></i> Ver en dólares';
            }
        }

        botonCambiarMoneda.addEventListener(
            "click",
            function () {

                mostrarUsd =
                    !mostrarUsd;

                actualizarMoneda();
            }
        );

        actualizarMoneda();
    }

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