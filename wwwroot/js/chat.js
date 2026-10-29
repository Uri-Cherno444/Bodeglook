// 07 · Chat de grupo y 07g · Crear grupo.
// Los mensajes se piden con fetch: todos al abrir y después, cada 3 segundos, solo los nuevos.
// En cada consulta también se refresca la lista "Chats" (hora, orden, no leídos y grupos nuevos).

const conversacion = document.getElementById("conversacion");
// En /Grupos sin ningún grupo no hay conversación: solo la lista y el modal
const idGrupo = conversacion ? Number(conversacion.dataset.idGrupo) : 0;
const listaMensajes = document.getElementById("listaMensajes");
const formularioMensaje = document.getElementById("formularioMensaje");
const campoMensaje = document.getElementById("campoMensaje");

let ultimoId = 0;     // el último mensaje que ya está dibujado
let ultimoDia = "";   // para poner el separador "Hoy" / "Ayer" / "Lun 26/10" cuando cambia el día

// Las consultas y los envíos van de a uno, en fila: así un mensaje nunca se dibuja dos veces
// ni en desorden aunque el envío y la consulta de los 3 segundos coincidan.
let pedidos = Promise.resolve();
function enFila(tarea) {
    pedidos = pedidos.then(tarea).catch(function () { });
    return pedidos;
}

// Si la sesión se cerró o ya no es integrante, se va de la página
function revisarRespuesta(respuesta) {
    if (respuesta.status === 401) window.location.href = "/Cuenta/IniciarSesion";
    if (respuesta.status === 403) window.location.href = "/Grupos";
    if (!respuesta.ok) throw new Error("Error " + respuesta.status);
    return respuesta;
}

// Devuelve { mensajes, encuestas }: los mensajes nuevos y cómo están todas las encuestas del grupo
async function traerMensajesNuevos() {
    const respuesta = revisarRespuesta(await fetch("/Grupos/Mensajes?id=" + idGrupo + "&desde=" + ultimoId));
    const datos = await respuesta.json();
    const mensajes = datos.mensajes;

    // Se baja hasta el final solo si ya estaba abajo (o es la primera carga), para no cortarle la lectura
    const estabaAbajo = ultimoId === 0 ||
        listaMensajes.scrollHeight - listaMensajes.scrollTop - listaMensajes.clientHeight < 80;

    mensajes.forEach(dibujarMensaje);
    // Después de dibujar los mensajes, así las encuestas nuevas ya tienen su tarjeta
    actualizarEncuestas(datos.encuestas);

    if (mensajes.length === 0) return;
    const ultimo = mensajes[mensajes.length - 1];
    if (estabaAbajo || ultimo.esMio) listaMensajes.scrollTop = listaMensajes.scrollHeight;
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

    // Mensaje encuesta: va la tarjeta vacía y la llena actualizarEncuestas
    if (mensaje.idPlan) {
        fila.className = "mensaje mensaje-encuesta" + (mensaje.esMio ? " mensaje-mio" : "");
        fila.innerHTML = (mensaje.esMio ? "" : '<span class="avatar avatar-mensaje"></span>') + '<div class="encuesta"></div>';
        if (!mensaje.esMio) fila.querySelector(".avatar").textContent = mensaje.iniciales;
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
        fila.querySelector(".avatar").textContent = mensaje.iniciales;
        fila.querySelector(".mensaje-autor").textContent = mensaje.nombre;
    }

    // textContent para que un mensaje con "<" o ">" no se interprete como HTML
    fila.querySelector(".mensaje-texto p").textContent = mensaje.contenido;
    fila.querySelector(".mensaje-hora").textContent = mensaje.hora;
    listaMensajes.appendChild(fila);
}

// ---------- Encuestas de los planes (07 y 07e) ----------

// Solo se vuelve a dibujar la encuesta que cambió (alguien votó o el creador la confirmó)
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

// La encuesta tiene dos partes con el mismo diseño: LUGAR y HORARIO.
// estado: "votacion", "confirmado" o "cancelado".
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

    // Resaltado: mi voto mientras se vota; el que ganó cuando está confirmado; ninguno si se canceló
    const lugarResaltado = abierto ? encuesta.miVoto : confirmado ? encuesta.idElegido : null;
    const horarioResaltado = abierto ? encuesta.miVotoHorario : confirmado ? encuesta.idHorarioElegido : null;

    const lugares = tarjeta.querySelector(".encuesta-lugares");
    lugares.setAttribute("aria-label", "Lugar");
    encuesta.opciones.forEach(function (opcion) {
        lugares.appendChild(crearOpcionEncuesta(encuesta, "lugar", opcion.idRestaurante, opcion.nombre, opcion.detalle,
            opcion.votos, opcion.porcentaje, opcion.idRestaurante === lugarResaltado));
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

// Una opción con el radio del kit, cantidad de votos y barra. tipo: "lugar" u "horario" (el horario no lleva detalle).
function crearOpcionEncuesta(encuesta, tipo, id, nombre, detalle, votos, porcentaje, resaltada) {
    const abierto = encuesta.estado === "votacion";
    const fila = document.createElement("label");
    fila.className = "opcion-plan";
    if (resaltada) fila.classList.add("opcion-plan-elegida");
    if (!abierto && !resaltada) fila.classList.add("opcion-plan-apagada");
    fila.innerHTML =
        '<span class="opcion-plan-fila">' +
        '  <span class="opcion-plan-elegir"><span class="opcion-plan-nombre"></span>' +
        '    <input type="radio" class="radio" /><span class="radio-circulo" aria-hidden="true"></span></span>' +
        '  <span class="opcion-plan-votos"></span>' +
        '</span>' +
        (detalle ? '<span class="opcion-plan-detalle"></span>' : '') +
        '<span class="barra-votos"><span class="barra-votos-avance"></span></span>';

    fila.querySelector(".opcion-plan-nombre").textContent = nombre;
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

// Votar: se toca una opción de lugar o de horario. Se pueden cambiar los dos mientras esté En votación.
if (listaMensajes) {
    listaMensajes.addEventListener("change", function (evento) {
        const radio = evento.target;
        if (!radio.classList.contains("radio")) return;
        const esHorario = radio.dataset.tipo === "horario";
        const datos = esHorario
            ? { idPlan: Number(radio.dataset.idPlan), idHorario: Number(radio.value) }
            : { idPlan: Number(radio.dataset.idPlan), idRestaurante: Number(radio.value) };
        enFila(async function () {
            revisarRespuesta(await fetch(esHorario ? "/Planes/VotarHorario" : "/Planes/Votar", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(datos)
            }));
            await traerMensajesNuevos();
        });
    });
}

// Reemplaza la lista "Chats" por la que arma el servidor (_Chats.cshtml)
async function actualizarListaChats() {
    const respuesta = revisarRespuesta(await fetch("/Grupos/ListaChats?abierto=" + idGrupo));
    const html = await respuesta.text();
    document.querySelector(".panel-chats").outerHTML = html;
}

// Primero los mensajes (que marcan el chat como leído) y después la lista, así el abierto queda sin no leídos
async function actualizar() {
    if (idGrupo) await traerMensajesNuevos();
    await actualizarListaChats();
}

// Enter dentro del campo también manda el formulario
if (formularioMensaje) {
    formularioMensaje.addEventListener("submit", function (evento) {
        evento.preventDefault();
        const contenido = campoMensaje.value.trim();
        if (contenido === "") return;

        campoMensaje.value = "";
        enFila(async function () {
            const respuesta = revisarRespuesta(await fetch("/Grupos/EnviarMensaje", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ idGrupo: idGrupo, contenido: contenido })
            }));
            const resultado = await respuesta.json();
            if (!resultado.ok) {
                campoMensaje.value = contenido;
                return;
            }
            await actualizar();
        });
        campoMensaje.focus();
    });
}

if (idGrupo) enFila(traerMensajesNuevos);

// Cada 3 segundos, si la consulta anterior ya terminó
let consultando = false;
setInterval(function () {
    if (consultando) return;
    consultando = true;
    enFila(actualizar).then(function () {
        consultando = false;
    });
}, 3000);

// ---------- 07g · Crear grupo ----------

const dialogoGrupo = document.getElementById("dialogoCrearGrupo");
const formularioGrupo = document.getElementById("formularioGrupo");
const listaAmigos = document.getElementById("listaAmigos");
const buscarAmigos = document.getElementById("buscarAmigos");
const contadorSeleccionados = document.getElementById("contadorSeleccionados");
const campoFoto = document.getElementById("campoFoto");
const fotoNuevaCirculo = document.getElementById("fotoNuevaCirculo");
const botonCrearGrupo = formularioGrupo.querySelector("button[type=submit]");

async function abrirCrearGrupo() {
    formularioGrupo.reset();
    fotoNuevaCirculo.textContent = "+";
    ocultarAviso();
    listaAmigos.innerHTML = "";
    actualizarContador();
    dialogoGrupo.showModal();

    try {
        const respuesta = revisarRespuesta(await fetch("/Grupos/Amigos"));
        const amigos = await respuesta.json();
        if (amigos.length === 0) {
            listaAmigos.innerHTML = '<p class="lista-amigos-vacia">Todavía no hay amigos para sumar.</p>';
        }
        amigos.forEach(function (amigo) {
            listaAmigos.appendChild(crearFilaAmigo(amigo));
        });
    } catch (error) {
        mostrarAviso("No pudimos traer tus amigos. Probá de nuevo en un rato.");
    }
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

// "Julián" se encuentra escribiendo "julian"
function sinAcentos(texto) {
    return texto.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();
}

// El buscador solo esconde filas: los que ya estaban marcados siguen marcados
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

// Muestra la foto elegida adentro del círculo
campoFoto.addEventListener("change", function () {
    fotoNuevaCirculo.textContent = "+";
    if (campoFoto.files.length === 0) return;
    const imagen = document.createElement("img");
    imagen.src = URL.createObjectURL(campoFoto.files[0]);
    imagen.alt = "";
    fotoNuevaCirculo.textContent = "";
    fotoNuevaCirculo.appendChild(imagen);
});

document.getElementById("cerrarCrearGrupo").addEventListener("click", function () {
    dialogoGrupo.close();
});

document.getElementById("cancelarCrearGrupo").addEventListener("click", function () {
    dialogoGrupo.close();
});

formularioGrupo.querySelector(".aviso-cerrar").addEventListener("click", ocultarAviso);

// Se manda como formulario (no JSON) porque lleva la foto
formularioGrupo.addEventListener("submit", async function (evento) {
    evento.preventDefault();
    ocultarAviso();
    botonCrearGrupo.disabled = true;
    botonCrearGrupo.classList.add("cargando");

    try {
        const respuesta = revisarRespuesta(await fetch("/Grupos/CrearGrupo", {
            method: "POST",
            body: new FormData(formularioGrupo)
        }));
        const resultado = await respuesta.json();
        if (resultado.ok) {
            window.location.href = resultado.destino;
            return;
        }
        mostrarAviso(resultado.mensaje);
    } catch (error) {
        mostrarAviso("No pudimos crear el grupo. Probá de nuevo en un rato.");
    }

    botonCrearGrupo.disabled = false;
    botonCrearGrupo.classList.remove("cargando");
});

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

// El ícono de "Chats" abre el modal. La lista se reemplaza cada 3 segundos, por eso se escucha en document.
document.addEventListener("click", function (evento) {
    if (evento.target.closest(".panel-chats-crear")) {
        evento.preventDefault();
        abrirCrearGrupo();
    }
});

// ---------- 07b · Crear plan ----------

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
            cerrarResultados(campo);
        });
        chipsHorario.forEach(function (chip) {
            chip.setAttribute("aria-pressed", "false");
        });

        // Si viene desde el Detalle de un restaurante, ese lugar va en el primer campo (solo la primera vez)
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

    // Selección múltiple: cada chip se marca y desmarca. Hasta 3 horarios.
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

    // Al escribir en un campo con error, se le saca el error
    [nombrePlan, diaPlan].forEach(function (input) {
        input.addEventListener("input", function () {
            limpiarErrorCampo(input.closest(".campo"));
        });
    });

    // 07c · Cada campo de lugar busca mientras escribís (espera 250 ms a que deje de escribir)
    camposLugar.forEach(function (campo) {
        const input = campo.querySelector(".lugar-plan-buscar");
        const resultados = campo.querySelector(".resultados-lugares");
        let espera = null;

        input.addEventListener("input", function () {
            // Si cambia el texto, el lugar que había elegido ya no vale
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
            cerrarResultados(campo);
        });

        // Escape cierra la lista sin cerrar el modal
        input.addEventListener("keydown", function (evento) {
            if (evento.key === "Escape" && !resultados.hidden) {
                evento.preventDefault();
                cerrarResultados(campo);
            }
        });

        // Así el clic en un resultado no le saca el foco al campo antes de elegirlo
        resultados.addEventListener("mousedown", function (evento) {
            evento.preventDefault();
        });
    });

    async function buscarLugares(campo, texto) {
        const input = campo.querySelector(".lugar-plan-buscar");
        const resultados = campo.querySelector(".resultados-lugares");
        const respuesta = revisarRespuesta(await fetch("/Planes/BuscarLugares?idGrupo=" + idGrupo + "&texto=" + encodeURIComponent(texto)));
        const datos = await respuesta.json();
        // Si mientras tanto siguió escribiendo, esta respuesta ya no sirve
        if (input.value.trim() !== texto) return;

        resultados.innerHTML = "";
        const hayRestricciones = datos.restriccionesGrupo.length > 0;
        const aptos = datos.lugares.filter(function (lugar) { return lugar.apto; });
        const otros = datos.lugares.filter(function (lugar) { return !lugar.apto; });

        if (datos.lugares.length === 0) {
            resultados.innerHTML = '<p class="resultados-vacio">No encontramos lugares con ese nombre.</p>';
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
            '<span class="resultado-lugar-titulo"><span class="resultado-lugar-nombre"></span><span class="resultado-lugar-detalle"></span></span>' +
            '<span class="etiquetas"></span>';
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

        // Si escribió en un campo de lugar pero no eligió nada de la lista
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
                // La encuesta aparece en el chat como un mensaje más
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

// Estado de error del kit: ícono de atención + tooltip negro (igual que en cuenta.js)
function mostrarErrorCampo(campo, mensaje) {
    campo.classList.add("campo-con-error");

    const entrada = campo.querySelector(".campo-entrada");
    const error = document.createElement("span");
    error.className = "campo-error";
    error.innerHTML = '<span class="tooltip"></span><img src="/img/icono-error.svg" width="24" height="24" alt="Error" />';
    error.querySelector(".tooltip").textContent = mensaje;
    entrada.appendChild(error);

    // Los horarios no tienen input: se enfoca el primer chip
    campo.querySelector("input, button").focus();
}

function limpiarErrorCampo(campo) {
    campo.classList.remove("campo-con-error");
    const error = campo.querySelector(".campo-error");
    if (error) error.remove();
}

// Viene de otra pantalla: /Grupos?crear=1 abre Crear grupo y /Planes/NuevoPlan llega con ?plan=1
const parametros = new URLSearchParams(window.location.search);
if (parametros.get("crear") === "1") abrirCrearGrupo();
if (parametros.has("crear") || parametros.has("plan") || parametros.has("lugar")) {
    history.replaceState(null, "", window.location.pathname);
}
