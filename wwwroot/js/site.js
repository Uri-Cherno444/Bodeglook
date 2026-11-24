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
