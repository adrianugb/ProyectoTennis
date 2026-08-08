document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const mapaElemento = document.getElementById("mapaSede");
    const latitudInput = document.getElementById("LatitudCentro");
    const longitudInput = document.getElementById("LongitudCentro");
    const textoLatitud = document.getElementById("textoLatitud");
    const textoLongitud = document.getElementById("textoLongitud");
    const errorUbicacion = document.getElementById("errorUbicacion");
    const formulario = document.getElementById("formZona");
    const botonUbicacion = document.getElementById("btnMiUbicacion");
    const botonLimpiar = document.getElementById("btnLimpiarUbicacion");

    if (
        !mapaElemento ||
        !latitudInput ||
        !longitudInput ||
        typeof L === "undefined"
    ) {
        return;
    }

    delete L.Icon.Default.prototype._getIconUrl;

    L.Icon.Default.mergeOptions({
        iconRetinaUrl:
            "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png",

        iconUrl:
            "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png",

        shadowUrl:
            "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png"
    });

    const convertirNumero = function (valor) {
        if (!valor) {
            return Number.NaN;
        }

        return Number.parseFloat(
            valor.toString().replace(",", ".")
        );
    };

    const latitudGuardada = convertirNumero(latitudInput.value);
    const longitudGuardada = convertirNumero(longitudInput.value);

    const tieneUbicacionGuardada =
        Number.isFinite(latitudGuardada) &&
        Number.isFinite(longitudGuardada);

    const latitudCostaRica = 9.9281;
    const longitudCostaRica = -84.0907;

    const mapa = L.map("mapaSede").setView(
        tieneUbicacionGuardada
            ? [latitudGuardada, longitudGuardada]
            : [latitudCostaRica, longitudCostaRica],
        tieneUbicacionGuardada ? 15 : 12
    );

    L.tileLayer(
        "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        {
            maxZoom: 19,
            attribution:
                '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }
    ).addTo(mapa);

    let marcador = null;

    const seleccionarUbicacion = function (latitud, longitud) {
        const latitudFormateada =
            Number(latitud).toFixed(7);

        const longitudFormateada =
            Number(longitud).toFixed(7);

        latitudInput.value =
            latitudFormateada;

        longitudInput.value =
            longitudFormateada;

        const latitudTextoInput =
            document.getElementById("LatitudCentroTexto");

        const longitudTextoInput =
            document.getElementById("LongitudCentroTexto");

        if (latitudTextoInput) {
            latitudTextoInput.value =
                latitudFormateada;
        }

        if (longitudTextoInput) {
            longitudTextoInput.value =
                longitudFormateada;
        }

        if (textoLatitud) {
            textoLatitud.textContent =
                latitudFormateada;
        }

        if (textoLongitud) {
            textoLongitud.textContent =
                longitudFormateada;
        }

        if (errorUbicacion) {
            errorUbicacion.classList.add("d-none");
        }

        if (marcador === null) {
            marcador = L.marker(
                [latitud, longitud],
                {
                    draggable: true
                }
            ).addTo(mapa);

            marcador
                .bindPopup("Ubicación seleccionada")
                .openPopup();

            marcador.on(
                "dragend",
                function (evento) {
                    const posicion =
                        evento.target.getLatLng();

                    seleccionarUbicacion(
                        posicion.lat,
                        posicion.lng
                    );
                }
            );
        } else {
            marcador.setLatLng(
                [latitud, longitud]
            );
        }
    };

    if (tieneUbicacionGuardada) {
        seleccionarUbicacion(
            latitudGuardada,
            longitudGuardada
        );

        mapa.setView(
            [latitudGuardada, longitudGuardada],
            15
        );
    } else {
        if (textoLatitud) {
            textoLatitud.textContent = "No seleccionada";
        }

        if (textoLongitud) {
            textoLongitud.textContent = "No seleccionada";
        }
    }

    mapa.on("click", function (evento) {
        seleccionarUbicacion(
            evento.latlng.lat,
            evento.latlng.lng
        );
    });

    if (botonUbicacion) {
        botonUbicacion.addEventListener("click", function () {
            if (!navigator.geolocation) {
                alert(
                    "El navegador no permite obtener la ubicación actual."
                );

                return;
            }

            botonUbicacion.disabled = true;
            botonUbicacion.innerHTML =
                '<i class="fa fa-spinner fa-spin me-1"></i> Buscando...';

            navigator.geolocation.getCurrentPosition(
                function (posicion) {
                    const latitud = posicion.coords.latitude;
                    const longitud = posicion.coords.longitude;

                    seleccionarUbicacion(latitud, longitud);

                    mapa.setView(
                        [latitud, longitud],
                        16
                    );

                    botonUbicacion.disabled = false;
                    botonUbicacion.innerHTML =
                        '<i class="fa fa-crosshairs me-1"></i> Usar mi ubicación actual';
                },
                function () {
                    alert(
                        "No fue posible obtener su ubicación. Revise los permisos del navegador."
                    );

                    botonUbicacion.disabled = false;
                    botonUbicacion.innerHTML =
                        '<i class="fa fa-crosshairs me-1"></i> Usar mi ubicación actual';
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000
                }
            );
        });
    }

    if (botonLimpiar) {
        botonLimpiar.addEventListener("click", function () {
            latitudInput.value = "";
            longitudInput.value = "";

            const latitudTextoInput =
                document.getElementById("LatitudCentroTexto");

            const longitudTextoInput =
                document.getElementById("LongitudCentroTexto");

            if (latitudTextoInput) {
                latitudTextoInput.value = "";
            }

            if (longitudTextoInput) {
                longitudTextoInput.value = "";
            }

            if (textoLatitud) {
                textoLatitud.textContent =
                    "No seleccionada";
            }

            if (textoLongitud) {
                textoLongitud.textContent =
                    "No seleccionada";
            }

            if (marcador !== null) {
                mapa.removeLayer(marcador);
                marcador = null;
            }
        });
    }

    if (formulario) {
        formulario.addEventListener("submit", function (evento) {
            if (
                latitudInput.value.trim() === "" ||
                longitudInput.value.trim() === ""
            ) {
                evento.preventDefault();

                if (errorUbicacion) {
                    errorUbicacion.classList.remove("d-none");
                }

                mapaElemento.scrollIntoView({
                    behavior: "smooth",
                    block: "center"
                });
            }
        });
    }

    window.setTimeout(function () {
        mapa.invalidateSize();
    }, 100);
});