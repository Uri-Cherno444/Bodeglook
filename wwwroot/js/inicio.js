// 03 · Inicio: el corazón de cada tarjeta agrega o saca de favoritos con fetch
// y actualiza "Tus favoritos" sin recargar. Los chips de restricción cambian las dos grillas con fetch.

const lugaresInicio = document.getElementById("lugaresInicio");

// Las grillas se reemplazan al cambiar de chip: por eso se buscan cada vez
function grillaFavoritos() {
    return document.getElementById("grillaFavoritos");
}

function sinFavoritos() {
    return document.getElementById("sinFavoritos");
}

// ---------- Chips de restricción ----------

document.querySelectorAll(".filtros-rapidos .chip-filtro").forEach(function (chip) {
    chip.addEventListener("click", async function (evento) {
        evento.preventDefault();
        if (chip.classList.contains("chip-activo")) return;

        const restriccion = chip.dataset.restriccion;
        try {
            const respuesta = await fetch("/Home/LugaresInicio" + (restriccion === "" ? "" : "?restriccion=" + restriccion));

            // Si se venció la sesión, al login
            if (respuesta.status === 401) {
                window.location.href = "/Cuenta/IniciarSesion";
                return;
            }

            lugaresInicio.innerHTML = await respuesta.text();
            marcarChip(chip);
            // La dirección queda con el chip elegido (así al volver a entrar se ve igual), sin recargar
            history.replaceState(null, "", chip.getAttribute("href"));
        } catch (error) {
            // Si falla la conexión, queda el chip que estaba
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

// ---------- Favoritos ----------

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

        // Si se venció la sesión, al login
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
        // Si falla la conexión, el corazón queda como estaba
    }

    boton.disabled = false;
});

// El mismo lugar puede estar en "Recomendado para vos" y en "Tus favoritos": se pintan todos sus corazones
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
        // Se copia la tarjeta que se tocó y se suma al final (los favoritos van en el orden en que se guardaron)
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
