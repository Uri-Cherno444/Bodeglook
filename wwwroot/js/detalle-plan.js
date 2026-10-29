// 07h / 07i · Detalle del plan.
// Cada 3 segundos se vuelve a pedir la parte que cambia (votos, participantes y estado), igual que el chat.
// El creador confirma o cancela con el diálogo del kit.

const detallePlan = document.getElementById("detallePlan");
const contenidoPlan = document.getElementById("contenidoPlan");
const idPlan = Number(detallePlan.dataset.idPlan);

// Si la sesión se cerró o ya no es integrante, se va de la página
function revisarRespuesta(respuesta) {
    if (respuesta.status === 401) window.location.href = "/Cuenta/IniciarSesion";
    if (respuesta.status === 403) window.location.href = "/Grupos";
    if (!respuesta.ok) throw new Error("Error " + respuesta.status);
    return respuesta;
}

let ultimoHtml = null;
let consultando = false;

async function actualizar() {
    // Con un diálogo abierto no se reemplaza nada, así no se cierra mientras el creador elige
    if (consultando || contenidoPlan.querySelector("dialog[open]")) return;
    consultando = true;
    try {
        const respuesta = revisarRespuesta(await fetch("/Planes/DetalleParcial/" + idPlan));
        const html = await respuesta.text();
        if (ultimoHtml !== null && html !== ultimoHtml && !contenidoPlan.querySelector("dialog[open]")) {
            contenidoPlan.innerHTML = html;
        }
        ultimoHtml = html;
    } catch (error) {
        // Si falla una vez, se reintenta en la próxima vuelta
    }
    consultando = false;
}

setInterval(actualizar, 3000);
actualizar();

// ---------- Confirmar lugar y horario ----------

// "Bodega Los Alamos · Sáb 24/10 a las 21:00". Si hay empate y todavía no eligió, queda deshabilitado.
function actualizarResumen(dialogo) {
    const lugar = dialogo.querySelector('[name="lugarElegido"]:checked, input[type="hidden"][name="lugarElegido"]');
    const horario = dialogo.querySelector('[name="horarioElegido"]:checked, input[type="hidden"][name="horarioElegido"]');
    dialogo.querySelector("#resumenConfirmar").textContent =
        (lugar ? lugar.dataset.texto : "Elegí el lugar") + " · " + dialogo.dataset.dia +
        (horario ? " a las " + horario.dataset.texto : " · Elegí el horario");
    dialogo.querySelector("#confirmarPlan").disabled = !(lugar && horario);
    return { lugar: lugar, horario: horario };
}

async function mandar(url, datos, error) {
    error.hidden = true;
    const respuesta = revisarRespuesta(await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(datos)
    }));
    const resultado = await respuesta.json();
    if (!resultado.ok) {
        error.textContent = resultado.mensaje;
        error.hidden = false;
        return false;
    }
    return true;
}

// El contenido se reemplaza cada 3 segundos, por eso los clics se escuchan en el contenedor
contenidoPlan.addEventListener("click", async function (evento) {
    const boton = evento.target.closest("button");
    if (!boton) return;

    if (boton.id === "abrirConfirmarPlan") {
        const dialogo = document.getElementById("dialogoConfirmarPlan");
        dialogo.querySelector("#errorConfirmar").hidden = true;
        actualizarResumen(dialogo);
        dialogo.showModal();
    }

    if (boton.id === "abrirCancelarPlan") {
        const dialogo = document.getElementById("dialogoCancelarPlan");
        dialogo.querySelector("#errorCancelar").hidden = true;
        dialogo.showModal();
    }

    if (boton.hasAttribute("data-cerrar")) {
        boton.closest("dialog").close();
    }

    if (boton.id === "confirmarPlan") {
        const dialogo = document.getElementById("dialogoConfirmarPlan");
        const elegidos = actualizarResumen(dialogo);
        boton.disabled = true;
        boton.classList.add("cargando");
        const ok = await mandar("/Planes/Confirmar", {
            idPlan: idPlan,
            idRestaurante: Number(elegidos.lugar.value),
            idHorario: Number(elegidos.horario.value)
        }, dialogo.querySelector("#errorConfirmar"));
        boton.classList.remove("cargando");
        boton.disabled = false;
        if (ok) {
            dialogo.close();
            actualizar();
        }
    }

    if (boton.id === "cancelarPlan") {
        const dialogo = document.getElementById("dialogoCancelarPlan");
        boton.disabled = true;
        boton.classList.add("cargando");
        const ok = await mandar("/Planes/Cancelar", { idPlan: idPlan }, dialogo.querySelector("#errorCancelar"));
        boton.classList.remove("cargando");
        boton.disabled = false;
        if (ok) {
            dialogo.close();
            actualizar();
        }
    }
});

// Al elegir en un empate se actualiza el resumen del diálogo
contenidoPlan.addEventListener("change", function (evento) {
    if (evento.target.closest("#dialogoConfirmarPlan")) {
        actualizarResumen(document.getElementById("dialogoConfirmarPlan"));
    }
});
