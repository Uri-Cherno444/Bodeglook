const lugaresInicio = document.getElementById("lugaresInicio");

function grillaFavoritos() {
    return document.getElementById("grillaFavoritos");
}

function sinFavoritos() {
    return document.getElementById("sinFavoritos");
}

document.querySelectorAll(".filtros-rapidos .chip-filtro").forEach(function (chip) {
    chip.addEventListener("click", async function (evento) {
        evento.preventDefault();
        if (chip.classList.contains("chip-activo")) return;

        const restriccion = chip.dataset.restriccion;
        try {
            const respuesta = await fetch("/Home/LugaresInicio" + (restriccion === "" ? "" : "?restriccion=" + restriccion));

            if (respuesta.status === 401) {
                window.location.href = "/Cuenta/IniciarSesion";
                return;
            }

            lugaresInicio.innerHTML = await respuesta.text();
            marcarChip(chip);
            history.replaceState(null, "", chip.getAttribute("href"));
        } catch (error) {
        }
    });
});

function marcarChip(elegido) {
    document.querySelectorAll(".filtros-rapidos .chip-filtro").forEach(function (chip) {
        const activo = chip === elegido;
        chip.classList.toggle("chip-activo", activo);
        chip.classList.toggle("chip-crema", !activo);
        if (activo) chip.setAttribute("aria-current", "true");
        else chip.removeAttribute("aria-current");
    });
}

document.addEventListener("click", async function (evento) {
    const boton = evento.target.closest(".boton-favorito");
    if (!boton) return;

    const tarjeta = boton.closest(".tarjeta-lugar");
    const idRestaurante = Number(tarjeta.dataset.idRestaurante);
    boton.disabled = true;

    try {
        const respuesta = await fetch("/Home/AlternarFavorito", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ idRestaurante: idRestaurante })
        });

        if (respuesta.status === 401) {
            window.location.href = "/Cuenta/IniciarSesion";
            return;
        }

        const resultado = await respuesta.json();
        if (resultado.ok) {
            actualizarCorazones(idRestaurante, resultado.esFavorito);
            actualizarListaFavoritos(tarjeta, idRestaurante, resultado.esFavorito);
        }
    } catch (error) {
    }

    boton.disabled = false;
});

function actualizarCorazones(idRestaurante, esFavorito) {
    document.querySelectorAll('.tarjeta-lugar[data-id-restaurante="' + idRestaurante + '"] .boton-favorito').forEach(function (corazon) {
        corazon.classList.toggle("es-favorito", esFavorito);
        corazon.setAttribute("aria-pressed", esFavorito ? "true" : "false");
        corazon.setAttribute("aria-label", esFavorito ? "Sacar de favoritos" : "Agregar a favoritos");
    });
}

function actualizarListaFavoritos(tarjeta, idRestaurante, esFavorito) {
    const grilla = grillaFavoritos();
    const enFavoritos = grilla.querySelector('[data-id-restaurante="' + idRestaurante + '"]');

    if (esFavorito && !enFavoritos) {
        const copia = tarjeta.cloneNode(true);
        copia.querySelector(".boton-favorito").disabled = false;
        grilla.appendChild(copia);
    }

    if (!esFavorito && enFavoritos) {
        enFavoritos.classList.add("tarjeta-saliendo");
        setTimeout(function () {
            enFavoritos.remove();
            sinFavoritos().hidden = grilla.children.length > 0;
        }, 250);
    }

    sinFavoritos().hidden = grilla.children.length > 0;
}
