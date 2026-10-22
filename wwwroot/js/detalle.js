// 04 · Detalle + Reseña: volver, pestañas, puntaje, dejar reseña, "Ver las N" y favoritos.
// Todo se manda con fetch y se actualiza sin recargar.

const detalle = document.querySelector(".detalle");
const idRestaurante = Number(detalle.dataset.idRestaurante);

// Si la sesión se venció, el servidor responde 401: al login
function irAlLoginSiHaceFalta(respuesta) {
    if (respuesta.status === 401) {
        window.location.href = "/Cuenta/IniciarSesion";
        return true;
    }
    return false;
}

// ---------- Volver ----------
// Vuelve a la pantalla anterior. Si se entró directo (sin pantalla anterior de Bodeglook), va a Inicio.

document.getElementById("botonVolver").addEventListener("click", function (evento) {
    if (document.referrer.startsWith(window.location.origin) && window.history.length > 1) {
        evento.preventDefault();
        window.history.back();
    }
});

// ---------- Pestañas ----------
// Cada bloque tiene data-pestanas con las pestañas en las que se ve

const pestanas = document.querySelectorAll(".pestana");

pestanas.forEach(function (pestana) {
    pestana.addEventListener("click", function () {
        pestanas.forEach(function (otra) {
            otra.classList.remove("activa");
            otra.setAttribute("aria-selected", "false");
        });
        pestana.classList.add("activa");
        pestana.setAttribute("aria-selected", "true");

        const elegida = pestana.dataset.pestana;
        document.querySelectorAll("[data-pestanas]").forEach(function (bloque) {
            bloque.hidden = !bloque.dataset.pestanas.split(" ").includes(elegida);
        });
    });
});

// ---------- Barra de puntaje ----------

const barraPuntaje = document.getElementById("barraPuntaje");
const puntos = barraPuntaje.querySelectorAll(".barra-puntaje-punto");
const etiquetaPuntaje = document.getElementById("etiquetaPuntaje");
const puntajeInicial = 4;
let puntajeElegido = puntajeInicial;

function elegirPuntaje(puntaje) {
    puntajeElegido = puntaje;
    barraPuntaje.style.setProperty("--posicion", (puntaje - 1) / 4);

    puntos.forEach(function (punto) {
        const valor = Number(punto.dataset.puntaje);
        punto.classList.toggle("pintado", valor <= puntaje);
        punto.setAttribute("aria-checked", valor === puntaje ? "true" : "false");
        punto.tabIndex = valor === puntaje ? 0 : -1;
        if (valor === puntaje) etiquetaPuntaje.textContent = punto.getAttribute("aria-label");
    });
}

puntos.forEach(function (punto) {
    punto.addEventListener("click", function () {
        elegirPuntaje(Number(punto.dataset.puntaje));
    });
});

// Con las flechas del teclado también se cambia el puntaje
barraPuntaje.addEventListener("keydown", function (evento) {
    let nuevo = puntajeElegido;
    if (evento.key === "ArrowRight" || evento.key === "ArrowUp") nuevo = Math.min(5, puntajeElegido + 1);
    if (evento.key === "ArrowLeft" || evento.key === "ArrowDown") nuevo = Math.max(1, puntajeElegido - 1);
    if (nuevo === puntajeElegido) return;

    evento.preventDefault();
    elegirPuntaje(nuevo);
    puntos[nuevo - 1].focus();
});

// ---------- Reseñas de la comunidad ----------

const listaResenas = document.getElementById("listaResenas");
const sinResenas = document.getElementById("sinResenas");
const botonVerResenas = document.getElementById("botonVerResenas");
let cantidadResenas = Number(botonVerResenas.textContent.replace(/\D/g, ""));

// Arma la tarjeta igual que la de Razor. textContent para que el texto no se interprete como HTML.
function crearTarjetaResena(resena) {
    const tarjeta = document.createElement("div");
    tarjeta.className = "tarjeta-resena";
    tarjeta.innerHTML =
        '<div class="resena-autor">' +
        '  <span class="avatar"></span>' +
        '  <div class="resena-datos"><span class="resena-nombre"></span><span class="resena-fecha"></span></div>' +
        '  <div class="estrellas"></div>' +
        '</div>';

    tarjeta.querySelector(".avatar").textContent = resena.iniciales;
    tarjeta.querySelector(".resena-nombre").textContent = resena.nombre;
    tarjeta.querySelector(".resena-fecha").textContent = resena.hace;

    const estrellas = tarjeta.querySelector(".estrellas");
    estrellas.setAttribute("aria-label", resena.puntaje + " de 5");
    for (let estrella = 1; estrella <= 5; estrella++) {
        const imagen = document.createElement("img");
        imagen.src = estrella <= resena.puntaje ? "/img/icono-estrella-llena.svg" : "/img/icono-estrella-vacia.svg";
        imagen.width = 16;
        imagen.height = 16;
        imagen.alt = "";
        estrellas.appendChild(imagen);
    }

    if (resena.comentario) {
        const texto = document.createElement("p");
        texto.className = "resena-texto";
        texto.textContent = resena.comentario;
        tarjeta.appendChild(texto);
    }
    return tarjeta;
}

// "Ver las N" se ve mientras haya reseñas sin mostrar
function actualizarVerResenas() {
    botonVerResenas.textContent = "Ver las " + cantidadResenas;
    botonVerResenas.hidden = listaResenas.children.length >= cantidadResenas;
    sinResenas.hidden = listaResenas.children.length > 0;
}

botonVerResenas.addEventListener("click", async function () {
    botonVerResenas.disabled = true;
    try {
        const respuesta = await fetch("/Restaurantes/Resenas/" + idRestaurante + "?desde=" + listaResenas.children.length);
        if (irAlLoginSiHaceFalta(respuesta)) return;

        const resenas = await respuesta.json();
        resenas.forEach(function (resena) {
            listaResenas.appendChild(crearTarjetaResena(resena));
        });
        actualizarVerResenas();
    } catch (error) {
        // Si falla la conexión, el botón queda para volver a probar
    }
    botonVerResenas.disabled = false;
});

// ---------- Dejar una reseña ----------

const formularioResena = document.getElementById("formularioResena");
const comentarioResena = document.getElementById("comentarioResena");
const botonEnviarResena = document.getElementById("botonEnviarResena");

function mostrarAviso(mensaje) {
    const aviso = document.getElementById("aviso");
    document.getElementById("avisoTexto").textContent = mensaje;
    aviso.hidden = false;
    // Se vuelve a poner la clase para que la animación arranque de nuevo
    aviso.classList.remove("aviso-visible");
    void aviso.offsetWidth;
    aviso.classList.add("aviso-visible");
}

document.querySelector(".aviso-cerrar").addEventListener("click", function () {
    document.getElementById("aviso").hidden = true;
});

// El puntaje aparece en el banner y al lado del nombre
function actualizarPuntaje(puntaje, textoCantidad) {
    document.querySelectorAll(".texto-puntaje").forEach(function (texto) {
        texto.textContent = puntaje;
    });
    document.getElementById("puntajeBanner").hidden = false;
    document.getElementById("puntajeTitulo").hidden = false;
    document.getElementById("cantidadResenas").textContent = textoCantidad;
}

formularioResena.addEventListener("submit", async function (evento) {
    evento.preventDefault();
    document.getElementById("aviso").hidden = true;
    botonEnviarResena.disabled = true;
    botonEnviarResena.classList.add("cargando");

    try {
        const respuesta = await fetch("/Restaurantes/DejarResena", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                idRestaurante: idRestaurante,
                puntaje: puntajeElegido,
                comentario: comentarioResena.value
            })
        });
        if (irAlLoginSiHaceFalta(respuesta)) return;

        const resultado = await respuesta.json();
        if (resultado.ok) {
            // La nueva es la más reciente: va primera en la lista
            const tarjeta = crearTarjetaResena(resultado.resena);
            tarjeta.classList.add("resena-nueva");
            listaResenas.prepend(tarjeta);

            cantidadResenas = resultado.cantidad;
            actualizarVerResenas();
            actualizarPuntaje(resultado.puntaje, resultado.textoCantidad);

            comentarioResena.value = "";
            elegirPuntaje(puntajeInicial);
        } else {
            mostrarAviso("No se pudo guardar la reseña. Probá de nuevo.");
        }
    } catch (error) {
        mostrarAviso("No se pudo guardar la reseña. Revisá tu conexión y probá de nuevo.");
    }

    botonEnviarResena.disabled = false;
    botonEnviarResena.classList.remove("cargando");
});

// ---------- Favoritos ----------
// El corazón del banner y "Agregar a favoritos" usan la misma función que Inicio (/Home/AlternarFavorito)

const corazonBanner = document.querySelector(".detalle-banner-favorito");
const botonFavorito = document.getElementById("botonFavorito");

function pintarFavorito(esFavorito) {
    corazonBanner.classList.toggle("es-favorito", esFavorito);
    corazonBanner.setAttribute("aria-pressed", esFavorito ? "true" : "false");
    corazonBanner.setAttribute("aria-label", esFavorito ? "Sacar de favoritos" : "Agregar a favoritos");

    botonFavorito.setAttribute("aria-pressed", esFavorito ? "true" : "false");
    botonFavorito.textContent = esFavorito ? "Sacar de favoritos" : "Agregar a favoritos";
}

async function alternarFavorito() {
    corazonBanner.disabled = true;
    botonFavorito.disabled = true;

    try {
        const respuesta = await fetch("/Home/AlternarFavorito", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ idRestaurante: idRestaurante })
        });
        if (irAlLoginSiHaceFalta(respuesta)) return;

        const resultado = await respuesta.json();
        if (resultado.ok) pintarFavorito(resultado.esFavorito);
    } catch (error) {
        // Si falla la conexión, queda como estaba
    }

    corazonBanner.disabled = false;
    botonFavorito.disabled = false;
}

corazonBanner.addEventListener("click", alternarFavorito);
botonFavorito.addEventListener("click", alternarFavorito);
