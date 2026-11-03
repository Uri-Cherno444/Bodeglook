// 02c · Onboarding · Gustos y 02d · Filtro dietario.
// Los chips se marcan con aria-pressed. Al seguir se mandan los datos con fetch en JSON
// y el servidor contesta a qué página ir. Usa mostrarAviso() de cuenta.js para los errores.

// Paso 1
function prepararGustos() {
    // Cada grupo de chips: "varias" deja marcar muchos, "una" deja marcar uno solo
    document.querySelectorAll(".onboarding-chips").forEach(function (grupo) {
        grupo.querySelectorAll(".chip-opcion").forEach(function (chip) {
            chip.addEventListener("click", function () {
                const marcado = chip.getAttribute("aria-pressed") === "true";

                if (grupo.dataset.tipo === "una") {
                    grupo.querySelectorAll(".chip-opcion").forEach(function (otro) {
                        otro.setAttribute("aria-pressed", "false");
                    });
                }
                chip.setAttribute("aria-pressed", marcado ? "false" : "true");

                if (grupo.id === "chipsRestricciones" && !marcado) {
                    desmarcarSegunNinguna(grupo, chip);
                }
            });
        });
    });

    document.querySelector(".aviso-cerrar").addEventListener("click", ocultarAviso);

    const boton = document.getElementById("botonSeguir");
    boton.addEventListener("click", function () {
        // Se manda todo lo marcado. "Ninguna" no tiene data-id, por eso no cuenta como restricción.
        const restricciones = [];
        document.querySelectorAll("#chipsRestricciones .chip-opcion[data-id][aria-pressed=true]").forEach(function (chip) {
            restricciones.push(Number(chip.dataset.id));
        });
        const zonas = [];
        document.querySelectorAll("#chipsZonas .chip-opcion[aria-pressed=true]").forEach(function (chip) {
            zonas.push(Number(chip.dataset.id));
        });
        const frecuencia = document.querySelector("#chipsFrecuencia .chip-opcion[aria-pressed=true]");
        const companias = [];
        document.querySelectorAll("#chipsCompania .chip-opcion[aria-pressed=true]").forEach(function (chip) {
            companias.push(chip.dataset.valor);
        });

        enviar(boton, "/Cuenta/GuardarGustos", {
            restricciones: restricciones,
            zonas: zonas,
            frecuencia: frecuencia ? frecuencia.dataset.valor : null,
            companias: companias
        });
    });
}

// "Ninguna" desmarca las demás restricciones, y marcar otra desmarca "Ninguna"
function desmarcarSegunNinguna(grupo, chipMarcado) {
    const ninguna = grupo.querySelector("[data-ninguna]");

    if (chipMarcado === ninguna) {
        grupo.querySelectorAll(".chip-opcion[data-id]").forEach(function (chip) {
            chip.setAttribute("aria-pressed", "false");
        });
    } else {
        ninguna.setAttribute("aria-pressed", "false");
    }
}

// Paso 2
function prepararFiltroDietario() {
    document.querySelector(".aviso-cerrar").addEventListener("click", ocultarAviso);

    const boton = document.getElementById("botonEntrar");
    boton.addEventListener("click", function () {
        const elegido = document.querySelector("input[name=filtro]:checked");
        enviar(boton, "/Cuenta/GuardarFiltro", elegido.value === "apto");
    });
}

async function enviar(boton, url, datos) {
    ocultarAviso();
    boton.disabled = true;
    boton.classList.add("cargando");

    try {
        const respuesta = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(datos)
        });
        const resultado = await respuesta.json();

        if (resultado.ok) {
            window.location.href = resultado.destino;
            return;
        }
        mostrarAviso("No pudimos guardar tus preferencias. Probá de nuevo.");
    } catch (error) {
        mostrarAviso("No pudimos conectarnos. Probá de nuevo en un rato.");
    }

    boton.disabled = false;
    boton.classList.remove("cargando");
}
