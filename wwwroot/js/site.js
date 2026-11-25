const botonAvatar = document.getElementById("botonAvatar");

if (botonAvatar) {
    const menu = document.getElementById("menuUsuario");
    const dialogo = document.getElementById("dialogoCerrarSesion");

    function abrirMenu() {
        menu.hidden = false;
        botonAvatar.setAttribute("aria-expanded", "true");
    }

    function cerrarMenu() {
        menu.hidden = true;
        botonAvatar.setAttribute("aria-expanded", "false");
    }

    botonAvatar.addEventListener("click", function () {
        if (menu.hidden) {
            abrirMenu();
        } else {
            cerrarMenu();
        }
    });

    document.addEventListener("click", function (evento) {
        if (!menu.hidden && !menu.contains(evento.target) && !botonAvatar.contains(evento.target)) {
            cerrarMenu();
        }
    });

    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape" && !menu.hidden) {
            cerrarMenu();
            botonAvatar.focus();
        }
    });

    document.getElementById("botonCerrarSesion").addEventListener("click", function () {
        cerrarMenu();
        dialogo.showModal();
    });

    document.getElementById("dialogoCerrar").addEventListener("click", function () {
        dialogo.close();
    });

    document.getElementById("dialogoCancelar").addEventListener("click", function () {
        dialogo.close();
    });
}

// Puntaje de 1 a 5 en estrellas, redondeado a la media estrella más cercana. En el servidor lo hace _Estrellas.cshtml.
function crearEstrellas(puntaje, tamano) {
    const redondeado = Math.round(puntaje * 2) / 2;
    const texto = Math.round(puntaje * 10) / 10 + " de 5 estrellas";

    const estrellas = document.createElement("span");
    estrellas.className = "estrellas";
    estrellas.setAttribute("role", "img");
    estrellas.setAttribute("aria-label", texto);
    estrellas.title = texto;

    for (let estrella = 1; estrella <= 5; estrella++) {
        const tipo = redondeado >= estrella ? "llena" : redondeado >= estrella - 0.5 ? "media" : "vacia";
        const imagen = document.createElement("img");
        imagen.src = "/img/icono-estrella-" + tipo + ".svg";
        imagen.width = tamano;
        imagen.height = tamano;
        imagen.alt = "";
        estrellas.appendChild(imagen);
    }
    return estrellas;
}
