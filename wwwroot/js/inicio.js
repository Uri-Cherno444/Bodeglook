// 03 · Inicio: el corazón de cada tarjeta agrega o saca de favoritos con fetch
// y actualiza "Tus favoritos" sin recargar.

const grillaFavoritos = document.getElementById("grillaFavoritos");
const sinFavoritos = document.getElementById("sinFavoritos");

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
    const enFavoritos = grillaFavoritos.querySelector('[data-id-restaurante="' + idRestaurante + '"]');

    if (esFavorito && !enFavoritos) {
        // Se copia la tarjeta que se tocó y se suma al final (los favoritos van en el orden en que se guardaron)
        const copia = tarjeta.cloneNode(true);
        copia.querySelector(".boton-favorito").disabled = false;
        grillaFavoritos.appendChild(copia);
    }

    if (!esFavorito && enFavoritos) {
        enFavoritos.classList.add("tarjeta-saliendo");
        setTimeout(function () {
            enFavoritos.remove();
            sinFavoritos.hidden = grillaFavoritos.children.length > 0;
        }, 250);
    }

    sinFavoritos.hidden = grillaFavoritos.children.length > 0;
}
