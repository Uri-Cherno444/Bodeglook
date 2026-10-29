// 05 · Descubrir: busca con fetch a /Restaurantes/Resultados y arma la lista, la paginación
// y los pins del mapa sin recargar. El mapa es Leaflet con OpenStreetMap.

const formBusqueda = document.getElementById("formBusqueda");
const textoBusqueda = document.getElementById("textoBusqueda");
const listaResultados = document.getElementById("listaResultados");
const sinResultados = document.getElementById("sinResultados");
const paginacion = document.getElementById("paginacion");
const busquedasRecientes = document.getElementById("busquedasRecientes");
const chipsRecientes = document.getElementById("chipsRecientes");

// Lo que se buscó por última vez (las páginas siguientes usan el mismo texto)
let textoActual = textoBusqueda.value.trim();

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

async function buscar(pagina) {
    listaResultados.setAttribute("aria-busy", "true");

    try {
        const respuesta = await fetch("/Restaurantes/Resultados?texto=" + encodeURIComponent(textoActual) + "&pagina=" + pagina);

        // Si se venció la sesión, al login
        if (respuesta.status === 401) {
            window.location.href = "/Cuenta/IniciarSesion";
            return;
        }

        const datos = await respuesta.json();
        mostrarResultados(datos.resultados);
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
    sinResultados.hidden = resultados.length > 0;

    resultados.forEach(function (lugar) {
        const tarjeta = plantilla.content.firstElementChild.cloneNode(true);
        tarjeta.dataset.idRestaurante = lugar.id;
        if (lugar.esPro) tarjeta.classList.add("resultado-pro");
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

// "Palermo · a 2,3 km" (en el mismo barrio del usuario: "Palermo · En tu barrio")
function textoBarrio(lugar) {
    if (lugar.distanciaKm === null) return lugar.barrio;
    if (lugar.distanciaKm === 0) return lugar.barrio + " · En tu barrio";
    return lugar.barrio + " · a " + String(lugar.distanciaKm).replace(".", ",") + " km";
}

// "Vinoteca · ★ 4.7" (sin reseñas, solo la categoría)
function textoCategoria(lugar) {
    if (lugar.puntaje === null) return lugar.categoria;
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

// Al entrar se busca lo que vino en la ruta (o todo, si vino vacío)
buscar(1);
