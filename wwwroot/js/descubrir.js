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

const textoSinResultados = sinResultados.textContent;

let textoActual = textoBusqueda.value.trim();
let paginaActual = 1;
let categoriaActual = "";
let zonaActiva = true;
let numeroPedido = 0;

let latitud = null;
let longitud = null;

const elementoMapa = document.getElementById("mapa");
const centroLatitud = Number(elementoMapa.dataset.latitud);
const centroLongitud = Number(elementoMapa.dataset.longitud);
const centroEsBarrio = elementoMapa.dataset.esBarrio === "true";

const mapa = L.map("mapa", { scrollWheelZoom: true, minZoom: 10 })
    .setView([centroLatitud, centroLongitud], centroEsBarrio ? 15 : 12);

L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(mapa);
mapa.createPane("ubicacion").style.zIndex = 150;
mapa.getPane("tilePane").style.mixBlendMode = "multiply";

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

const iconoUbicacion = L.divIcon({
    className: "",
    html: '<span class="mi-ubicacion"><span class="mi-ubicacion-onda"></span><span class="mi-ubicacion-punto"></span></span>',
    iconSize: [44, 44],
    iconAnchor: [22, 22]
});
let puntoUbicacion = null;

let movimientoDeCodigo = false;

function moverMapa(accion) {
    movimientoDeCodigo = true;
    accion();
    movimientoDeCodigo = false;
}

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

formBusqueda.addEventListener("submit", function (evento) {
    evento.preventDefault();
    textoActual = textoBusqueda.value.trim();
    zonaActiva = false;
    buscar(1);
});

chipsRecientes.addEventListener("click", function (evento) {
    const chip = evento.target.closest(".chip-reciente");
    if (!chip) return;
    textoBusqueda.value = chip.textContent;
    textoActual = chip.textContent;
    zonaActiva = false;
    buscar(1);
});

selectorBarrio.addEventListener("change", function () {
    cambiarBarrio(selectorBarrio.value);
});

selectorOrden.addEventListener("change", function () {
    buscar(1);
});

function cambiarBarrio(idBarrio) {
    selectorBarrio.value = idBarrio;
    zonaActiva = false;
    buscar(1);
}

async function buscar(pagina) {
    const pedido = ++numeroPedido;
    const pedirPins = pagina === 1;
    listaResultados.setAttribute("aria-busy", "true");

    let direccion = "/Restaurantes/Resultados?texto=" + encodeURIComponent(textoActual) + "&pagina=" + pagina + "&orden=" + selectorOrden.value;
    if (selectorBarrio.value !== "") direccion += "&idBarrio=" + selectorBarrio.value;
    if (categoriaActual !== "") direccion += "&idCategoria=" + categoriaActual;
    if (latitud !== null && longitud !== null) direccion += "&latitud=" + latitud + "&longitud=" + longitud;
    if (pedirPins) direccion += "&pines=true";

    if (zonaActiva) {
        const limites = mapa.getBounds();
        direccion += "&norte=" + limites.getNorth() + "&sur=" + limites.getSouth() +
            "&este=" + limites.getEast() + "&oeste=" + limites.getWest() + "&zoom=" + mapa.getZoom();
    }

    try {
        const respuesta = await fetch(direccion);

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
        if (datos.resultados.length > 0) elegirPin(datos.resultados[0].id);
    } catch (error) {
    }

    if (pedido === numeroPedido) listaResultados.removeAttribute("aria-busy");
}

function mostrarCuenta(total) {
    const texto = total + (total === 1 ? " lugar" : " lugares") + (zonaActiva ? " en esta zona" : "");
    resultadosCuenta.textContent = texto;
}

function mostrarResultados(resultados) {
    const plantilla = document.getElementById("plantillaResultado");
    listaResultados.innerHTML = "";
    document.getElementById("creditosResultados").hidden = !resultados.some(function (lugar) { return lugar.origen === "Geoapify"; });

    resultados.forEach(function (lugar) {
        const tarjeta = plantilla.content.firstElementChild.cloneNode(true);
        tarjeta.dataset.idRestaurante = lugar.id;
        if (lugar.destacado) tarjeta.classList.add("resultado-pro");
        else tarjeta.querySelector(".chip-recomendado").remove();

        const foto = tarjeta.querySelector(".resultado-foto");
        if (lugar.foto) foto.src = lugar.foto;
        else foto.removeAttribute("src");

        const nombre = tarjeta.querySelector(".resultado-nombre");
        nombre.textContent = lugar.nombre;
        nombre.href = "/Restaurantes/Detalle/" + lugar.id;

        pintarCategoria(tarjeta.querySelector(".resultado-categoria"), lugar);
        tarjeta.querySelector(".resultado-barrio").textContent = textoBarrio(lugar);
        pintarCorazon(tarjeta.querySelector(".boton-favorito"), lugar.esFavorito);

        tarjeta.addEventListener("mouseenter", function () {
            elegirPin(lugar.id);
        });

        listaResultados.appendChild(tarjeta);
    });
}

function mostrarSinResultados(datos) {
    sinResultados.hidden = datos.resultados.length > 0;
    if (!datos.otroBarrio) {
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

function textoBarrio(lugar) {
    if (lugar.distanciaKm === null) return lugar.barrio;
    return lugar.barrio + " · " + textoDistancia(lugar.distanciaKm);
}

function textoDistancia(km) {
    if (km < 0.05) return "A menos de 1 cuadra";
    if (km < 0.15) return "A 1 cuadra";
    const cuadras = Math.round(km * 10);
    if (cuadras <= 20) return "A unas " + cuadras + " cuadras";
    return "A " + (Math.round(km * 10) / 10).toFixed(1).replace(".", ",") + " km";
}

function mostrarAvisoUbicacion(desdeBarrio) {
    if (desdeBarrio) {
        avisoUbicacion.textContent = "Distancias aproximadas desde tu barrio (" + desdeBarrio + "). Activá la ubicación para verlas desde donde estás.";
        avisoUbicacion.hidden = false;
    } else {
        avisoUbicacion.hidden = true;
    }
}

function estaEnLaCiudad(lat, lon) {
    return lat > -34.72 && lat < -34.52 && lon > -58.54 && lon < -58.33;
}

function mostrarUbicacion() {
    if (puntoUbicacion) puntoUbicacion.remove();
    puntoUbicacion = L.marker([latitud, longitud], { icon: iconoUbicacion, pane: "ubicacion", interactive: false, keyboard: false }).addTo(mapa);
    moverMapa(function () { mapa.setView([latitud, longitud], 17, { animate: false }); });
}

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

function pintarCategoria(elemento, lugar) {
    elemento.classList.add("estrellas-en-linea");
    if (lugar.puntaje === null) {
        elemento.textContent = lugar.categoria + " · Sin reseñas todavía";
        return;
    }
    elemento.textContent = lugar.categoria + " · ";
    elemento.appendChild(crearEstrellas(Number(lugar.puntaje), 14));
}

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

let esperaMovimiento = null;
mapa.on("moveend", function () {
    if (movimientoDeCodigo) return;
    clearTimeout(esperaMovimiento);
    esperaMovimiento = setTimeout(function () {
        zonaActiva = true;
        buscar(1);
    }, 400);
});

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
    pintarCategoria(tarjeta.querySelector(".mapa-tarjeta-detalle"), lugar);
    return tarjeta;
}

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
    }

    boton.disabled = false;
});

function pintarCorazon(boton, esFavorito) {
    boton.classList.toggle("es-favorito", esFavorito);
    boton.setAttribute("aria-pressed", esFavorito ? "true" : "false");
    boton.setAttribute("aria-label", esFavorito ? "Sacar de favoritos" : "Agregar a favoritos");
}

pedirUbicacionYBuscar();
