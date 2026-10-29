// 08 · Mis Planes: las pestañas piden los planes con fetch y se arman las tarjetas sin recargar.

const grillaPlanes = document.getElementById("grillaPlanes");
const sinPlanes = document.getElementById("sinPlanes");
const pestanas = document.querySelectorAll(".pestana");

async function cargarPlanes(filtro) {
    const respuesta = await fetch("/Planes/ListaPlanes?filtro=" + filtro);
    const planes = await respuesta.json();

    grillaPlanes.innerHTML = "";
    planes.forEach(function (plan) {
        grillaPlanes.appendChild(crearTarjetaPlan(plan));
    });

    // "Todavía no tenés planes" solo si no hay ninguno en "Todos"
    sinPlanes.hidden = !(planes.length === 0 && filtro === "todos");
}

function crearTarjetaPlan(plan) {
    const confirmado = plan.estado === "Confirmado";

    const tarjeta = document.createElement("a");
    tarjeta.className = "tarjeta-plan " + (confirmado ? "tarjeta-plan-confirmado" : "tarjeta-plan-votacion");
    // Confirmado: al Detalle del lugar elegido. En votación todavía no hay lugar: a la votación.
    tarjeta.href = confirmado ? "/Restaurantes/Detalle/" + plan.idRestauranteElegido : "/Planes/Detalle/" + plan.id;

    const detalle = confirmado
        ? "Confirmado en " + plan.lugarElegido
        : plan.cantidadLugares + (plan.cantidadLugares === 1 ? " lugar propuesto" : " lugares propuestos");

    tarjeta.innerHTML =
        '<span class="tarjeta-plan-franja"></span>' +
        '<span class="tarjeta-plan-contenido">' +
        '  <span class="tarjeta-plan-titulo"><span class="tarjeta-plan-nombre"></span><span class="chip chip-estado"></span></span>' +
        '  <span class="tarjeta-plan-pie"><span class="tarjeta-plan-detalle"></span><span class="avatares"></span></span>' +
        '</span>';

    // textContent para que un nombre con caracteres raros no se interprete como HTML
    tarjeta.querySelector(".tarjeta-plan-nombre").textContent = plan.titulo;
    tarjeta.querySelector(".chip-estado").textContent = plan.estado;
    tarjeta.querySelector(".chip-estado").classList.add(confirmado ? "chip-confirmado" : "chip-naranja");
    tarjeta.querySelector(".tarjeta-plan-detalle").textContent = detalle;

    const avatares = tarjeta.querySelector(".avatares");
    plan.iniciales.forEach(function (inicial) {
        const avatar = document.createElement("span");
        avatar.className = "avatar avatar-mini";
        avatar.textContent = inicial;
        avatares.appendChild(avatar);
    });

    return tarjeta;
}

pestanas.forEach(function (pestana) {
    pestana.addEventListener("click", function () {
        pestanas.forEach(function (otra) {
            otra.classList.remove("activa");
            otra.setAttribute("aria-selected", "false");
        });
        pestana.classList.add("activa");
        pestana.setAttribute("aria-selected", "true");
        cargarPlanes(pestana.dataset.filtro);
    });
});

cargarPlanes("todos");
