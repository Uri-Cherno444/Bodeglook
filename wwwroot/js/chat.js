const conversacion = document.getElementById("conversacion");
const idGrupo = conversacion ? Number(conversacion.dataset.idGrupo) : 0;
const listaMensajes = document.getElementById("listaMensajes");
const formularioMensaje = document.getElementById("formularioMensaje");
const campoMensaje = document.getElementById("campoMensaje");

let ultimoId = 0;
let ultimoDia = "";

let pedidos = Promise.resolve();
function enFila(tarea) {
    pedidos = pedidos.then(tarea).catch(function () { });
    return pedidos;
}

function revisarRespuesta(respuesta) {
    if (respuesta.status === 401) window.location.href = "/Cuenta/IniciarSesion";
    if (respuesta.status === 403) window.location.href = "/Grupos";
    if (!respuesta.ok) throw new Error("Error " + respuesta.status);
    return respuesta;
}

async function traerMensajesNuevos() {
    const respuesta = revisarRespuesta(await fetch("/Grupos/Mensajes?id=" + idGrupo + "&desde=" + ultimoId));
    const datos = await respuesta.json();
    const mensajes = datos.mensajes;

    const estabaAbajo = ultimoId === 0 ||
        listaMensajes.scrollHeight - listaMensajes.scrollTop - listaMensajes.clientHeight < 80;

    actualizarCabecera(datos.grupo);
    mensajes.forEach(dibujarMensaje);
    actualizarEncuestas(datos.encuestas);

    if (mensajes.length === 0) return;
    const ultimo = mensajes[mensajes.length - 1];
    if (estabaAbajo || ultimo.esMio) listaMensajes.scrollTop = listaMensajes.scrollHeight;
}

// Refleja los cambios del grupo (nombre, foto e integrantes) sin recargar la página
function actualizarCabecera(grupo) {
    document.getElementById("nombreGrupoAbierto").textContent = grupo.nombre;
    document.getElementById("integrantesGrupoAbierto").textContent = grupo.integrantes;
    document.getElementById("subtituloPlan").textContent = "Para " + grupo.nombre + " · " + grupo.integrantes;

    const contenedor = document.getElementById("fotoGrupo");
    const imagen = contenedor.querySelector(".foto-grupo-imagen");
    if (!grupo.foto || (imagen && imagen.getAttribute("src") === grupo.foto)) return;
    const nueva = document.createElement("img");
    nueva.className = "foto-grupo-imagen";
    nueva.src = grupo.foto;
    nueva.width = 64;
    nueva.height = 64;
    nueva.alt = "";
    contenedor.replaceChildren(nueva);
}

function dibujarMensaje(mensaje) {
    if (mensaje.id <= ultimoId) return;
    ultimoId = mensaje.id;

    if (mensaje.dia !== ultimoDia) {
        ultimoDia = mensaje.dia;
        const separador = document.createElement("p");
        separador.className = "mensaje-dia";
        separador.textContent = mensaje.textoDia;
        listaMensajes.appendChild(separador);
    }

    const fila = document.createElement("div");

    if (mensaje.idPlan) {
        fila.className = "mensaje mensaje-encuesta" + (mensaje.esMio ? " mensaje-mio" : "");
        fila.innerHTML = (mensaje.esMio ? "" : '<span class="avatar avatar-mensaje"></span>') + '<div class="encuesta"></div>';
        if (!mensaje.esMio) ponerAvatar(fila.querySelector(".avatar"), mensaje);
        fila.querySelector(".encuesta").dataset.idPlan = mensaje.idPlan;
        listaMensajes.appendChild(fila);
        return;
    }

    if (mensaje.esMio) {
        fila.className = "mensaje mensaje-mio";
        fila.innerHTML = '<div class="mensaje-texto"><p></p><span class="mensaje-hora"></span></div>';
    } else {
        fila.className = "mensaje";
        fila.innerHTML =
            '<span class="avatar avatar-mensaje"></span>' +
            '<div class="mensaje-burbuja">' +
            '  <span class="mensaje-autor"></span>' +
            '  <div class="mensaje-texto"><p></p><span class="mensaje-hora"></span></div>' +
            '</div>';
        ponerAvatar(fila.querySelector(".avatar"), mensaje);
        fila.querySelector(".mensaje-autor").textContent = mensaje.nombre;
    }

    fila.querySelector(".mensaje-texto p").textContent = mensaje.contenido;
    fila.querySelector(".mensaje-hora").textContent = mensaje.hora;
    listaMensajes.appendChild(fila);
}

function ponerAvatar(avatar, mensaje) {
    if (!mensaje.foto) {
        avatar.textContent = mensaje.iniciales;
        return;
    }
    const foto = document.createElement("img");
    foto.className = "foto-grupo-imagen";
    foto.src = mensaje.foto;
    foto.width = 36;
    foto.height = 36;
    foto.alt = "";
    avatar.appendChild(foto);
}

function actualizarEncuestas(encuestas) {
    encuestas.forEach(function (encuesta) {
        const tarjeta = listaMensajes.querySelector('.encuesta[data-id-plan="' + encuesta.id + '"]');
        if (!tarjeta) return;
        const datos = JSON.stringify(encuesta);
        if (tarjeta.dataset.datos === datos) return;
        tarjeta.dataset.datos = datos;
        dibujarEncuesta(tarjeta, encuesta);
    });
}

function dibujarEncuesta(tarjeta, encuesta) {
    const confirmado = encuesta.estado === "confirmado";
    const cancelado = encuesta.estado === "cancelado";
    const abierto = encuesta.estado === "votacion";

    tarjeta.classList.toggle("encuesta-confirmada", !abierto);
    tarjeta.innerHTML =
        '<span class="encuesta-etiqueta"></span>' +
        '<div class="encuesta-encabezado"><h3 class="encuesta-titulo"></h3><span class="chip chip-estado"></span></div>' +
        '<p class="encuesta-subtitulo"></p>' +
        '<span class="encuesta-seccion">LUGAR</span>' +
        '<div class="encuesta-opciones encuesta-lugares" role="radiogroup"></div>' +
        '<span class="encuesta-seccion">HORARIO</span>' +
        '<div class="encuesta-opciones encuesta-horarios" role="radiogroup"></div>' +
        '<div class="encuesta-pie"><span></span><a>Ver detalle</a></div>';

    const etiquetas = { votacion: "ENCUESTA DEL GRUPO", confirmado: "PLAN CONFIRMADO", cancelado: "PLAN CANCELADO" };
    const chips = { votacion: ["En votación", "chip-naranja"], confirmado: ["Confirmado", "chip-confirmado"], cancelado: ["Cancelado", "chip-cancelado"] };
    tarjeta.querySelector(".encuesta-etiqueta").textContent = etiquetas[encuesta.estado];
    tarjeta.querySelector(".encuesta-titulo").textContent = encuesta.titulo;
    const chip = tarjeta.querySelector(".chip-estado");
    chip.textContent = chips[encuesta.estado][0];
    chip.classList.add(chips[encuesta.estado][1]);

    let subtitulo = encuesta.textoDia + " · Votá un lugar y un horario";
    if (confirmado) subtitulo = "Ganó " + encuesta.lugarElegido + " · " + encuesta.textoDiaYHora;
    if (cancelado) subtitulo = encuesta.textoDia + " · El plan se canceló";
    tarjeta.querySelector(".encuesta-subtitulo").textContent = subtitulo;

    const lugarResaltado = abierto ? encuesta.miVoto : confirmado ? encuesta.idElegido : null;
    const horarioResaltado = abierto ? encuesta.miVotoHorario : confirmado ? encuesta.idHorarioElegido : null;

    const lugares = tarjeta.querySelector(".encuesta-lugares");
    lugares.setAttribute("aria-label", "Lugar");
    encuesta.opciones.forEach(function (opcion) {
        lugares.appendChild(crearOpcionEncuesta(encuesta, "lugar", opcion.idRestaurante, opcion.nombre, opcion.detalle,
            opcion.votos, opcion.porcentaje, opcion.idRestaurante === lugarResaltado, opcion.foto));
    });

    const horarios = tarjeta.querySelector(".encuesta-horarios");
    horarios.setAttribute("aria-label", "Horario");
    encuesta.horarios.forEach(function (horario) {
        horarios.appendChild(crearOpcionEncuesta(encuesta, "horario", horario.idHorario, horario.horario, null,
            horario.votos, horario.porcentaje, horario.idHorario === horarioResaltado));
    });

    const votaron = encuesta.votaron + " de " + encuesta.integrantes + " votaron";
    let pie = votaron + " · " + encuesta.creador + " confirma lugar y horario";
    if (confirmado) pie = "Confirmado por " + encuesta.creador + " · " + votaron;
    if (cancelado) pie = "Cancelado por " + encuesta.creador;
    tarjeta.querySelector(".encuesta-pie span").textContent = pie;
    tarjeta.querySelector(".encuesta-pie a").href = "/Planes/Detalle/" + encuesta.id;
}

function crearOpcionEncuesta(encuesta, tipo, id, nombre, detalle, votos, porcentaje, resaltada, foto) {
    const abierto = encuesta.estado === "votacion";
    const fila = document.createElement("label");
    fila.className = "opcion-plan";
    if (resaltada) fila.classList.add("opcion-plan-elegida");
    if (!abierto && !resaltada) fila.classList.add("opcion-plan-apagada");
    fila.innerHTML =
        '<span class="opcion-plan-fila">' +
        '  <span class="opcion-plan-elegir">' + (foto ? '<img class="lugar-miniatura" width="40" height="40" loading="lazy" alt="" />' : '') + '<span class="opcion-plan-nombre"></span>' +
        '    <input type="radio" class="radio" /><span class="radio-circulo" aria-hidden="true"></span></span>' +
        '  <span class="opcion-plan-votos"></span>' +
        '</span>' +
        (detalle ? '<span class="opcion-plan-detalle"></span>' : '') +
        '<span class="barra-votos"><span class="barra-votos-avance"></span></span>';

    fila.querySelector(".opcion-plan-nombre").textContent = nombre;
    if (foto) fila.querySelector(".lugar-miniatura").src = foto;
    const radio = fila.querySelector(".radio");
    radio.name = "plan-" + encuesta.id + "-" + tipo;
    radio.value = id;
    radio.dataset.idPlan = encuesta.id;
    radio.dataset.tipo = tipo;
    radio.checked = resaltada;
    radio.disabled = !abierto;
    fila.querySelector(".opcion-plan-votos").textContent = votos + (votos === 1 ? " voto" : " votos");
    if (detalle) fila.querySelector(".opcion-plan-detalle").textContent = detalle;
    fila.querySelector(".barra-votos-avance").style.width = porcentaje + "%";
    return fila;
}

if (listaMensajes) {
    listaMensajes.addEventListener("change", function (evento) {
        const radio = evento.target;
        if (!radio.classList.contains("radio")) return;
        const esHorario = radio.dataset.tipo === "horario";
        const datos = esHorario
            ? { idPlan: Number(radio.dataset.idPlan), idHorarioVotado: Number(radio.value) }
            : { idPlan: Number(radio.dataset.idPlan), idRestaurante: Number(radio.value) };
        const tarjeta = radio.closest(".encuesta");
        enFila(async function () {
            const respuesta = revisarRespuesta(await fetch(esHorario ? "/Planes/VotarHorario" : "/Planes/Votar", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(datos)
            }));
            const resultado = await respuesta.json();
            if (!resultado.ok) tarjeta.dataset.datos = "";
            await traerMensajesNuevos();
            if (!resultado.ok) mostrarErrorEncuesta(tarjeta, radio.dataset.tipo, resultado.mensaje);
        });
    });
}

function mostrarErrorEncuesta(tarjeta, tipo, mensaje) {
    const opciones = tarjeta.querySelector(tipo === "horario" ? ".encuesta-horarios" : ".encuesta-lugares");
    if (!opciones) return;

    const error = document.createElement("p");
    error.className = "encuesta-error";
    error.setAttribute("role", "alert");
    error.innerHTML = '<img src="/img/icono-error.svg" width="24" height="24" alt="" /><span></span>';
    error.querySelector("span").textContent = mensaje;
    opciones.after(error);
}

async function actualizarListaChats() {
    const respuesta = revisarRespuesta(await fetch("/Grupos/ListaChats?abierto=" + idGrupo));
    const html = await respuesta.text();
    document.querySelector(".panel-chats").outerHTML = html;
}

async function actualizar() {
    if (idGrupo) await traerMensajesNuevos();
    await actualizarListaChats();
}

if (formularioMensaje) {
    formularioMensaje.addEventListener("submit", function (evento) {
        evento.preventDefault();
        const contenido = campoMensaje.value.trim();
        if (contenido === "") return;

        campoMensaje.value = "";
        ajustarAlturaMensaje();
        enFila(async function () {
            const respuesta = revisarRespuesta(await fetch("/Grupos/EnviarMensaje", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ idGrupo: idGrupo, contenido: contenido })
            }));
            const resultado = await respuesta.json();
            if (!resultado.ok) {
                campoMensaje.value = contenido;
                ajustarAlturaMensaje();
                return;
            }
            await actualizar();
        });
        campoMensaje.focus();
    });

    campoMensaje.addEventListener("input", ajustarAlturaMensaje);
    window.addEventListener("resize", ajustarAlturaMensaje);

    // Enter envía y Shift + Enter hace un salto de línea
    campoMensaje.addEventListener("keydown", function (evento) {
        if (evento.key !== "Enter" || evento.shiftKey || evento.isComposing) return;
        evento.preventDefault();
        formularioMensaje.requestSubmit();
    });
}

function ajustarAlturaMensaje() {
    campoMensaje.style.height = "";
    campoMensaje.style.overflowY = "";
    if (campoMensaje.value === "") return;

    const estilos = getComputedStyle(campoMensaje);
    const bordes = parseFloat(estilos.borderTopWidth) + parseFloat(estilos.borderBottomWidth);
    const alto = campoMensaje.scrollHeight + bordes;
    const maximo = parseFloat(estilos.maxHeight);
    campoMensaje.style.height = Math.min(alto, maximo) + "px";
    if (alto > maximo) campoMensaje.style.overflowY = "auto";
}

if (idGrupo) enFila(traerMensajesNuevos);

let consultando = false;
setInterval(function () {
    if (consultando) return;
    consultando = true;
    enFila(actualizar).then(function () {
        consultando = false;
    });
}, 3000);

const dialogoGrupo = document.getElementById("dialogoCrearGrupo");
const formularioGrupo = document.getElementById("formularioGrupo");
const listaAmigos = document.getElementById("listaAmigos");
const buscarAmigos = document.getElementById("buscarAmigos");
const contadorSeleccionados = document.getElementById("contadorSeleccionados");
const campoFoto = document.getElementById("campoFoto");
const fotoNuevaCirculo = document.getElementById("fotoNuevaCirculo");
const botonGuardarGrupo = document.getElementById("botonGuardarGrupo");

// El mismo formulario sirve para crear un grupo y para editar el que está abierto
let editandoGrupo = false;
let aperturaGrupo = 0;

async function pedirJson(url) {
    const respuesta = revisarRespuesta(await fetch(url));
    return respuesta.json();
}

async function abrirFormularioGrupo(editar) {
    editandoGrupo = editar;
    const apertura = ++aperturaGrupo;

    formularioGrupo.reset();
    fotoNuevaCirculo.textContent = "+";
    ocultarAviso();
    listaAmigos.innerHTML = "";
    actualizarContador();
    document.getElementById("dialogoCrearGrupoTitulo").textContent = editar ? "Editar grupo" : "Crear grupo";
    document.getElementById("notaCrearGrupo").hidden = editar;
    botonGuardarGrupo.textContent = editar ? "Guardar cambios" : "Crear grupo";
    botonGuardarGrupo.classList.remove("cargando");
    // Al editar no se puede guardar hasta tener los datos actuales, para no pisarlos con un formulario vacío
    botonGuardarGrupo.disabled = editar;
    dialogoGrupo.showModal();

    try {
        const [amigos, grupo] = await Promise.all([
            pedirJson("/Grupos/Amigos"),
            editar ? pedirJson("/Grupos/DatosGrupo?id=" + idGrupo) : null
        ]);
        // Si mientras tanto se volvió a abrir, esta respuesta ya no va: duplicaría la lista
        if (apertura !== aperturaGrupo) return;

        const filas = new Map(amigos.map(function (amigo) {
            return [amigo.id, crearFilaAmigo(amigo)];
        }));
        listaAmigos.replaceChildren(...filas.values());
        if (filas.size === 0) {
            listaAmigos.innerHTML = '<p class="lista-amigos-vacia">Todavía no hay amigos para sumar.</p>';
        }

        if (grupo) {
            document.getElementById("nombreGrupo").value = grupo.nombre;
            if (grupo.foto && campoFoto.files.length === 0) mostrarFotoGrupo(grupo.foto);
            grupo.integrantes.forEach(function (id) {
                if (filas.has(id)) filas.get(id).querySelector(".casilla").checked = true;
            });
            botonGuardarGrupo.disabled = false;
        }
        buscarAmigos.dispatchEvent(new Event("input"));
        actualizarContador();
    } catch (error) {
        if (apertura !== aperturaGrupo) return;
        mostrarAviso(editar
            ? "No pudimos traer los datos del grupo. Probá de nuevo en un rato."
            : "No pudimos traer tus amigos. Probá de nuevo en un rato.");
    }
}

function mostrarFotoGrupo(ruta) {
    const imagen = document.createElement("img");
    imagen.src = ruta;
    imagen.alt = "";
    fotoNuevaCirculo.replaceChildren(imagen);
}

function crearFilaAmigo(amigo) {
    const fila = document.createElement("label");
    fila.className = "amigo";
    fila.innerHTML =
        '<span class="avatar"></span>' +
        '<span class="amigo-nombre"></span>' +
        '<input type="checkbox" class="casilla" name="integrantes" />' +
        '<span class="casilla-caja" aria-hidden="true"></span>';
    fila.querySelector(".avatar").textContent = amigo.iniciales;
    fila.querySelector(".amigo-nombre").textContent = amigo.nombre;
    fila.querySelector(".casilla").value = amigo.id;
    return fila;
}

function actualizarContador() {
    const cantidad = listaAmigos.querySelectorAll(".casilla:checked").length;
    contadorSeleccionados.textContent = cantidad + (cantidad === 1 ? " seleccionado" : " seleccionados");
}

function sinAcentos(texto) {
    return texto.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();
}

buscarAmigos.addEventListener("input", function () {
    const buscado = sinAcentos(buscarAmigos.value.trim());
    listaAmigos.querySelectorAll(".amigo").forEach(function (fila) {
        fila.hidden = !sinAcentos(fila.querySelector(".amigo-nombre").textContent).includes(buscado);
    });
});

listaAmigos.addEventListener("change", actualizarContador);

document.getElementById("botonSubirFoto").addEventListener("click", function () {
    campoFoto.click();
});

campoFoto.addEventListener("change", function () {
    fotoNuevaCirculo.textContent = "+";
    if (campoFoto.files.length > 0) {
        mostrarFotoGrupo(URL.createObjectURL(campoFoto.files[0]));
    } else if (editandoGrupo) {
        const actual = document.querySelector("#fotoGrupo .foto-grupo-imagen");
        if (actual) mostrarFotoGrupo(actual.getAttribute("src"));
    }
});

document.getElementById("cerrarCrearGrupo").addEventListener("click", function () {
    dialogoGrupo.close();
});

document.getElementById("cancelarCrearGrupo").addEventListener("click", function () {
    dialogoGrupo.close();
});

formularioGrupo.querySelector(".aviso-cerrar").addEventListener("click", ocultarAviso);

formularioGrupo.addEventListener("submit", async function (evento) {
    evento.preventDefault();
    ocultarAviso();
    botonGuardarGrupo.disabled = true;
    botonGuardarGrupo.classList.add("cargando");
    const editar = editandoGrupo;
    const apertura = aperturaGrupo;
    let error = "";

    try {
        const respuesta = await fetch(editar ? "/Grupos/EditarGrupo?id=" + idGrupo : "/Grupos/CrearGrupo", {
            method: "POST",
            body: new FormData(formularioGrupo)
        });
        // Al editar, un 403 es falta de permiso: se avisa acá en vez de sacar a la persona del chat
        if (editar && respuesta.status === 403) {
            error = "Solo quien creó el grupo puede editarlo.";
        } else {
            const resultado = await revisarRespuesta(respuesta).json();
            if (resultado.ok && !editar) {
                window.location.href = resultado.destino;
                return;
            }
            if (resultado.ok) {
                if (apertura === aperturaGrupo) dialogoGrupo.close();
                mostrarExito("Guardamos los cambios del grupo.");
                enFila(actualizar);
            } else {
                error = resultado.mensaje;
            }
        }
    } catch (falla) {
        error = editar
            ? "No pudimos guardar los cambios. Probá de nuevo en un rato."
            : "No pudimos crear el grupo. Probá de nuevo en un rato.";
    }

    if (apertura !== aperturaGrupo) return;
    if (error) mostrarAviso(error);
    botonGuardarGrupo.disabled = false;
    botonGuardarGrupo.classList.remove("cargando");
});

const avisoExito = document.getElementById("avisoExito");
let esperaExito = null;

function mostrarExito(mensaje) {
    document.getElementById("avisoExitoTexto").textContent = mensaje;
    avisoExito.hidden = false;
    clearTimeout(esperaExito);
    esperaExito = setTimeout(function () {
        avisoExito.hidden = true;
    }, 4000);
}

avisoExito.querySelector(".aviso-cerrar").addEventListener("click", function () {
    avisoExito.hidden = true;
});

const botonEditarGrupo = document.getElementById("botonEditarGrupo");
if (botonEditarGrupo) {
    botonEditarGrupo.addEventListener("click", function () {
        abrirFormularioGrupo(true);
    });
}

function mostrarAviso(mensaje) {
    const aviso = document.getElementById("aviso");
    document.getElementById("avisoTexto").textContent = mensaje;
    aviso.hidden = false;
    aviso.classList.remove("aviso-visible");
    void aviso.offsetWidth;
    aviso.classList.add("aviso-visible");
}

function ocultarAviso() {
    document.getElementById("aviso").hidden = true;
}

document.addEventListener("click", function (evento) {
    if (evento.target.closest(".panel-chats-crear")) {
        evento.preventDefault();
        abrirFormularioGrupo(false);
    }
});

const dialogoPlan = document.getElementById("dialogoCrearPlan");

if (dialogoPlan) {
    const formularioPlan = document.getElementById("formularioPlan");
    const nombrePlan = document.getElementById("nombrePlan");
    const diaPlan = document.getElementById("diaPlan");
    const chipsHorario = formularioPlan.querySelectorAll(".chip-horario");
    const campoHorarios = formularioPlan.querySelector('[data-campo="horarios"]');
    const camposLugar = formularioPlan.querySelectorAll(".lugar-plan");
    const botonPublicar = formularioPlan.querySelector("button[type=submit]");

    function abrirCrearPlan() {
        formularioPlan.reset();
        formularioPlan.querySelectorAll(".campo").forEach(limpiarErrorCampo);
        document.getElementById("avisoPlan").hidden = true;
        camposLugar.forEach(function (campo) {
            campo.querySelector(".lugar-plan-buscar").dataset.idLugar = "";
            campo.querySelector(".resultados-lugares").replaceChildren();
            cerrarResultados(campo);
        });
        chipsHorario.forEach(function (chip) {
            chip.setAttribute("aria-pressed", "false");
        });

        if (dialogoPlan.dataset.lugarId) {
            const primero = camposLugar[0].querySelector(".lugar-plan-buscar");
            primero.value = dialogoPlan.dataset.lugarTexto;
            primero.dataset.idLugar = dialogoPlan.dataset.lugarId;
            dialogoPlan.dataset.lugarId = "";
        }

        dialogoPlan.showModal();
        nombrePlan.focus();
    }

    function horariosMarcados() {
        return Array.from(chipsHorario)
            .filter(function (chip) { return chip.getAttribute("aria-pressed") === "true"; })
            .map(function (chip) { return chip.dataset.horario; });
    }

    chipsHorario.forEach(function (chip) {
        chip.addEventListener("click", function () {
            limpiarErrorCampo(campoHorarios);
            const marcado = chip.getAttribute("aria-pressed") === "true";
            if (!marcado && horariosMarcados().length >= 3) {
                mostrarErrorCampo(campoHorarios, "Podés marcar hasta 3 horarios.");
                return;
            }
            chip.setAttribute("aria-pressed", marcado ? "false" : "true");
        });
    });

    [nombrePlan, diaPlan].forEach(function (input) {
        input.addEventListener("input", function () {
            limpiarErrorCampo(input.closest(".campo"));
        });
    });

    camposLugar.forEach(function (campo) {
        const input = campo.querySelector(".lugar-plan-buscar");
        const resultados = campo.querySelector(".resultados-lugares");
        let espera = null;

        input.addEventListener("input", function () {
            input.dataset.idLugar = "";
            limpiarErrorCampo(campo);
            clearTimeout(espera);
            const texto = input.value.trim();
            if (texto === "") {
                cerrarResultados(campo);
                return;
            }
            espera = setTimeout(function () {
                buscarLugares(campo, texto);
            }, 250);
        });

        input.addEventListener("blur", function () {
            clearTimeout(espera);
            cerrarResultados(campo);
        });

        // Al volver a un campo sin lugar elegido se busca de nuevo, con las opciones al día
        input.addEventListener("focus", function () {
            const texto = input.value.trim();
            if (texto !== "" && !input.dataset.idLugar) buscarLugares(campo, texto);
        });

        input.addEventListener("keydown", function (evento) {
            if (evento.key === "Escape" && !resultados.hidden) {
                evento.preventDefault();
                cerrarResultados(campo);
            }
        });

        resultados.addEventListener("mousedown", function (evento) {
            evento.preventDefault();
        });
    });

    function idsElegidosEnOtros(campo) {
        return Array.from(camposLugar)
            .filter(function (otro) { return otro !== campo; })
            .map(function (otro) { return Number(otro.querySelector(".lugar-plan-buscar").dataset.idLugar); })
            .filter(Boolean);
    }

    // Deja un solo lugar por id y saca los ya elegidos en las otras opciones
    function lugaresSinRepetir(campo, lugares) {
        const elegidos = idsElegidosEnOtros(campo);
        const porId = new Map(lugares.map(function (lugar) { return [lugar.id, lugar]; }));
        return Array.from(porId.values()).filter(function (lugar) { return !elegidos.includes(lugar.id); });
    }

    async function buscarLugares(campo, texto) {
        const input = campo.querySelector(".lugar-plan-buscar");
        const resultados = campo.querySelector(".resultados-lugares");
        const consulta = new URLSearchParams({ idGrupo: idGrupo, texto: texto });
        idsElegidosEnOtros(campo).forEach(function (id) {
            consulta.append("excluir", id);
        });
        const datos = await pedirJson("/Planes/BuscarLugares?" + consulta);
        // Una respuesta que llega tarde no reabre la lista de un campo que ya no se está usando
        if (input.value.trim() !== texto || input.dataset.idLugar || document.activeElement !== input) return;

        const lugares = lugaresSinRepetir(campo, datos.lugares);
        resultados.innerHTML = "";
        const hayRestricciones = datos.restriccionesGrupo.length > 0;
        const aptos = lugares.filter(function (lugar) { return lugar.apto; });
        const otros = lugares.filter(function (lugar) { return !lugar.apto; });

        if (lugares.length === 0) {
            resultados.innerHTML = '<p class="resultados-vacio"></p>';
            resultados.firstChild.textContent = datos.yaElegidos
                ? "Ese lugar ya está en otra opción."
                : "No encontramos lugares con ese nombre.";
        }
        if (hayRestricciones && aptos.length > 0) {
            const encabezado = document.createElement("div");
            encabezado.className = "resultados-encabezado";
            encabezado.textContent = "Aptos para todo el grupo";
            datos.restriccionesGrupo.forEach(function (nombre) {
                encabezado.appendChild(crearEtiqueta(nombre));
            });
            resultados.appendChild(encabezado);
        }
        aptos.forEach(function (lugar) {
            resultados.appendChild(crearResultado(campo, lugar));
        });
        if (hayRestricciones && otros.length > 0) {
            const separador = document.createElement("div");
            separador.className = "resultados-separador";
            separador.textContent = "Otros lugares";
            resultados.appendChild(separador);
        }
        otros.forEach(function (lugar) {
            resultados.appendChild(crearResultado(campo, lugar));
        });

        resultados.hidden = false;
        input.setAttribute("aria-expanded", "true");
    }

    function crearResultado(campo, lugar) {
        const boton = document.createElement("button");
        boton.type = "button";
        boton.className = "resultado-lugar";
        boton.setAttribute("role", "option");
        boton.innerHTML =
            '<img class="lugar-miniatura" width="48" height="48" alt="" />' +
            '<span class="resultado-lugar-textos">' +
            '  <span class="resultado-lugar-titulo"><span class="resultado-lugar-nombre"></span><span class="resultado-lugar-detalle"></span></span>' +
            '  <span class="etiquetas"></span>' +
            '</span>';
        boton.querySelector(".lugar-miniatura").src = lugar.foto;
        boton.querySelector(".resultado-lugar-nombre").textContent = lugar.nombre;
        boton.querySelector(".resultado-lugar-detalle").textContent = lugar.detalle;
        lugar.restricciones.forEach(function (nombre) {
            boton.querySelector(".etiquetas").appendChild(crearEtiqueta(nombre));
        });

        boton.addEventListener("click", function () {
            const input = campo.querySelector(".lugar-plan-buscar");
            input.value = lugar.textoCampo;
            input.dataset.idLugar = lugar.id;
            cerrarResultados(campo);
            limpiarErrorCampo(campo);
        });
        return boton;
    }

    function crearEtiqueta(nombre) {
        const etiqueta = document.createElement("span");
        etiqueta.className = "etiqueta-restriccion";
        etiqueta.textContent = nombre;
        return etiqueta;
    }

    function cerrarResultados(campo) {
        campo.querySelector(".resultados-lugares").hidden = true;
        campo.querySelector(".lugar-plan-buscar").setAttribute("aria-expanded", "false");
    }

    formularioPlan.querySelector(".aviso-cerrar").addEventListener("click", function () {
        document.getElementById("avisoPlan").hidden = true;
    });

    formularioPlan.addEventListener("submit", async function (evento) {
        evento.preventDefault();
        formularioPlan.querySelectorAll(".campo").forEach(limpiarErrorCampo);
        document.getElementById("avisoPlan").hidden = true;

        for (const campo of camposLugar) {
            const input = campo.querySelector(".lugar-plan-buscar");
            if (input.value.trim() !== "" && !input.dataset.idLugar) {
                mostrarErrorCampo(campo, "Elegí un lugar de la lista.");
                return;
            }
        }

        const datos = {
            idGrupo: idGrupo,
            titulo: nombrePlan.value,
            dia: diaPlan.value,
            horarios: horariosMarcados(),
            lugares: Array.from(camposLugar).map(function (campo) {
                const id = campo.querySelector(".lugar-plan-buscar").dataset.idLugar;
                return id ? Number(id) : null;
            })
        };

        botonPublicar.disabled = true;
        botonPublicar.classList.add("cargando");
        try {
            const respuesta = revisarRespuesta(await fetch("/Planes/CrearPlan", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(datos)
            }));
            const resultado = await respuesta.json();
            if (resultado.ok) {
                dialogoPlan.close();
                enFila(actualizar);
            } else {
                mostrarErrorCampo(formularioPlan.querySelector('[data-campo="' + resultado.campo + '"]'), resultado.mensaje);
            }
        } catch (error) {
            document.getElementById("avisoPlanTexto").textContent = "No pudimos crear el plan. Probá de nuevo en un rato.";
            document.getElementById("avisoPlan").hidden = false;
        }
        botonPublicar.disabled = false;
        botonPublicar.classList.remove("cargando");
    });

    document.getElementById("botonCrearPlan").addEventListener("click", abrirCrearPlan);
    document.getElementById("cerrarCrearPlan").addEventListener("click", function () {
        dialogoPlan.close();
    });
    document.getElementById("cancelarCrearPlan").addEventListener("click", function () {
        dialogoPlan.close();
    });

    if (new URLSearchParams(window.location.search).get("plan") === "1") abrirCrearPlan();
}

function mostrarErrorCampo(campo, mensaje) {
    campo.classList.add("campo-con-error");

    const entrada = campo.querySelector(".campo-entrada");
    const error = document.createElement("span");
    error.className = "campo-error";
    error.innerHTML = '<span class="tooltip"></span><img src="/img/icono-error.svg" width="24" height="24" alt="Error" />';
    error.querySelector(".tooltip").textContent = mensaje;
    entrada.appendChild(error);

    campo.querySelector("input, button").focus();
}

function limpiarErrorCampo(campo) {
    campo.classList.remove("campo-con-error");
    const error = campo.querySelector(".campo-error");
    if (error) error.remove();
}

const parametros = new URLSearchParams(window.location.search);
if (parametros.get("crear") === "1") abrirFormularioGrupo(false);
if (parametros.has("crear") || parametros.has("plan") || parametros.has("lugar")) {
    history.replaceState(null, "", window.location.pathname);
}
