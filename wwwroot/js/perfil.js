// 08c · Editar perfil (modal), cambiar foto y 08d · Cambios guardados.
// "Guardar cambios" manda el formulario con fetch, cierra el modal y actualiza la tarjeta de perfil sin recargar.
// Usa mostrarErrorCampo(), limpiarErrorCampo(), limpiarErrores(), mostrarAviso() y ocultarAviso() de cuenta.js.

const modalPerfil = document.getElementById("modalEditarPerfil");
const formularioPerfil = document.getElementById("formularioPerfil");
const fotoModal = document.getElementById("modalPerfilFoto");
const dialogoFoto = document.getElementById("dialogoFoto");
const avisoExito = document.getElementById("avisoExito");

const campoNombre = document.getElementById("campoNombre");
const campoUsuario = document.getElementById("campoUsuario");
const campoUbicacion = document.getElementById("campoUbicacion");
const restriccionNinguna = document.getElementById("restriccionNinguna");
const opcionSoloApto = document.getElementById("verSoloApto");
const opcionVerTodo = document.getElementById("verTodo");

// La foto del modal tal como está guardada (para volver a ella al cancelar)
let vistaFotoGuardada = fotoModal.innerHTML;
// La foto nueva ya recortada: se sube recién con "Guardar cambios"
let fotoPendiente = null;
let urlFotoPendiente = null;
let temporizadorExito = null;

// ---------- Abrir y cerrar el modal ----------

document.getElementById("botonEditarPerfil").addEventListener("click", function () {
    modalPerfil.showModal();
});

document.getElementById("cerrarModalPerfil").addEventListener("click", function () {
    modalPerfil.close();
});

document.getElementById("cancelarPerfil").addEventListener("click", function () {
    modalPerfil.close();
});

// Se cierre como se cierre (Cancelar, la X, Escape o después de guardar) el formulario queda
// con lo último que está guardado: lo que se tocó y no se guardó se descarta.
modalPerfil.addEventListener("close", function () {
    formularioPerfil.reset();
    limpiarErrores(formularioPerfil);
    descartarFotoPendiente();
    actualizarSoloApto();
});

document.querySelector("#aviso .aviso-cerrar").addEventListener("click", ocultarAviso);

// Al escribir o elegir en un campo con error, se le saca el error
formularioPerfil.querySelectorAll(".campo input, .campo select").forEach(function (entrada) {
    entrada.addEventListener("input", function () {
        limpiarErrorCampo(entrada.closest(".campo"));
    });
});

// ---------- Restricciones: "Ninguna" y "Solo lo apto" ----------

// "Ninguna" desmarca las demás restricciones, y marcar otra desmarca "Ninguna".
// "Ninguna" queda marcada sola cuando no hay ninguna restricción marcada.
formularioPerfil.addEventListener("change", function (evento) {
    const marcada = evento.target;
    if (marcada !== restriccionNinguna && marcada.name !== "restricciones") return;

    if (marcada === restriccionNinguna) {
        formularioPerfil.querySelectorAll('input[name="restricciones"]').forEach(function (casilla) {
            casilla.checked = false;
        });
    }
    restriccionNinguna.checked = !hayRestriccionesMarcadas();
    actualizarSoloApto();
});

function hayRestriccionesMarcadas() {
    return formularioPerfil.querySelector('input[name="restricciones"]:checked') !== null;
}

// Sin restricciones no hay nada que filtrar: "Solo lo apto para mí" no se puede elegir
function actualizarSoloApto() {
    const hay = hayRestriccionesMarcadas();
    opcionSoloApto.disabled = !hay;
    if (!hay) opcionVerTodo.checked = true;
}

// ---------- Cambiar foto ----------

const archivoFoto = document.getElementById("archivoFoto");
const lienzo = document.getElementById("lienzoFoto");
const contexto = lienzo.getContext("2d");
const zoomFoto = document.getElementById("zoomFoto");

const LADO = 299;       // el lienzo
const DIAMETRO = 264;   // el círculo que queda de foto de perfil
const SALIDA = 512;     // la foto que se sube: 512 x 512

let imagen = null;
let escalaBase = 1;
let centroX = LADO / 2;
let centroY = LADO / 2;

document.getElementById("botonCambiarFoto").addEventListener("click", function () {
    archivoFoto.click();
});

archivoFoto.addEventListener("change", function () {
    const archivo = archivoFoto.files[0];
    archivoFoto.value = "";
    if (!archivo) return;

    if (!/^image\/(jpeg|png|webp)$/.test(archivo.type)) {
        mostrarAviso("La foto tiene que ser JPG, PNG o WEBP.");
        return;
    }
    if (archivo.size > 5 * 1024 * 1024) {
        mostrarAviso("La foto puede pesar hasta 5 MB.");
        return;
    }
    ocultarAviso();
    abrirEditorFoto(archivo);
});

function abrirEditorFoto(archivo) {
    const url = URL.createObjectURL(archivo);
    const foto = new Image();
    foto.onload = function () {
        URL.revokeObjectURL(url);
        imagen = foto;
        // La foto cubre justo el círculo; con el zoom se acerca
        escalaBase = DIAMETRO / Math.min(foto.width, foto.height);
        zoomFoto.value = 1;
        centroX = LADO / 2;
        centroY = LADO / 2;
        dibujarEditor();
        dialogoFoto.showModal();
    };
    foto.onerror = function () {
        URL.revokeObjectURL(url);
        mostrarAviso("No pudimos abrir esa foto. Probá con otra.");
    };
    foto.src = url;
}

// La foto no puede dejar espacios vacíos dentro del círculo
function limitarEncuadre() {
    const escala = escalaBase * Number(zoomFoto.value);
    const sobraX = Math.max(0, (imagen.width * escala - DIAMETRO) / 2);
    const sobraY = Math.max(0, (imagen.height * escala - DIAMETRO) / 2);
    centroX = Math.min(LADO / 2 + sobraX, Math.max(LADO / 2 - sobraX, centroX));
    centroY = Math.min(LADO / 2 + sobraY, Math.max(LADO / 2 - sobraY, centroY));
}

function dibujarEditor() {
    limitarEncuadre();
    const escala = escalaBase * Number(zoomFoto.value);
    const ancho = imagen.width * escala;
    const alto = imagen.height * escala;

    contexto.clearRect(0, 0, LADO, LADO);
    contexto.drawImage(imagen, centroX - ancho / 2, centroY - alto / 2, ancho, alto);

    // Lo que queda fuera del círculo se oscurece
    contexto.fillStyle = "rgba(0, 0, 0, 0.55)";
    contexto.beginPath();
    contexto.rect(0, 0, LADO, LADO);
    contexto.arc(LADO / 2, LADO / 2, DIAMETRO / 2, 0, Math.PI * 2, true);
    contexto.fill("evenodd");

    contexto.strokeStyle = "#ffffff";
    contexto.lineWidth = 3;
    contexto.beginPath();
    contexto.arc(LADO / 2, LADO / 2, DIAMETRO / 2, 0, Math.PI * 2);
    contexto.stroke();
}

// Arrastrar para encuadrar
let arrastrando = false;
let ultimoX = 0;
let ultimoY = 0;

lienzo.addEventListener("pointerdown", function (evento) {
    arrastrando = true;
    ultimoX = evento.clientX;
    ultimoY = evento.clientY;
    lienzo.setPointerCapture(evento.pointerId);
});

lienzo.addEventListener("pointermove", function (evento) {
    if (!arrastrando) return;
    // El lienzo puede verse más chico que 299 px en pantallas angostas
    const proporcion = lienzo.width / lienzo.getBoundingClientRect().width;
    centroX += (evento.clientX - ultimoX) * proporcion;
    centroY += (evento.clientY - ultimoY) * proporcion;
    ultimoX = evento.clientX;
    ultimoY = evento.clientY;
    dibujarEditor();
});

lienzo.addEventListener("pointerup", function () { arrastrando = false; });
lienzo.addEventListener("pointercancel", function () { arrastrando = false; });
zoomFoto.addEventListener("input", dibujarEditor);

document.getElementById("cerrarDialogoFoto").addEventListener("click", function () {
    dialogoFoto.close();
});

document.getElementById("cancelarDialogoFoto").addEventListener("click", function () {
    dialogoFoto.close();
});

// "Guardar imagen": recorta el círculo en una foto cuadrada de 512 px. Todavía no se sube:
// se muestra en el modal y se sube con "Guardar cambios".
document.getElementById("guardarDialogoFoto").addEventListener("click", function () {
    const salida = document.createElement("canvas");
    salida.width = SALIDA;
    salida.height = SALIDA;
    const pincel = salida.getContext("2d");
    pincel.fillStyle = "#ffffff";
    pincel.fillRect(0, 0, SALIDA, SALIDA);

    const proporcion = SALIDA / DIAMETRO;
    const escala = escalaBase * Number(zoomFoto.value);
    const ancho = imagen.width * escala;
    const alto = imagen.height * escala;
    const margen = (LADO - DIAMETRO) / 2;
    pincel.drawImage(imagen,
        (centroX - ancho / 2 - margen) * proporcion,
        (centroY - alto / 2 - margen) * proporcion,
        ancho * proporcion,
        alto * proporcion);

    salida.toBlob(function (blob) {
        if (!blob) {
            mostrarAviso("No pudimos preparar la foto. Probá con otra.");
            dialogoFoto.close();
            return;
        }
        liberarFotoPendiente();
        fotoPendiente = blob;
        urlFotoPendiente = URL.createObjectURL(blob);
        ponerFoto(fotoModal, urlFotoPendiente);
        dialogoFoto.close();
    }, "image/jpeg", 0.9);
});

function ponerFoto(contenedor, url) {
    contenedor.innerHTML = "";
    const foto = document.createElement("img");
    foto.className = "perfil-foto-imagen";
    foto.src = url;
    foto.alt = "";
    contenedor.appendChild(foto);
}

function liberarFotoPendiente() {
    if (urlFotoPendiente) URL.revokeObjectURL(urlFotoPendiente);
    fotoPendiente = null;
    urlFotoPendiente = null;
}

// Al cancelar, la foto del modal vuelve a la que estaba guardada
function descartarFotoPendiente() {
    liberarFotoPendiente();
    fotoModal.innerHTML = vistaFotoGuardada;
}

// ---------- Guardar cambios ----------

formularioPerfil.addEventListener("submit", async function (evento) {
    evento.preventDefault();
    limpiarErrores(formularioPerfil);

    const nombre = campoNombre.value.trim();
    const usuario = campoUsuario.value.trim();

    // El servidor valida lo mismo; esto es para avisar sin esperar
    const errores = [];
    if (nombre === "") errores.push(["nombre", "Completá tu nombre y apellido."]);
    else if (nombre.length > 100) errores.push(["nombre", "Puede tener hasta 100 caracteres."]);
    if (usuario === "") errores.push(["usuario", "Elegí un nombre de usuario."]);
    else if (/\s/.test(usuario)) errores.push(["usuario", "No puede tener espacios."]);
    else if (usuario.length > 50) errores.push(["usuario", "Puede tener hasta 50 caracteres."]);
    if (campoUbicacion.value === "") errores.push(["ubicacion", "Elegí tu ubicación."]);

    if (errores.length > 0) {
        mostrarErroresDeCampos(errores);
        return;
    }

    const datos = new FormData();
    datos.append("nombreCompleto", nombre);
    datos.append("nombreUsuario", usuario);
    datos.append("idBarrio", campoUbicacion.value);
    formularioPerfil.querySelectorAll('input[name="restricciones"]:checked').forEach(function (casilla) {
        datos.append("restricciones", casilla.value);
    });
    formularioPerfil.querySelectorAll('input[name="zonas"]:checked').forEach(function (casilla) {
        datos.append("zonas", casilla.value);
    });
    const frecuencia = formularioPerfil.querySelector('input[name="frecuencia"]:checked');
    if (frecuencia) datos.append("frecuencia", frecuencia.value);
    formularioPerfil.querySelectorAll('input[name="companias"]:checked').forEach(function (casilla) {
        datos.append("companias", casilla.value);
    });
    datos.append("soloApto", opcionSoloApto.checked && !opcionSoloApto.disabled ? "true" : "false");
    if (fotoPendiente) datos.append("foto", fotoPendiente, "foto.jpg");

    const boton = formularioPerfil.querySelector("button[type=submit]");
    boton.disabled = true;
    boton.classList.add("cargando");

    try {
        const respuesta = await fetch("/Perfil/GuardarPerfil", { method: "POST", body: datos });

        // Si se venció la sesión, al login
        if (respuesta.status === 401) {
            window.location.href = "/Cuenta/IniciarSesion";
            return;
        }

        const resultado = await respuesta.json();
        if (resultado.ok) {
            aplicarPerfilGuardado(resultado.perfil);
            // Lo que se acaba de guardar pasa a ser el punto de partida del formulario
            guardarComoPuntoDePartida();
            modalPerfil.close();
            mostrarAvisoExito();
        } else if (resultado.campo === "aviso") {
            mostrarAviso(resultado.mensaje);
        } else {
            mostrarErroresDeCampos([[resultado.campo, resultado.mensaje]]);
        }
    } catch (error) {
        mostrarAviso("No pudimos conectarnos. Probá de nuevo en un rato.");
    }

    boton.disabled = false;
    boton.classList.remove("cargando");
});

function mostrarErroresDeCampos(errores) {
    errores.forEach(function (error) {
        mostrarErrorCampo(formularioPerfil.querySelector('[data-campo="' + error[0] + '"]'), error[1]);
    });
    // El foco queda en el primer campo con error
    formularioPerfil.querySelector(".campo-con-error input, .campo-con-error select").focus();
}

// form.reset() vuelve a los valores por defecto del HTML: se los actualiza con lo recién guardado
function guardarComoPuntoDePartida() {
    formularioPerfil.querySelectorAll("input[type=checkbox], input[type=radio]").forEach(function (entrada) {
        entrada.defaultChecked = entrada.checked;
    });
    formularioPerfil.querySelectorAll("input[type=text]").forEach(function (entrada) {
        entrada.defaultValue = entrada.value;
    });
    formularioPerfil.querySelectorAll("select option").forEach(function (opcion) {
        opcion.defaultSelected = opcion.selected;
    });
}

// Actualiza sin recargar la tarjeta de perfil, el modal y el menú del avatar del header
function aplicarPerfilGuardado(perfil) {
    document.getElementById("perfilNombre").textContent = perfil.nombre;
    document.getElementById("perfilUsuario").textContent = "@" + perfil.usuario + (perfil.barrio ? " · " + perfil.barrio : "");

    if (perfil.foto) {
        ponerFoto(document.getElementById("perfilFoto"), perfil.foto);
        ponerFoto(fotoModal, perfil.foto);
    } else if (!fotoModal.querySelector("img")) {
        // Sin foto, el modal muestra las iniciales
        fotoModal.textContent = perfil.iniciales;
    }
    liberarFotoPendiente();
    vistaFotoGuardada = fotoModal.innerHTML;

    const chips = document.getElementById("perfilChips");
    chips.innerHTML = "";
    perfil.restricciones.forEach(function (nombre) {
        const chip = document.createElement("span");
        chip.className = "chip chip-naranja chip-perfil";
        chip.textContent = nombre;
        chips.appendChild(chip);
    });
    chips.hidden = perfil.restricciones.length === 0;
    document.getElementById("perfilChipsVacio").hidden = perfil.restricciones.length > 0;

    // Header: el avatar con las iniciales, y el nombre y @usuario del menú
    const avatar = document.getElementById("botonAvatar");
    if (avatar) avatar.textContent = perfil.iniciales;
    const avatarMenu = document.querySelector(".avatar-menu");
    if (avatarMenu) avatarMenu.textContent = perfil.iniciales;
    const nombreMenu = document.querySelector(".menu-usuario-nombre");
    if (nombreMenu) nombreMenu.textContent = perfil.nombre;
    const usuarioMenu = document.querySelector(".menu-usuario-arroba");
    if (usuarioMenu) usuarioMenu.textContent = "@" + perfil.usuario;
}

// ---------- 08d · Snackbar de éxito ----------

function mostrarAvisoExito() {
    clearTimeout(temporizadorExito);
    avisoExito.hidden = false;
    // Se vuelve a poner la clase para que la animación arranque de nuevo
    avisoExito.classList.remove("aviso-visible");
    void avisoExito.offsetWidth;
    avisoExito.classList.add("aviso-visible");
    temporizadorExito = setTimeout(ocultarAvisoExito, 4000);
}

function ocultarAvisoExito() {
    clearTimeout(temporizadorExito);
    avisoExito.hidden = true;
}

document.getElementById("cerrarAvisoExito").addEventListener("click", ocultarAvisoExito);
