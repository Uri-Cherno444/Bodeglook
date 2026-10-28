// 04 · Detalle + Reseña: volver, pestañas, modal "Dejar una reseña" (04b), "Ver las N" y favoritos.
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

// ---------- Dejar una reseña (modal 04b) ----------
// <dialog> se cierra solo con Escape; la X y Cancelar lo cierran a mano.

const modalResena = document.getElementById("modalResena");
const formularioResena = document.getElementById("formularioResena");
const comentarioResena = document.getElementById("comentarioResena");
const botonDejarResena = document.getElementById("botonDejarResena");
const botonPublicarResena = document.getElementById("botonPublicarResena");
const puntajeGeneral = document.getElementById("puntajeGeneral");
const dimensiones = ["comida", "ambiente", "atencion", "precio"];

botonDejarResena.addEventListener("click", function () {
    modalResena.showModal();
});

document.getElementById("cerrarModalResena").addEventListener("click", function () {
    modalResena.close();
});

document.getElementById("cancelarResena").addEventListener("click", function () {
    modalResena.close();
});

// Clic en el fondo oscuro (fuera de la caja) también cierra
modalResena.addEventListener("click", function (evento) {
    if (evento.target === modalResena) modalResena.close();
});

function puntajeDe(dimension) {
    const elegido = formularioResena.querySelector('input[name="' + dimension + '"]:checked');
    return elegido ? Number(elegido.value) : 0;
}

// "Puntaje general": promedio de las dimensiones elegidas, con un decimal y coma ("4,0")
function actualizarPuntajeGeneral() {
    const elegidos = dimensiones.map(puntajeDe).filter(function (puntaje) { return puntaje > 0; });
    if (elegidos.length === 0) {
        puntajeGeneral.textContent = "–";
        return;
    }
    const promedio = elegidos.reduce(function (suma, puntaje) { return suma + puntaje; }, 0) / elegidos.length;
    puntajeGeneral.textContent = promedio.toFixed(1).replace(".", ",");
}

formularioResena.addEventListener("change", actualizarPuntajeGeneral);

function mostrarAviso(idAviso, mensaje) {
    const aviso = document.getElementById(idAviso);
    document.getElementById(idAviso + "Texto").textContent = mensaje;
    aviso.hidden = false;
    // Se vuelve a poner la clase para que la animación arranque de nuevo
    aviso.classList.remove("aviso-visible");
    void aviso.offsetWidth;
    aviso.classList.add("aviso-visible");
}

document.querySelectorAll(".aviso-cerrar").forEach(function (boton) {
    boton.addEventListener("click", function () {
        boton.closest(".aviso").hidden = true;
    });
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

    if (dimensiones.some(function (dimension) { return puntajeDe(dimension) === 0; })) {
        mostrarAviso("aviso", "Elegí un puntaje del 1 al 5 en Comida, Ambiente, Atención y Precio y valor.");
        return;
    }

    botonPublicarResena.disabled = true;
    botonPublicarResena.classList.add("cargando");

    try {
        const respuesta = await fetch("/Restaurantes/DejarResena", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                idRestaurante: idRestaurante,
                comida: puntajeDe("comida"),
                ambiente: puntajeDe("ambiente"),
                atencion: puntajeDe("atencion"),
                precio: puntajeDe("precio"),
                comentario: comentarioResena.value,
                esCanje: formularioResena.querySelector('input[name="visita"]:checked').value === "canje"
            })
        });
        if (irAlLoginSiHaceFalta(respuesta)) return;

        const resultado = await respuesta.json();
        if (resultado.ok) {
            modalResena.close();
            // Se deja una sola reseña por lugar
            botonDejarResena.disabled = true;
            botonDejarResena.textContent = "Ya dejaste una reseña";

            if (resultado.publicada) {
                // La nueva es la más reciente: va primera en la lista
                const tarjeta = crearTarjetaResena(resultado.resena);
                tarjeta.classList.add("resena-nueva");
                listaResenas.prepend(tarjeta);

                cantidadResenas = resultado.cantidad;
                actualizarVerResenas();
                actualizarPuntaje(resultado.puntaje, resultado.textoCantidad);
            } else {
                mostrarAviso("avisoDetalle", "Tu reseña se publica 24 horas después de la visita.");
            }
        } else {
            mostrarAviso("aviso", resultado.mensaje || "No se pudo guardar la reseña. Probá de nuevo.");
        }
    } catch (error) {
        mostrarAviso("aviso", "No se pudo guardar la reseña. Revisá tu conexión y probá de nuevo.");
    }

    botonPublicarResena.disabled = false;
    botonPublicarResena.classList.remove("cargando");
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
