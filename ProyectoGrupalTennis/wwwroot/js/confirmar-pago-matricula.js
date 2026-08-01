document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const formulario =
        document.getElementById("formConfirmarPago");

    const modalidadAcademia =
        document.getElementById("modalidadAcademia");

    const modalidadDomicilio =
        document.getElementById("modalidadDomicilio");

    const detalleDomicilio =
        document.getElementById("detalleDomicilio");

    const filaDesplazamiento =
        document.getElementById("filaDesplazamiento");

    const aceptacionDomicilio =
        document.getElementById("aceptacionDomicilio");

    const aceptaCosto =
        document.getElementById("aceptaCostoDomicilio");

    const errorAceptacion =
        document.getElementById("errorAceptacion");

    const montoTotal =
        document.getElementById("montoTotal");

    if (
        !formulario ||
        !modalidadAcademia ||
        !montoTotal
    ) {
        return;
    }

    const montoBase =
        Number.parseFloat(
            montoTotal.dataset.montoBase || "0"
        );

    const costoDesplazamiento =
        Number.parseFloat(
            montoTotal.dataset.costoDesplazamiento || "0"
        );

    function formatearColones(valor) {
        return new Intl.NumberFormat(
            "es-CR",
            {
                style: "currency",
                currency: "CRC",
                minimumFractionDigits: 2
            }
        ).format(valor);
    }

    function actualizarModalidad() {
        const esDomicilio =
            modalidadDomicilio &&
            modalidadDomicilio.checked;

        if (detalleDomicilio) {
            detalleDomicilio.classList.toggle(
                "d-none",
                !esDomicilio
            );
        }

        if (filaDesplazamiento) {
            filaDesplazamiento.classList.toggle(
                "d-none",
                !esDomicilio
            );
        }

        if (aceptacionDomicilio) {
            aceptacionDomicilio.classList.toggle(
                "d-none",
                !esDomicilio
            );
        }

        if (!esDomicilio && aceptaCosto) {
            aceptaCosto.checked = false;
        }

        if (errorAceptacion) {
            errorAceptacion.classList.add("d-none");
        }

        const total =
            esDomicilio
                ? montoBase + costoDesplazamiento
                : montoBase;

        montoTotal.textContent =
            formatearColones(total);
    }

    modalidadAcademia.addEventListener(
        "change",
        actualizarModalidad
    );

    if (modalidadDomicilio) {
        modalidadDomicilio.addEventListener(
            "change",
            actualizarModalidad
        );
    }

    formulario.addEventListener(
        "submit",
        function (evento) {
            const esDomicilio =
                modalidadDomicilio &&
                modalidadDomicilio.checked;

            if (
                esDomicilio &&
                aceptaCosto &&
                !aceptaCosto.checked
            ) {
                evento.preventDefault();

                errorAceptacion.classList.remove(
                    "d-none"
                );

                aceptacionDomicilio.scrollIntoView({
                    behavior: "smooth",
                    block: "center"
                });
            }
        }
    );

    actualizarModalidad();
});