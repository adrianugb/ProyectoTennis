document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const mapaElemento =
        document.getElementById("mapaLogisticoAdmin");

    if (!mapaElemento || typeof L === "undefined") {
        return;
    }

    let clases = [];

    try {
        clases = JSON.parse(
            mapaElemento.dataset.clases || "[]"
        );
    } catch (error) {
        console.error(
            "No fue posible cargar las clases del mapa.",
            error
        );

        return;
    }

    if (!Array.isArray(clases) || clases.length === 0) {
        return;
    }

    const mapa = L.map(mapaElemento, {
        scrollWheelZoom: false
    }).setView(
        [9.9281, -84.0907],
        10
    );

    L.tileLayer(
        "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        {
            maxZoom: 19,
            attribution:
                '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }
    ).addTo(mapa);

    const grupoMarcadores = L.featureGroup()
        .addTo(mapa);

    const marcadoresPorMatricula =
        new Map();

    clases.forEach(function (clase) {
        const latitud =
            Number.parseFloat(clase.latitud);

        const longitud =
            Number.parseFloat(clase.longitud);

        if (
            !Number.isFinite(latitud) ||
            !Number.isFinite(longitud)
        ) {
            return;
        }

        const marcador = L.marker(
            [latitud, longitud]
        ).addTo(grupoMarcadores);

        marcador.bindPopup(
            construirPopup(clase),
            {
                maxWidth: 330
            }
        );

        /*
         * Puede haber varios horarios para la misma matrícula.
         * Guardamos una lista de marcadores por matrícula.
         */
        const clave =
            String(clase.idMatricula);

        if (!marcadoresPorMatricula.has(clave)) {
            marcadoresPorMatricula.set(
                clave,
                []
            );
        }

        marcadoresPorMatricula
            .get(clave)
            .push(marcador);
    });

    if (grupoMarcadores.getLayers().length > 0) {
        mapa.fitBounds(
            grupoMarcadores.getBounds(),
            {
                padding: [35, 35],
                maxZoom: 15
            }
        );
    }

    const tarjetas =
        document.querySelectorAll(
            ".geo-admin-clase-item"
        );

    tarjetas.forEach(function (tarjeta) {
        tarjeta.addEventListener(
            "click",
            function () {
                tarjetas.forEach(function (elemento) {
                    elemento.classList.remove(
                        "active"
                    );
                });

                tarjeta.classList.add("active");

                const idMatricula =
                    tarjeta.dataset.idMatricula;

                const latitud =
                    Number.parseFloat(
                        tarjeta.dataset.latitud
                    );

                const longitud =
                    Number.parseFloat(
                        tarjeta.dataset.longitud
                    );

                if (
                    Number.isFinite(latitud) &&
                    Number.isFinite(longitud)
                ) {
                    mapa.setView(
                        [latitud, longitud],
                        16,
                        {
                            animate: true
                        }
                    );
                }

                const marcadores =
                    marcadoresPorMatricula.get(
                        String(idMatricula)
                    );

                if (
                    Array.isArray(marcadores) &&
                    marcadores.length > 0
                ) {
                    marcadores[0].openPopup();
                }

                if (window.innerWidth < 992) {
                    mapaElemento.scrollIntoView({
                        behavior: "smooth",
                        block: "center"
                    });
                }
            }
        );
    });

    window.setTimeout(function () {
        mapa.invalidateSize();
    }, 150);

    function construirPopup(clase) {
        return `
            <div class="geo-popup-admin">
                <strong>${escaparHtml(clase.curso)}</strong>

                <span>
                    <i class="fa fa-user"></i>
                    ${escaparHtml(clase.alumno)}
                </span>

                <span>
                    <i class="fa fa-chalkboard-teacher"></i>
                    ${escaparHtml(clase.profesor)}
                </span>

                <span>
                    <i class="fa fa-calendar"></i>
                    ${escaparHtml(clase.fecha)}
                    (${escaparHtml(clase.diaSemana)})
                </span>

                <span>
                    <i class="fa fa-clock-o"></i>
                    ${escaparHtml(clase.horaInicio)}
                    -
                    ${escaparHtml(clase.horaFin)}
                </span>

                <span>
                    <i class="fa fa-map-marker"></i>
                    ${escaparHtml(clase.direccion)}
                </span>

                <span>
                    <i class="fa fa-map"></i>
                    ${escaparHtml(clase.zona)}
                </span>

                <span>
                    <i class="fa fa-phone"></i>
                    ${escaparHtml(clase.telefono)}
                </span>
            </div>
        `;
    }

    function escaparHtml(valor) {
        const elemento =
            document.createElement("div");

        elemento.textContent =
            valor == null ? "" : String(valor);

        return elemento.innerHTML;
    }
});