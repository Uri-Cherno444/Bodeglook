// 05 · Descubrir: busca con fetch a /Restaurantes/Resultados y arma la lista, la paginación
// y los pins del mapa sin recargar. El mapa es Leaflet con OpenStreetMap.

const formBusqueda = document.getElementById("formBusqueda");
const textoBusqueda = document.getElementById("textoBusqueda");
const listaResultados = document.getElementById("listaResultados");
const sinResultados = document.getElementById("sinResultados");
const paginacion = document.getElementById("paginacion");
const busquedasRecientes = document.getElementById("busquedasRecientes");
const chipsRecientes = document.getElementById("chipsRecientes");
const selectorBarrio = document.getElementById("selectorBarrio");
const selectorOrden = document.getElementById("selectorOrden");
const resultadosOrden = document.getElementById("resultadosOrden");
const avisoUbicacion = document.getElementById("avisoUbicacion");

// "No encontramos lugares con esa búsqueda." (el texto de siempre está en la vista)
const textoSinResultados = sinResultados.textContent;

// Lo que se buscó por última vez (las páginas siguientes usan el mismo texto)
let textoActual = textoBusqueda.value.trim();
let paginaActual = 1;

// La ubicación del navegador. Solo vive en esta página: se manda en cada búsqueda y no se guarda.
// Si es null, el servidor mide desde el barrio del perfil.
let latitud = null;
let longitud = null;

// ---------- Mapa ----------

const mapa = L.map("mapa", { scrollWheelZoom: false }).setView([-34.6037, -58.3816], 12);
L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(mapa);

const pinNegro = L.icon({ iconUrl: "/img/icono-pin-mapa.svg", iconSize: [44, 55], iconAnchor: [22, 54] });
const pinNaranja = L.icon({ iconUrl: "/img/icono-pin-mapa-elegido.svg", iconSize: [64, 80], iconAnchor: [32, 78] });

let pins = [];
let pinElegido = null;

// ---------- Buscar ----------

formBusqueda.addEventListener("submit", function (evento) {
    evento.preventDefault();
    textoActual = textoBusqueda.value.trim();
    buscar(1);
});

// Tocar una búsqueda reciente la vuelve a buscar
chipsRecientes.addEventListener("click", function (evento) {
    const chip = evento.target.closest(".chip-reciente");
    if (!chip) return;
    textoBusqueda.value = chip.textContent;
    textoActual = chip.textContent;
    buscar(1);
});

// El barrio que estaba elegido antes del cambio ("" = Todos los barrios)
let barrioAnterior = selectorBarrio.value;

// Elegir un barrio muestra solo los lugares de ese barrio, desde la página 1
selectorBarrio.addEventListener("change", function () {
    cambiarBarrio(selectorBarrio.value);
});

// Cambiar el orden vuelve a la página 1
selectorOrden.addEventListener("change", function () {
    buscar(1);
});

// Con barrio se ve el selector "Más cerca" / "Mejor puntuados"; sin barrio, el texto "Mejor puntuados" de antes.
// Al pasar de "Todos los barrios" a un barrio, el orden arranca en "Más cerca"; entre barrios se mantiene.
function cambiarBarrio(idBarrio) {
    selectorBarrio.value = idBarrio;
    if (barrioAnterior === "") selectorOrden.value = "cerca";
    barrioAnterior = idBarrio;

    selectorOrden.hidden = idBarrio === "";
    resultadosOrden.hidden = idBarrio !== "";
    buscar(1);
}

async function buscar(pagina) {
    listaResultados.setAttribute("aria-busy", "true");

    // Texto, página, barrio y orden (el orden solo con barrio). La ubicación solo si el navegador la dio.
    let direccion = "/Restaurantes/Resultados?texto=" + encodeURIComponent(textoActual) + "&pagina=" + pagina;
    if (selectorBarrio.value !== "") direccion += "&idBarrio=" + selectorBarrio.value + "&orden=" + selectorOrden.value;
    if (latitud !== null && longitud !== null) direccion += "&latitud=" + latitud + "&longitud=" + longitud;

    try {
        const respuesta = await fetch(direccion);

        // Si se venció la sesión, al login
        if (respuesta.status === 401) {
            window.location.href = "/Cuenta/IniciarSesion";
            return;
        }

        const datos = await respuesta.json();
        paginaActual = datos.pagina;
        mostrarAvisoUbicacion(datos.desdeBarrio);
        mostrarResultados(datos.resultados);
        mostrarSinResultados(datos);
        mostrarPaginacion(datos.pagina, datos.totalPaginas);
        mostrarRecientes(datos.recientes);
        mostrarPins(datos.resultados);
    } catch (error) {
        // Si falla la conexión, queda lo que se estaba viendo
    }

    listaResultados.removeAttribute("aria-busy");
}

// ---------- Lista ----------

function mostrarResultados(resultados) {
    const plantilla = document.getElementById("plantillaResultado");
    listaResultados.innerHTML = "";
    // Los créditos de Geoapify y OpenStreetMap se muestran si algún resultado viene de ahí
    document.getElementById("creditosResultados").hidden = !resultados.some(function (lugar) { return lugar.origen === "Geoapify"; });

    resultados.forEach(function (lugar) {
        const tarjeta = plantilla.content.firstElementChild.cloneNode(true);
        tarjeta.dataset.idRestaurante = lugar.id;
        // Solo el Pro destacado (uno, arriba de la página 1) lleva el chip "Recomendado"
        if (lugar.destacado) tarjeta.classList.add("resultado-pro");
        else tarjeta.querySelector(".chip-recomendado").remove();

        const foto = tarjeta.querySelector(".resultado-foto");
        if (lugar.foto) foto.src = lugar.foto;
        else foto.removeAttribute("src");

        const nombre = tarjeta.querySelector(".resultado-nombre");
        nombre.textContent = lugar.nombre;
        nombre.href = "/Restaurantes/Detalle/" + lugar.id;

        tarjeta.querySelector(".resultado-categoria").textContent = textoCategoria(lugar);
        tarjeta.querySelector(".resultado-barrio").textContent = textoBarrio(lugar);
        pintarCorazon(tarjeta.querySelector(".boton-favorito"), lugar.esFavorito);

        // Pasar por arriba de una tarjeta marca su pin en el mapa
        tarjeta.addEventListener("mouseenter", function () {
            elegirPin(lugar.id);
        });

        listaResultados.appendChild(tarjeta);
    });
}

// Sin resultados: el texto de siempre o, si el texto está en otro barrio,
// "No encontramos sushi en Palermo. Hay 3 en Belgrano" con el link que cambia el desplegable a ese barrio
function mostrarSinResultados(datos) {
    sinResultados.hidden = datos.resultados.length > 0;
    if (!datos.otroBarrio) {
        sinResultados.textContent = textoSinResultados;
        return;
    }

    sinResultados.textContent = "No encontramos " + textoActual + " en " + datos.barrioElegido + ". ";
    const link = document.createElement("button");
    link.type = "button";
    link.className = "sin-resultados-link";
    link.textContent = "Hay " + datos.otroBarrio.cantidad + " en " + datos.otroBarrio.nombre;
    link.addEventListener("click", function () {
        cambiarBarrio(String(datos.otroBarrio.id));
    });
    sinResultados.appendChild(link);
}

// "Palermo · A unas 8 cuadras" (sin distancia, solo el barrio)
function textoBarrio(lugar) {
    if (lugar.distanciaKm === null) return lugar.barrio;
    return lugar.barrio + " · " + textoDistancia(lugar.distanciaKm);
}

// 1 cuadra = 100 m. Menos de 50 m -> "A menos de 1 cuadra"; de 50 a 149 m -> "A 1 cuadra";
// desde 150 m y hasta 20 cuadras -> "A unas N cuadras"; más -> en km con un decimal.
// 0,8 km -> "A unas 8 cuadras"; 3,1 km -> "A 3,1 km"
function textoDistancia(km) {
    if (km < 0.05) return "A menos de 1 cuadra";
    if (km < 0.15) return "A 1 cuadra";
    const cuadras = Math.round(km * 10);
    if (cuadras <= 20) return "A unas " + cuadras + " cuadras";
    return "A " + (Math.round(km * 10) / 10).toFixed(1).replace(".", ",") + " km";
}

// ---------- Ubicación ----------

// Si el servidor midió desde el barrio del perfil, se avisa arriba de la lista
function mostrarAvisoUbicacion(desdeBarrio) {
    if (desdeBarrio) {
        avisoUbicacion.textContent = "Distancias aproximadas desde tu barrio (" + desdeBarrio + "). Activá la ubicación para verlas desde donde estás.";
        avisoUbicacion.hidden = false;
    } else {
        avisoUbicacion.hidden = true;
    }
}

// Al entrar se pide la ubicación y después se hace la primera búsqueda.
// Si la bloquea, falla o el navegador no la tiene (o la página no es HTTPS), se busca sin ubicación.
// Si no contesta el permiso en 5 segundos, se busca igual; si la da más tarde, se vuelve a buscar con ella.
function pedirUbicacionYBuscar() {
    if (!navigator.geolocation) {
        buscar(1);
        return;
    }

    let yaBusco = false;
    const espera = setTimeout(function () {
        yaBusco = true;
        buscar(1);
    }, 5000);

    navigator.geolocation.getCurrentPosition(
        function (posicion) {
            clearTimeout(espera);
            latitud = posicion.coords.latitude;
            longitud = posicion.coords.longitude;
            buscar(yaBusco ? paginaActual : 1);
            yaBusco = true;
        },
        function () {
            clearTimeout(espera);
            if (!yaBusco) buscar(1);
            yaBusco = true;
        },
        { timeout: 10000, maximumAge: 300000 }
    );
}

// "Vinoteca · ★ 4.7" (sin reseñas, solo la categoría)
function textoCategoria(lugar) {
    if (lugar.puntaje === null) return lugar.categoria + " · Sin reseñas todavía";
    return lugar.categoria + " · ★ " + lugar.puntaje;
}

// ---------- Paginación ----------

// Muestra hasta 6 números alrededor de la página actual y "..." si hay más
function mostrarPaginacion(pagina, totalPaginas) {
    paginacion.innerHTML = "";
    paginacion.hidden = totalPaginas <= 1;
    if (totalPaginas <= 1) return;

    paginacion.appendChild(crearFlecha("/img/icono-paginacion-anterior.svg", "Página anterior", pagina - 1, pagina === 1));

    let desde = Math.max(1, pagina - 1);
    let hasta = Math.min(totalPaginas, desde + 5);
    desde = Math.max(1, hasta - 5);

    for (let numero = desde; numero <= hasta; numero++) {
        const boton = document.createElement("button");
        boton.type = "button";
        boton.className = "paginacion-numero";
        boton.textContent = numero;
        if (numero === pagina) boton.setAttribute("aria-current", "page");
        else boton.addEventListener("click", function () { buscar(numero); });
        paginacion.appendChild(boton);
    }

    if (hasta < totalPaginas) {
        const puntos = document.createElement("span");
        puntos.className = "paginacion-puntos";
        puntos.textContent = "...";
        paginacion.appendChild(puntos);
    }

    paginacion.appendChild(crearFlecha("/img/icono-paginacion-siguiente.svg", "Página siguiente", pagina + 1, pagina === totalPaginas));
}

function crearFlecha(icono, texto, destino, desactivada) {
    const boton = document.createElement("button");
    boton.type = "button";
    boton.className = "paginacion-flecha";
    boton.setAttribute("aria-label", texto);
    boton.disabled = desactivada;
    boton.innerHTML = '<img src="' + icono + '" width="6" height="10" alt="" />';
    boton.addEventListener("click", function () { buscar(destino); });
    return boton;
}

// ---------- Búsquedas recientes ----------

function mostrarRecientes(recientes) {
    chipsRecientes.innerHTML = "";
    busquedasRecientes.hidden = recientes.length === 0;

    recientes.forEach(function (texto) {
        const chip = document.createElement("button");
        chip.type = "button";
        chip.className = "chip chip-crema chip-filtro chip-reciente";
        chip.textContent = texto;
        chipsRecientes.appendChild(chip);
    });
}

// ---------- Pins del mapa ----------

// Un pin por resultado de la página (los que no tienen coordenadas no van al mapa)
function mostrarPins(resultados) {
    pins.forEach(function (pin) { pin.marcador.remove(); });
    pins = [];
    pinElegido = null;

    resultados.forEach(function (lugar) {
        if (lugar.latitud === null || lugar.longitud === null) return;

        const marcador = L.marker([lugar.latitud, lugar.longitud], { icon: pinNegro, title: lugar.nombre, keyboard: true }).addTo(mapa);
        marcador.on("click", function () { elegirPin(lugar.id); });
        pins.push({ id: lugar.id, lugar: lugar, marcador: marcador });
    });

    if (pins.length === 0) return;

    const limites = L.latLngBounds(pins.map(function (pin) { return pin.marcador.getLatLng(); }));
    mapa.fitBounds(limites, { padding: [80, 80], maxZoom: 15 });

    // Arranca elegido el primero, como en el Figma
    elegirPin(pins[0].id);
}

// El pin elegido se ve naranja y más grande, con su tarjetita arriba
function elegirPin(idRestaurante) {
    const pin = pins.find(function (p) { return p.id === idRestaurante; });
    if (!pin || pin === pinElegido) return;

    if (pinElegido) {
        pinElegido.marcador.setIcon(pinNegro);
        pinElegido.marcador.setZIndexOffset(0);
        pinElegido.marcador.unbindTooltip();
    }

    pin.marcador.setIcon(pinNaranja);
    pin.marcador.setZIndexOffset(1000);
    pin.marcador.bindTooltip(crearTarjetaMapa(pin.lugar), {
        permanent: true,
        direction: "top",
        offset: [0, -86],
        className: "mapa-tooltip",
        interactive: true
    }).openTooltip();
    pinElegido = pin;
}

function crearTarjetaMapa(lugar) {
    const plantilla = document.getElementById("plantillaTarjetaMapa");
    const tarjeta = plantilla.content.firstElementChild.cloneNode(true);
    tarjeta.href = "/Restaurantes/Detalle/" + lugar.id;

    const imagen = tarjeta.querySelector(".mapa-tarjeta-imagen");
    if (lugar.foto) imagen.src = lugar.foto;
    else imagen.remove();

    tarjeta.querySelector(".mapa-tarjeta-nombre").textContent = lugar.nombre;
    tarjeta.querySelector(".mapa-tarjeta-detalle").textContent = textoCategoria(lugar);
    return tarjeta;
}

// ---------- Favoritos ----------

listaResultados.addEventListener("click", async function (evento) {
    const boton = evento.target.closest(".boton-favorito");
    if (!boton) return;

    const idRestaurante = Number(boton.closest(".resultado").dataset.idRestaurante);
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
        if (resultado.ok) pintarCorazon(boton, resultado.esFavorito);
    } catch (error) {
        // Si falla la conexión, el corazón queda como estaba
    }

    boton.disabled = false;
});

function pintarCorazon(boton, esFavorito) {
    boton.classList.toggle("es-favorito", esFavorito);
    boton.setAttribute("aria-pressed", esFavorito ? "true" : "false");
    boton.setAttribute("aria-label", esFavorito ? "Sacar de favoritos" : "Agregar a favoritos");
}

// Si el navegador recuerda un barrio elegido (al volver atrás), se ve el selector de orden que corresponde
selectorOrden.hidden = selectorBarrio.value === "";
resultadosOrden.hidden = selectorBarrio.value !== "";

// Al entrar se busca lo que vino en la ruta (o todo, si vino vacío), con la ubicación si la da
pedirUbicacionYBuscar();
