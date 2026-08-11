document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const mapaElemento =
        document.getElementById("mapaAlumno");

    const latitudInput =
        document.getElementById("Latitud");

    const longitudInput =
        document.getElementById("Longitud");

    const textoLatitud =
        document.getElementById("textoLatitud");

    const textoLongitud =
        document.getElementById("textoLongitud");

    const botonUbicacion =
        document.getElementById("btnMiUbicacion");

    const botonLimpiar =
        document.getElementById("btnLimpiarUbicacion");

    const errorUbicacion =
        document.getElementById("errorUbicacion");

    const formulario =
        document.getElementById("formUbicacionAlumno");

    const idZonaInput =
        document.getElementById("IdZona");

    const resultadoZona =
        document.getElementById("resultadoZona");

    const zonaSinDetectar =
        document.getElementById("zonaSinDetectar");

    const nombreZona =
        document.getElementById("nombreZona");

    const estadoCobertura =
        document.getElementById("estadoCobertura");

    const distanciaZona =
        document.getElementById("distanciaZona");

    const radioZona =
        document.getElementById("radioZona");

    const costoZona =
        document.getElementById("costoZona");

    const tarifaZona =
        document.getElementById("tarifaZona");

    const mensajeZona =
        document.getElementById("mensajeZona");

    const costoDistanciaZona =
        document.getElementById("costoDistanciaZona");

    const costoTotalZona =
        document.getElementById("costoTotalZona");

    const botonCambiarMoneda =
        document.getElementById("btnCambiarMonedaZona");

    const TIPO_CAMBIO_USD = 452.51;

    let monedaZona = "CRC";

    let valoresZona = {
        costo: 0,
        tarifaKm: 0,
        costoPorDistancia: 0,
        costoDesplazamiento: 0
    };

    function formatearColon(valor) {
        const numero = Number(valor || 0);

        return new Intl.NumberFormat(
            "es-CR",
            {
                style: "currency",
                currency: "CRC",
                minimumFractionDigits: 2
            }
        ).format(numero);
    }

    function formatearDolar(valor) {
        const numero = Number(valor || 0);

        return new Intl.NumberFormat(
            "en-US",
            {
                style: "currency",
                currency: "USD",
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        ).format(numero);
    }

    function convertirColonesADolares(valor) {
        return Number(valor || 0) / TIPO_CAMBIO_USD;
    }

    function actualizarValoresMoneda() {

        if (monedaZona === "USD") {

            costoZona.textContent =
                formatearDolar(
                    convertirColonesADolares(
                        valoresZona.costo
                    )
                );

            tarifaZona.textContent =
                formatearDolar(
                    convertirColonesADolares(
                        valoresZona.tarifaKm
                    )
                );

            costoDistanciaZona.textContent =
                formatearDolar(
                    convertirColonesADolares(
                        valoresZona.costoPorDistancia
                    )
                );

            costoTotalZona.textContent =
                formatearDolar(
                    convertirColonesADolares(
                        valoresZona.costoDesplazamiento
                    )
                );

            if (botonCambiarMoneda) {
                botonCambiarMoneda.innerHTML =
                    '<i class="fa fa-money me-1"></i> Ver en colones';
            }

        } else {

            costoZona.textContent =
                formatearColon(
                    valoresZona.costo
                );

            tarifaZona.textContent =
                formatearColon(
                    valoresZona.tarifaKm
                );

            costoDistanciaZona.textContent =
                formatearColon(
                    valoresZona.costoPorDistancia
                );

            costoTotalZona.textContent =
                formatearColon(
                    valoresZona.costoDesplazamiento
                );

            if (botonCambiarMoneda) {
                botonCambiarMoneda.innerHTML =
                    '<i class="fa fa-dollar-sign me-1"></i> Ver en dólares';
            }
        }
    }

    function limpiarZonaDetectada() {
        if (idZonaInput) {
            idZonaInput.value = "";
        }

        if (resultadoZona) {
            resultadoZona.classList.add("d-none");
        }

        if (zonaSinDetectar) {
            zonaSinDetectar.classList.remove("d-none");
        }
    }

    async function detectarZona(latitud, longitud) {
        limpiarZonaDetectada();

        try {
            const url =
                `/Geolocalizacion/ObtenerZona?latitud=${encodeURIComponent(latitud)}&longitud=${encodeURIComponent(longitud)}`;

            const respuesta = await fetch(url, {
                method: "GET",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!respuesta.ok) {
                throw new Error(
                    "No fue posible consultar la zona."
                );
            }

            const datos = await respuesta.json();

            if (!datos.encontrada) {
                mensajeZona.textContent =
                    "No se encontró una zona activa para la ubicación seleccionada.";

                resultadoZona.classList.remove("d-none");
                zonaSinDetectar.classList.add("d-none");

                nombreZona.textContent =
                    "Fuera de cobertura";

                estadoCobertura.className =
                    "geo-status geo-status-inactive";

                estadoCobertura.innerHTML =
                    '<i class="fa fa-ban"></i> No disponible';

                return;
            }

            idZonaInput.value = datos.idZona;

            nombreZona.textContent = datos.nombre;

            distanciaZona.textContent =
                `${Number(datos.distancia).toFixed(2)} km`;

            radioZona.textContent =
                datos.radio !== null
                    ? `${Number(datos.radio).toFixed(2)} km`
                    : "Sin límite";

            valoresZona = {
                costo: Number(datos.costo || 0),
                tarifaKm: Number(datos.tarifaKm || 0),
                costoPorDistancia:
                    Number(datos.costoPorDistancia || 0),
                costoDesplazamiento:
                    Number(datos.costoDesplazamiento || 0)
            };

            actualizarValoresMoneda();

            resultadoZona.classList.remove("d-none");
            zonaSinDetectar.classList.add("d-none");

            if (datos.dentroRadio) {
                estadoCobertura.className =
                    "geo-status geo-status-active";

                estadoCobertura.innerHTML =
                    '<i class="fa fa-check"></i> Dentro de cobertura';

                mensajeZona.textContent =
                    "La ubicación está dentro del área de cobertura. El costo mostrado corresponde únicamente al desplazamiento del profesor.";
            } else {
                estadoCobertura.className =
                    "geo-status geo-status-inactive";

                estadoCobertura.innerHTML =
                    '<i class="fa fa-ban"></i> Fuera de cobertura';

                mensajeZona.textContent =
                    "La ubicación supera el radio máximo de desplazamiento permitido.";

                idZonaInput.value = "";
            }
        } catch (error) {
            console.error(error);

            mensajeZona.textContent =
                "Ocurrió un problema al detectar la zona. Intente nuevamente.";

            resultadoZona.classList.remove("d-none");
            zonaSinDetectar.classList.add("d-none");
        }
    }
    if (
        !mapaElemento ||
        !latitudInput ||
        !longitudInput ||
        typeof L === "undefined"
    ) {
        return;
    }
    const iconoMarcador = L.icon({
        iconUrl:
            "https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/images/marker-icon.png",

        iconRetinaUrl:
            "https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/images/marker-icon-2x.png",

        shadowUrl:
            "https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/images/marker-shadow.png",

        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    function convertirNumero(valor) {
        if (!valor) {
            return Number.NaN;
        }

        return Number.parseFloat(
            valor.toString().replace(",", ".")
        );
    }

    const latitudGuardada =
        convertirNumero(latitudInput.value);

    const longitudGuardada =
        convertirNumero(longitudInput.value);

    const tieneUbicacion =
        Number.isFinite(latitudGuardada) &&
        Number.isFinite(longitudGuardada) &&
        !(latitudGuardada === 0 && longitudGuardada === 0);

    const centroCostaRica = [9.9281, -84.0907];

    const mapa = L.map("mapaAlumno").setView(
        tieneUbicacion
            ? [latitudGuardada, longitudGuardada]
            : centroCostaRica,
        tieneUbicacion ? 16 : 11
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

    function seleccionarUbicacion(latitud, longitud) {
        const latitudFormateada =
            Number(latitud).toFixed(7);

        const longitudFormateada =
            Number(longitud).toFixed(7);

        latitudInput.value =
            latitudFormateada;

        longitudInput.value =
            longitudFormateada;

        textoLatitud.textContent =
            latitudFormateada;

        textoLongitud.textContent =
            longitudFormateada;

        errorUbicacion.classList.add("d-none");

        detectarZona(
            latitudFormateada,
            longitudFormateada
        );

        if (marcador === null) {
            marcador = L.marker(
                [latitud, longitud],
                {
                    draggable: true,
                    icon: iconoMarcador
                }
            ).addTo(mapa);

            marcador
                .bindPopup("Mi ubicación")
                .openPopup();

            marcador.on("dragend", function (evento) {
                const posicion =
                    evento.target.getLatLng();

                seleccionarUbicacion(
                    posicion.lat,
                    posicion.lng
                );
            });
        } else {
            marcador.setLatLng(
                [latitud, longitud]
            );
        }
    }

    if (tieneUbicacion) {
        seleccionarUbicacion(
            latitudGuardada,
            longitudGuardada
        );
    }

    mapa.on("click", function (evento) {
        seleccionarUbicacion(
            evento.latlng.lat,
            evento.latlng.lng
        );
    });

    botonUbicacion.addEventListener("click", function () {
        if (!navigator.geolocation) {
            alert(
                "El navegador no permite obtener su ubicación."
            );

            return;
        }

        botonUbicacion.disabled = true;

        botonUbicacion.innerHTML =
            '<i class="fa fa-spinner fa-spin"></i> Buscando...';

        navigator.geolocation.getCurrentPosition(
            function (posicion) {
                const latitud =
                    posicion.coords.latitude;

                const longitud =
                    posicion.coords.longitude;

                seleccionarUbicacion(
                    latitud,
                    longitud
                );

                mapa.setView(
                    [latitud, longitud],
                    17
                );

                restaurarBotonUbicacion();
            },
            function () {
                alert(
                    "No fue posible obtener su ubicación. Revise los permisos del navegador."
                );

                restaurarBotonUbicacion();
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );
    });

    function restaurarBotonUbicacion() {
        botonUbicacion.disabled = false;

        botonUbicacion.innerHTML =
            '<i class="fa fa-crosshairs"></i> Usar mi ubicación actual';
    }

    botonLimpiar.addEventListener("click", function () {
        latitudInput.value = "";
        longitudInput.value = "";

        textoLatitud.textContent =
            "No seleccionada";

        textoLongitud.textContent =
            "No seleccionada";

        limpiarZonaDetectada();

        if (marcador !== null) {
            mapa.removeLayer(marcador);
            marcador = null;
        }
    });

    formulario.addEventListener("submit", function (evento) {
        const latitud =
            latitudInput.value.trim();

        const longitud =
            longitudInput.value.trim();

        const idZona =
            idZonaInput ? idZonaInput.value.trim() : "";

        if (
            latitud === "" ||
            longitud === "" ||
            idZona === ""
        ) {
            evento.preventDefault();

            if (idZona === "") {
                errorUbicacion.textContent =
                    "La ubicación debe estar dentro de una zona de cobertura.";
            } else {
                errorUbicacion.textContent =
                    "Debe seleccionar una ubicación en el mapa.";
            }

            errorUbicacion.classList.remove("d-none");

            mapaElemento.scrollIntoView({
                behavior: "smooth",
                block: "center"
            });
        }
    });

    if (botonCambiarMoneda) {

        botonCambiarMoneda.addEventListener(
            "click",
            function () {

                monedaZona =
                    monedaZona === "CRC"
                        ? "USD"
                        : "CRC";

                actualizarValoresMoneda();
            }
        );
    }

    window.setTimeout(function () {
        mapa.invalidateSize();
    }, 150);
});