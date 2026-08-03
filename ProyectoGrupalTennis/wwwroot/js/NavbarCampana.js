document.addEventListener("DOMContentLoaded", function () {

    const badge = document.getElementById("campanaBadge");
    const lista = document.getElementById("campanaLista");

    if (!badge || !lista)
        return;

    cargarResumenNotificaciones();
    setInterval(cargarResumenNotificaciones, 60000);

    async function cargarResumenNotificaciones() {
        try {
            const resp = await fetch("/Notificaciones/Resumen");

            if (!resp.ok) {
                lista.innerHTML =
                    '<li class="campana-item text-danger text-center">No se pudieron cargar las notificaciones.</li>';
                return;
            }

            const data = await resp.json();

            pintarBadge(data.noLeidas);
            pintarLista(data.notificaciones);
        }
        catch (err) {
            console.error(
                "No se pudo cargar el resumen de notificaciones",
                err
            );

            lista.innerHTML =
                '<li class="campana-item text-danger text-center">No se pudieron cargar las notificaciones.</li>';
        }
    }

    function pintarBadge(noLeidas) {
        if (noLeidas > 0) {
            badge.style.display = "inline-block";
            badge.innerText = noLeidas > 9 ? "9+" : noLeidas;
        }
        else {
            badge.style.display = "none";
        }
    }

    function pintarLista(notificaciones) {
        lista.innerHTML = "";

        if (!notificaciones || notificaciones.length === 0) {
            lista.innerHTML =
                '<li class="campana-item text-muted text-center">No tienes notificaciones.</li>';

            return;
        }

        notificaciones.forEach(n => {
            const li = document.createElement("li");

            li.className =
                "campana-item" +
                (n.leida ? "" : " campana-no-leida");

            li.innerHTML = `
                <div class="campana-item-contenido">
                    <strong>${escapeHtml(n.titulo)}</strong>
                    <p>${escapeHtml(n.mensaje)}</p>
                    <small>${escapeHtml(n.fecha)}</small>
                </div>

                <button type="button"
                        class="campana-eliminar"
                        title="Eliminar">

                    <i class="fa fa-trash"></i>

                </button>
            `;

            li.querySelector(".campana-eliminar")
                .addEventListener("click", function (e) {
                    e.stopPropagation();

                    eliminarNotificacion(
                        n.id,
                        li
                    );
                });

            lista.appendChild(li);
        });

        const verTodas = document.createElement("li");

        verTodas.className = "campana-ver-todas";

        verTodas.innerHTML =
            '<a href="/Notificaciones">Ver historial completo</a>';

        lista.appendChild(verTodas);
    }

    async function eliminarNotificacion(id, elemento) {
        const token =
            document.querySelector(
                '#antiForgeryForm input[name="__RequestVerificationToken"]'
            )?.value;

        if (!token) {
            console.error(
                "No se encontró el token antiforgery."
            );

            return;
        }

        try {
            const resp = await fetch(
                "/Notificaciones/Eliminar",
                {
                    method: "POST",

                    headers: {
                        "X-Requested-With": "XMLHttpRequest",
                        "Content-Type":
                            "application/x-www-form-urlencoded"
                    },

                    body:
                        `idNotificacion=${encodeURIComponent(id)}` +
                        `&__RequestVerificationToken=${encodeURIComponent(token)}`
                }
            );

            if (!resp.ok) {
                console.error(
                    "No se pudo eliminar la notificación."
                );

                return;
            }

            const data = await resp.json();

            if (data.success) {
                elemento.remove();

                cargarResumenNotificaciones();
            }
        }
        catch (err) {
            console.error(
                "No se pudo eliminar la notificación",
                err
            );
        }
    }

    function escapeHtml(text) {
        const div = document.createElement("div");

        div.innerText = text ?? "";

        return div.innerHTML;
    }
});