// 11 · Menú del avatar y 12 · Cerrar sesión (solo existen con sesión iniciada)

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

    // Al tocar fuera del avatar y del menú, se cierra
    document.addEventListener("click", function (evento) {
        if (!menu.hidden && !menu.contains(evento.target) && !botonAvatar.contains(evento.target)) {
            cerrarMenu();
        }
    });

    // Con Escape también se cierra (el diálogo ya se cierra solo con Escape)
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
