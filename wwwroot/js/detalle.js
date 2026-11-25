const detalle = document.querySelector(".detalle");
const idRestaurante = Number(detalle.dataset.idRestaurante);

function irAlLoginSiHaceFalta(respuesta) {
    if (respuesta.status === 401) {
        window.location.href = "/Cuenta/IniciarSesion";
        return true;
    }
    return false;
}

document.getElementById("botonVolver").addEventListener("click", function (evento) {
    if (document.referrer.startsWith(window.location.origin) && window.history.length > 1) {
        evento.preventDefault();
        window.history.back();
    }
});

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

const visor = document.getElementById("visorFotos");
const visorFoto = document.getElementById("visorFoto");
const visorContador = document.getElementById("visorContador");
const fotosGaleria = Array.from(document.querySelectorAll(".galeria-foto img")).map(function (imagen) {
    return imagen.getAttribute("src");
});
let fotoActual = 0;

function mostrarFotoDelVisor(indice) {
    fotoActual = (indice + fotosGaleria.length) % fotosGaleria.length;
    visorFoto.src = fotosGaleria[fotoActual];
    visorContador.textContent = (fotoActual + 1) + " de " + fotosGaleria.length;
}

const visorAnterior = document.getElementById("visorAnterior");
const visorSiguiente = document.getElementById("visorSiguiente");

document.querySelectorAll(".galeria-foto").forEach(function (boton) {
    boton.addEventListener("click", function () {
        visorAnterior.hidden = fotosGaleria.length === 1;
        visorSiguiente.hidden = fotosGaleria.length === 1;
        visorContador.hidden = false;
        mostrarFotoDelVisor(Number(boton.dataset.indice));
        visor.showModal();
    });
});

function abrirFotoDeResena(ruta) {
    fotoActual = 0;
    visorAnterior.hidden = true;
    visorSiguiente.hidden = true;
    visorContador.hidden = true;
    visorFoto.src = ruta;
    visor.showModal();
}

visorAnterior.addEventListener("click", function () {
    mostrarFotoDelVisor(fotoActual - 1);
});

visorSiguiente.addEventListener("click", function () {
    mostrarFotoDelVisor(fotoActual + 1);
});

document.getElementById("visorCerrar").addEventListener("click", function () {
    visor.close();
});

visor.addEventListener("click", function (evento) {
    if (evento.target === visor) visor.close();
});

visor.addEventListener("keydown", function (evento) {
    if (visorAnterior.hidden) return;
    if (evento.key === "ArrowLeft") mostrarFotoDelVisor(fotoActual - 1);
    if (evento.key === "ArrowRight") mostrarFotoDelVisor(fotoActual + 1);
});

const listaResenas = document.getElementById("listaResenas");
const sinResenas = document.getElementById("sinResenas");
const botonVerResenas = document.getElementById("botonVerResenas");
let cantidadResenas = Number(botonVerResenas.textContent.replace(/\D/g, ""));

function crearTarjetaResena(resena) {
    const tarjeta = document.createElement("div");
    tarjeta.className = "tarjeta-resena";
    tarjeta.innerHTML =
        '<div class="resena-autor">' +
        '  <span class="avatar"></span>' +
        '  <div class="resena-datos"><span class="resena-nombre"></span><span class="resena-fecha"></span></div>' +
        '</div>';

    const avatar = tarjeta.querySelector(".avatar");
    if (resena.foto) {
        const foto = document.createElement("img");
        foto.className = "foto-grupo-imagen";
        foto.src = resena.foto;
        foto.width = 44;
        foto.height = 44;
        foto.loading = "lazy";
        foto.alt = "";
        avatar.appendChild(foto);
    } else {
        avatar.textContent = resena.iniciales;
    }
    tarjeta.querySelector(".resena-nombre").textContent = resena.nombre;
    tarjeta.querySelector(".resena-fecha").textContent = resena.hace;

    tarjeta.querySelector(".resena-autor").appendChild(crearEstrellas(resena.puntaje, 16));

    if (resena.comentario) {
        const texto = document.createElement("p");
        texto.className = "resena-texto";
        texto.textContent = resena.comentario;
        tarjeta.appendChild(texto);
    }

    if (resena.fotoResena) {
        const boton = document.createElement("button");
        boton.type = "button";
        boton.className = "resena-foto";
        boton.setAttribute("aria-label", "Ver la foto de la reseña");
        const imagen = document.createElement("img");
        imagen.src = resena.fotoResena;
        imagen.width = 96;
        imagen.height = 96;
        imagen.loading = "lazy";
        imagen.alt = "";
        boton.appendChild(imagen);
        tarjeta.appendChild(boton);
    }
    return tarjeta;
}

listaResenas.addEventListener("click", function (evento) {
    const boton = evento.target.closest(".resena-foto");
    if (boton) abrirFotoDeResena(boton.querySelector("img").getAttribute("src"));
});

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
    }
    botonVerResenas.disabled = false;
});

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

modalResena.addEventListener("click", function (evento) {
    if (evento.target === modalResena) modalResena.close();
});

function puntajeDe(dimension) {
    const elegido = formularioResena.querySelector('input[name="' + dimension + '"]:checked');
    return elegido ? Number(elegido.value) : 0;
}

function actualizarPuntajeGeneral() {
    const elegidos = dimensiones.map(puntajeDe).filter(function (puntaje) { return puntaje > 0; });
    if (elegidos.length === 0) {
        puntajeGeneral.textContent = "–";
        return;
    }
    const promedio = elegidos.reduce(function (suma, puntaje) { return suma + puntaje; }, 0) / elegidos.length;
    puntajeGeneral.replaceChildren(crearEstrellas(promedio, 28));
}

formularioResena.addEventListener("change", actualizarPuntajeGeneral);

function pintarNotas(grupo, hasta) {
    grupo.querySelectorAll(".nota").forEach(function (nota) {
        nota.classList.toggle("pintada", Number(nota.querySelector("input").value) <= hasta);
    });
}

document.querySelectorAll(".notas").forEach(function (grupo) {
    const dimension = grupo.querySelector("input").name;

    function pintarElegida() {
        pintarNotas(grupo, puntajeDe(dimension));
    }

    grupo.addEventListener("mouseover", function (evento) {
        const nota = evento.target.closest(".nota");
        if (nota) pintarNotas(grupo, Number(nota.querySelector("input").value));
    });
    grupo.addEventListener("mouseleave", pintarElegida);
    grupo.addEventListener("change", pintarElegida);
    pintarElegida();
});

const TIPOS_FOTO = ["image/jpeg", "image/png", "image/webp"];
const TAMANO_MAXIMO_FOTO = 5 * 1024 * 1024;

const campoFotos = document.getElementById("campoFotos");
const zonaFoto = document.getElementById("zonaFoto");
const archivoFoto = document.getElementById("archivoFoto");
const miniaturaFoto = document.getElementById("miniaturaFoto");
const miniaturaImagen = document.getElementById("miniaturaImagen");
let fotoElegida = null;

function mostrarErrorEnCampo(campo, mensaje) {
    limpiarErrorEnCampo(campo);
    campo.classList.add("campo-con-error");

    const error = document.createElement("span");
    error.className = "campo-error";
    error.innerHTML = '<span class="tooltip"></span><img src="/img/icono-error.svg" width="24" height="24" alt="Error" />';
    error.querySelector(".tooltip").textContent = mensaje;
    campo.querySelector(".campo-entrada").appendChild(error);
}

function limpiarErrorEnCampo(campo) {
    campo.classList.remove("campo-con-error");
    const error = campo.querySelector(".campo-error");
    if (error) error.remove();
}

function mostrarErrorFoto(mensaje) {
    mostrarErrorEnCampo(campoFotos, mensaje);
}

function limpiarErrorFoto() {
    limpiarErrorEnCampo(campoFotos);
}

const campoComentario = document.getElementById("campoComentario");
comentarioResena.addEventListener("input", function () {
    limpiarErrorEnCampo(campoComentario);
});

function errorDeFoto(archivo) {
    if (!TIPOS_FOTO.includes(archivo.type)) return "La foto tiene que ser JPG, PNG o WEBP.";
    if (archivo.size > TAMANO_MAXIMO_FOTO) return "La foto no puede pesar más de 5 MB.";
    return "";
}

function quitarFoto() {
    if (miniaturaImagen.src.startsWith("blob:")) URL.revokeObjectURL(miniaturaImagen.src);
    fotoElegida = null;
    archivoFoto.value = "";
    miniaturaImagen.removeAttribute("src");
    miniaturaFoto.hidden = true;
}

function elegirFoto(archivo) {
    const mensaje = errorDeFoto(archivo);
    if (mensaje !== "") {
        archivoFoto.value = "";
        mostrarErrorFoto(mensaje);
        return;
    }

    limpiarErrorFoto();
    quitarFoto();
    fotoElegida = archivo;
    miniaturaImagen.src = URL.createObjectURL(archivo);
    miniaturaFoto.hidden = false;
}

archivoFoto.addEventListener("change", function () {
    if (archivoFoto.files.length > 0) elegirFoto(archivoFoto.files[0]);
});

document.getElementById("quitarFoto").addEventListener("click", function () {
    quitarFoto();
    limpiarErrorFoto();
});

zonaFoto.addEventListener("dragover", function (evento) {
    evento.preventDefault();
    zonaFoto.classList.add("arrastrando");
});

zonaFoto.addEventListener("dragleave", function () {
    zonaFoto.classList.remove("arrastrando");
});

zonaFoto.addEventListener("drop", function (evento) {
    evento.preventDefault();
    zonaFoto.classList.remove("arrastrando");
    if (evento.dataTransfer.files.length > 0) elegirFoto(evento.dataTransfer.files[0]);
});

function mostrarAviso(idAviso, mensaje) {
    const aviso = document.getElementById(idAviso);
    document.getElementById(idAviso + "Texto").textContent = mensaje;
    aviso.hidden = false;
    aviso.classList.remove("aviso-visible");
    void aviso.offsetWidth;
    aviso.classList.add("aviso-visible");
}

document.querySelectorAll(".aviso-cerrar").forEach(function (boton) {
    boton.addEventListener("click", function () {
        boton.closest(".aviso").hidden = true;
    });
});

function actualizarPuntaje(puntaje, textoCantidad) {
    document.querySelectorAll(".puntaje-estrellas").forEach(function (lugar) {
        lugar.replaceChildren(crearEstrellas(Number(puntaje), Number(lugar.dataset.tamano)));
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
        const datos = new FormData();
        datos.append("idRestaurante", idRestaurante);
        datos.append("comida", puntajeDe("comida"));
        datos.append("ambiente", puntajeDe("ambiente"));
        datos.append("atencion", puntajeDe("atencion"));
        datos.append("precio", puntajeDe("precio"));
        datos.append("comentario", comentarioResena.value);
        datos.append("esCanje", formularioResena.querySelector('input[name="visita"]:checked').value === "canje");
        if (fotoElegida !== null) datos.append("foto", fotoElegida);

        const respuesta = await fetch("/Restaurantes/DejarResena", {
            method: "POST",
            body: datos
        });
        if (irAlLoginSiHaceFalta(respuesta)) return;

        const resultado = await respuesta.json();
        if (resultado.ok) {
            modalResena.close();
            quitarFoto();
            botonDejarResena.disabled = true;
            botonDejarResena.textContent = "Ya dejaste una reseña";

            if (resultado.publicada) {
                const tarjeta = crearTarjetaResena(resultado.resena);
                tarjeta.classList.add("resena-nueva");
                listaResenas.prepend(tarjeta);

                cantidadResenas = resultado.cantidad;
                actualizarVerResenas();
                actualizarPuntaje(resultado.puntaje, resultado.textoCantidad);
            } else {
                mostrarAviso("avisoDetalle", "Tu reseña se publica 24 horas después de la visita.");
            }
        } else if (resultado.campo === "foto") {
            mostrarErrorFoto(resultado.mensaje);
        } else if (resultado.campo === "comentario") {
            mostrarErrorEnCampo(campoComentario, resultado.mensaje);
            comentarioResena.focus();
        } else {
            mostrarAviso("aviso", resultado.mensaje || "No se pudo guardar la reseña. Probá de nuevo.");
        }
    } catch (error) {
        mostrarAviso("aviso", "No se pudo guardar la reseña. Revisá tu conexión y probá de nuevo.");
    }

    botonPublicarResena.disabled = false;
    botonPublicarResena.classList.remove("cargando");
});

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
    }

    corazonBanner.disabled = false;
    botonFavorito.disabled = false;
}

corazonBanner.addEventListener("click", alternarFavorito);
botonFavorito.addEventListener("click", alternarFavorito);
