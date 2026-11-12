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
const avisoUbicacion = document.getElementById("avisoUbicacion");
const resultadosCuenta = document.getElementById("resultadosCuenta");

// "No encontramos lugares con esa búsqueda." (el texto de siempre está en la vista)
const textoSinResultados = sinResultados.textContent;

// Lo que se buscó por última vez (las páginas siguientes usan el mismo texto)
let textoActual = textoBusqueda.value.trim();
let paginaActual = 1;
// El chip de categoría elegido ("" = todas)
let categoriaActual = "";
// true: la lista y los pins son de lo que se ve en el mapa (y cambian al moverlo o hacer zoom).
// false: recién se buscó o se cambió un filtro, así que son de toda la Ciudad y el mapa se ajusta a los resultados.
let zonaActiva = true;
// Para ignorar respuestas viejas si se pidió otra cosa mientras tanto
let numeroPedido = 0;

// La ubicación del navegador. Solo vive en esta página: se manda en cada búsqueda y no se guarda.
// Si es null, el servidor mide desde el barrio del perfil.
let latitud = null;
let longitud = null;

// ---------- Mapa ----------

const elementoMapa = document.getElementById("mapa");
// Donde arranca el mapa si el navegador no da la ubicación: el barrio del perfil (zoom 15) o el centro de la Ciudad (zoom 12)
const centroLatitud = Number(elementoMapa.dataset.latitud);
const centroLongitud = Number(elementoMapa.dataset.longitud);
const centroEsBarrio = elementoMapa.dataset.esBarrio === "true";

const mapa = L.map("mapa", { scrollWheelZoom: true, minZoom: 10 })
    .setView([centroLatitud, centroLongitud], centroEsBarrio ? 15 : 12);

// Mapa de OpenStreetMap. El punto "Estás acá" va en una capa DEBAJO del mapa (150) y el mapa se mezcla con
// "multiply": el fondo y las calles blancas dejan ver el punto, pero el texto oscuro (nombres de calles) queda por encima y no se tapa.
L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(mapa);
mapa.createPane("ubicacion").style.zIndex = 150;
mapa.getPane("tilePane").style.mixBlendMode = "multiply";

// Pins estilo Google Maps: gota con un círculo blanco adentro y sombra suave. El elegido es más grande y más oscuro.
function crearPin(ancho, alto, color) {
    const svg = '<svg width="' + ancho + '" height="' + alto + '" viewBox="0 0 30 40" aria-hidden="true">' +
        '<path d="M15 1C7.3 1 1 7.2 1 14.9c0 10.2 12.3 22.8 13.1 23.6.5.5 1.3.5 1.8 0C16.7 37.7 29 25.1 29 14.9 29 7.2 22.7 1 15 1z" fill="' + color + '" stroke="#fff" stroke-width="2"/>' +
        '<circle cx="15" cy="15" r="5.5" fill="#fff"/></svg>';
    return L.divIcon({
        className: "mapa-pin",
        html: svg,
        iconSize: [ancho, alto],
        iconAnchor: [ancho / 2, alto]
    });
}
const pinNormal = crearPin(28, 37, "#ff914d");
const pinElegidoIcono = crearPin(40, 53, "#e8590c");

let pins = [];
let pinElegido = null;

// El punto de "Estás acá": chico, con una onda suave (ver .mi-ubicacion en descubrir.css)
const iconoUbicacion = L.divIcon({
    className: "",
    html: '<span class="mi-ubicacion"><span class="mi-ubicacion-onda"></span><span class="mi-ubicacion-punto"></span></span>',
    iconSize: [44, 44],
    iconAnchor: [22, 22]
});
let puntoUbicacion = null;

// Cuando el mapa se mueve por código (centrar, ajustar a los resultados) no hay que volver a buscar por zona
let movimientoDeCodigo = false;

function moverMapa(accion) {
    movimientoDeCodigo = true;
    accion();
    movimientoDeCodigo = false;
}

// Botón "Mi ubicación" (abajo a la derecha del mapa): vuelve a donde está la persona
const botonUbicacion = L.control({ position: "bottomright" });
botonUbicacion.onAdd = function () {
    const boton = L.DomUtil.create("button", "mapa-boton-ubicacion");
    boton.type = "button";
    boton.setAttribute("aria-label", "Ir a mi ubicación");
    boton.title = "Ir a mi ubicación";
    L.DomEvent.disableClickPropagation(boton);
    L.DomEvent.on(boton, "click", function () {
        if (latitud === null || longitud === null) return;
        zonaActiva = true;
        moverMapa(function () { mapa.setView([latitud, longitud], 17, { animate: false }); });
        buscar(1);
    });
    return boton;
};
botonUbicacion.addTo(mapa);

// ---------- Buscar ----------

formBusqueda.addEventListener("submit", function (evento) {
    evento.preventDefault();
    textoActual = textoBusqueda.value.trim();
    zonaActiva = false;
    buscar(1);
});

// Tocar una búsqueda reciente la vuelve a buscar
chipsRecientes.addEventListener("click", function (evento) {
    const chip = evento.target.closest(".chip-reciente");
    if (!chip) return;
    textoBusqueda.value = chip.textContent;
    textoActual = chip.textContent;
    zonaActiva = false;
    buscar(1);
});

// Elegir un barrio muestra solo los lugares de ese barrio, desde la página 1
selectorBarrio.addEventListener("change", function () {
    cambiarBarrio(selectorBarrio.value);
});

// Cambiar el orden (Más cerca / Mejor puntuados) vuelve a la página 1, con o sin barrio
selectorOrden.addEventListener("change", function () {
    buscar(1);
});

// Cambiar de barrio busca en toda la Ciudad (con ese barrio) y el mapa se ajusta a los resultados. El orden elegido se mantiene.
function cambiarBarrio(idBarrio) {
    selectorBarrio.value = idBarrio;
    zonaActiva = false;
    buscar(1);
}

// pagina 1 también pide los pins del mapa; las demás páginas solo cambian la lista (los pins son los mismos).
async function buscar(pagina) {
    const pedido = ++numeroPedido;
    const pedirPins = pagina === 1;
    listaResultados.setAttribute("aria-busy", "true");

    // Texto, página, orden, barrio y categoría. La ubicación solo si el navegador la dio.
    let direccion = "/Restaurantes/Resultados?texto=" + encodeURIComponent(textoActual) + "&pagina=" + pagina + "&orden=" + selectorOrden.value;
    if (selectorBarrio.value !== "") direccion += "&idBarrio=" + selectorBarrio.value;
    if (categoriaActual !== "") direccion += "&idCategoria=" + categoriaActual;
    if (latitud !== null && longitud !== null) direccion += "&latitud=" + latitud + "&longitud=" + longitud;
    if (pedirPins) direccion += "&pines=true";

    // Con zona activa se manda lo que se ve del mapa; cuanto menos zoom, menos lugares (los mejores)
    if (zonaActiva) {
        const limites = mapa.getBounds();
        direccion += "&norte=" + limites.getNorth() + "&sur=" + limites.getSouth() +
            "&este=" + limites.getEast() + "&oeste=" + limites.getWest() + "&zoom=" + mapa.getZoom();
    }

    try {
        const respuesta = await fetch(direccion);

        // Si se venció la sesión, al login
        if (respuesta.status === 401) {
            window.location.href = "/Cuenta/IniciarSesion";
            return;
        }

        const datos = await respuesta.json();
        if (pedido !== numeroPedido) return;

        paginaActual = datos.pagina;
        mostrarAvisoUbicacion(datos.desdeBarrio);
        mostrarResultados(datos.resultados);
        mostrarSinResultados(datos);
        mostrarCuenta(datos.total);
        mostrarPaginacion(datos.pagina, datos.totalPaginas);
        mostrarRecientes(datos.recientes);
        if (pedirPins && datos.pines) mostrarPins(datos.pines, !zonaActiva);
        // Arranca elegido el primero de la página, como en el Figma
        if (datos.resultados.length > 0) elegirPin(datos.resultados[0].id);
    } catch (error) {
        // Si falla la conexión, queda lo que se estaba viendo
    }

    if (pedido === numeroPedido) listaResultados.removeAttribute("aria-busy");
}

// "12 lugares en esta zona" (con zona) o "12 lugares" (en toda la Ciudad)
function mostrarCuenta(total) {
    const texto = total + (total === 1 ? " lugar" : " lugares") + (zonaActiva ? " en esta zona" : "");
    resultadosCuenta.textContent = texto;
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
        // En una zona sin lugares conviene mover o alejar el mapa; si no, el texto de siempre
        sinResultados.textContent = zonaActiva ? "No encontramos lugares en esta zona. Mové o alejá el mapa." : textoSinResultados;
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

// ¿Está en la Ciudad de Buenos Aires? Si no (o la ubicación es rara), el mapa arranca en el barrio del perfil.
function estaEnLaCiudad(lat, lon) {
    return lat > -34.72 && lat < -34.52 && lon > -58.54 && lon < -58.33;
}

// Pone el punto "Estás acá" y centra el mapa ahí con zoom de cuadra (17)
function mostrarUbicacion() {
    if (puntoUbicacion) puntoUbicacion.remove();
    puntoUbicacion = L.marker([latitud, longitud], { icon: iconoUbicacion, pane: "ubicacion", interactive: false, keyboard: false }).addTo(mapa);
    moverMapa(function () { mapa.setView([latitud, longitud], 17, { animate: false }); });
}

// Al entrar se pide la ubicación y después se hace la primera búsqueda, ya en la zona de la persona.
// Si la bloquea, falla, está fuera de la Ciudad o el navegador no la tiene (o la página no es HTTPS), el mapa queda en el barrio del perfil.
// Si no contesta el permiso en 5 segundos, se busca igual; si la da más tarde, se vuelve a buscar con ella.
// Si se llegó con un texto en la ruta (/Restaurantes/Descubrir?q=sushi), se busca en toda la Ciudad y el mapa se ajusta a los resultados.
function pedirUbicacionYBuscar() {
    zonaActiva = textoActual === "";

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
            if (estaEnLaCiudad(posicion.coords.latitude, posicion.coords.longitude)) {
                latitud = posicion.coords.latitude;
                longitud = posicion.coords.longitude;
                if (zonaActiva || yaBusco) mostrarUbicacion();
                else puntoUbicacion = L.marker([latitud, longitud], { icon: iconoUbicacion, pane: "ubicacion", interactive: false, keyboard: false }).addTo(mapa);
                buscar(1);
            } else if (!yaBusco) {
                buscar(1);
            }
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

// Un pin por cada lugar de la búsqueda, de todas las páginas (los que no tienen coordenadas no van al mapa).
// ajustar: al buscar en toda la Ciudad, el mapa se acerca para que entren todos; al mover el mapa no se toca.
function mostrarPins(resultados, ajustar) {
    pins.forEach(function (pin) { pin.marcador.remove(); });
    pins = [];
    pinElegido = null;

    resultados.forEach(function (lugar) {
        if (lugar.latitud === null || lugar.longitud === null) return;

        const marcador = L.marker([lugar.latitud, lugar.longitud], { icon: pinNormal, title: lugar.nombre, keyboard: true }).addTo(mapa);
        marcador.on("click", function () { elegirPin(lugar.id); });
        pins.push({ id: lugar.id, lugar: lugar, marcador: marcador });
    });

    if (!ajustar || pins.length === 0) return;

    const limites = L.latLngBounds(pins.map(function (pin) { return pin.marcador.getLatLng(); }));
    moverMapa(function () { mapa.fitBounds(limites, { padding: [80, 80], maxZoom: 15, animate: false }); });
}

// Al mover el mapa o hacer zoom se vuelve a pedir la lista y los pins de esa zona (con una pausa corta para no pedir de más)
let esperaMovimiento = null;
mapa.on("moveend", function () {
    if (movimientoDeCodigo) return;
    clearTimeout(esperaMovimiento);
    esperaMovimiento = setTimeout(function () {
        zonaActiva = true;
        buscar(1);
    }, 400);
});

// El pin elegido se ve naranja y más grande, con su tarjetita arriba
function elegirPin(idRestaurante) {
    const pin = pins.find(function (p) { return p.id === idRestaurante; });
    if (!pin || pin === pinElegido) return;

    if (pinElegido) {
        pinElegido.marcador.setIcon(pinNormal);
        pinElegido.marcador.setZIndexOffset(0);
        pinElegido.marcador.unbindTooltip();
    }

    pin.marcador.setIcon(pinElegidoIcono);
    pin.marcador.setZIndexOffset(1000);
    pin.marcador.bindTooltip(crearTarjetaMapa(pin.lugar), {
        permanent: true,
        direction: "top",
        offset: [0, -58],
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

// ---------- Categorías ----------

// Los chips de categoría (Parrilla, Pizzería...): muestran solo ese tipo de lugar, en toda la Ciudad
const categoriasLista = document.getElementById("categoriasLista");
categoriasLista.addEventListener("click", function (evento) {
    const chip = evento.target.closest(".chip-categoria-filtro");
    if (!chip) return;

    categoriaActual = chip.dataset.id;
    categoriasLista.querySelectorAll(".chip-categoria-filtro").forEach(function (otro) {
        const activo = otro === chip;
        otro.classList.toggle("chip-activo", activo);
        otro.classList.toggle("chip-crema", !activo);
        otro.setAttribute("aria-pressed", activo ? "true" : "false");
    });
    zonaActiva = false;
    buscar(1);
});

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

// Al entrar se busca lo que vino en la ruta (o todo, si vino vacío), con la ubicación si la da
pedirUbicacionYBuscar();
