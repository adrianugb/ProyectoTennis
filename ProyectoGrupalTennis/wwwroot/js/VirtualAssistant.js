document.addEventListener("DOMContentLoaded", function () {
    const toggle = document.getElementById("vaToggle");
    const panel = document.getElementById("vaPanel");
    const closeBtn = document.getElementById("vaClose");
    const messages = document.getElementById("vaMessages");
    const quickReplies = document.getElementById("vaQuickReplies");
    const input = document.getElementById("vaInput");
    const sendBtn = document.getElementById("vaSend");

    if (!toggle || !panel) return;

    let preguntasCargadas = false;

    toggle.addEventListener("click", function () {
        const abierto = panel.style.display === "flex";
        panel.style.display = abierto ? "none" : "flex";

        if (!abierto && !preguntasCargadas) {
            cargarPreguntasRapidas();
        }
    });

    closeBtn.addEventListener("click", function () {
        panel.style.display = "none";
    });

    sendBtn.addEventListener("click", enviarMensaje);

    input.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            enviarMensaje();
        }
    });

    async function cargarPreguntasRapidas() {
        try {
            const resp = await fetch("/Chatbot/PreguntasRapidas");
            if (!resp.ok) throw new Error("No se pudieron cargar las preguntas");

            const preguntas = await resp.json();
            preguntasCargadas = true;

            quickReplies.innerHTML = "";

            if (!preguntas || preguntas.length === 0) {
                quickReplies.style.display = "none";
                return;
            }

            preguntas.forEach(function (p) {
                const btn = document.createElement("button");
                btn.type = "button";
                btn.className = "va-quick";
                btn.textContent = p.pregunta;
                btn.addEventListener("click", function () {
                    enviarMensaje(p.pregunta);
                });
                quickReplies.appendChild(btn);
            });
        } catch (err) {
            quickReplies.innerHTML = "";
            console.error("Error cargando preguntas rápidas del chatbot", err);
        }
    }

    async function enviarMensaje(textoForzado) {
        const texto = (textoForzado ?? input.value ?? "").trim();
        if (!texto) return;

        agregarBurbuja(texto, "outgoing");
        input.value = "";

        const indicador = agregarTyping();

        try {
            const resp = await fetch("/Chatbot/Preguntar", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ mensaje: texto })
            });

            quitarTyping(indicador);

            if (!resp.ok) {
                agregarBurbuja(
                    "Tuve un problema para procesar tu consulta. Intenta de nuevo en un momento.",
                    "incoming"
                );
                return;
            }

            const data = await resp.json();
            agregarBurbuja(data.respuesta, "incoming", data.whatsappUrl);
        } catch (err) {
            quitarTyping(indicador);
            agregarBurbuja(
                "No pude conectarme en este momento. Intenta de nuevo en unos segundos.",
                "incoming"
            );
            console.error("Error enviando mensaje al chatbot", err);
        }
    }

    function agregarBurbuja(texto, tipo, whatsappUrl) {
        const wrapper = document.createElement("div");
        wrapper.className = "va-message " + tipo;

        const bubble = document.createElement("div");
        bubble.className = "va-bubble";
        bubble.textContent = texto;

        const meta = document.createElement("div");
        meta.className = "va-meta";
        meta.textContent = (tipo === "outgoing" ? "Tú" : "Asistente") + " • ahora";
        bubble.appendChild(meta);

        if (whatsappUrl) {
            const link = document.createElement("a");
            link.href = whatsappUrl;
            link.target = "_blank";
            link.rel = "noopener";
            link.className = "va-whatsapp-btn";
            link.innerHTML = '<i class="fab fa-whatsapp"></i> Escribir por WhatsApp';
            bubble.appendChild(link);
        }

        wrapper.appendChild(bubble);
        messages.appendChild(wrapper);
        messages.scrollTop = messages.scrollHeight;
    }

    function agregarTyping() {
        const wrapper = document.createElement("div");
        wrapper.className = "va-message incoming";
        wrapper.innerHTML = '<div class="va-bubble va-typing">Escribiendo...</div>';
        messages.appendChild(wrapper);
        messages.scrollTop = messages.scrollHeight;
        return wrapper;
    }

    function quitarTyping(elemento) {
        if (elemento && elemento.parentNode) {
            elemento.parentNode.removeChild(elemento);
        }
    }
});
