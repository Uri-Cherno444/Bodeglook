document.querySelectorAll(".plan-opcion input").forEach(function (radio) {
    radio.addEventListener("change", function () {
        document.querySelectorAll(".plan-opcion").forEach(function (opcion) {
            opcion.classList.toggle("plan-opcion-elegida", opcion.querySelector("input").checked);
        });
    });
});

// El texto de ejemplo del desplegable se ve en gris hasta que se elige algo
document.querySelectorAll("#barrio, #categoria").forEach(function (desplegable) {
    desplegable.addEventListener("change", function () {
        desplegable.classList.toggle("sin-elegir", desplegable.value === "");
    });
});

document.getElementById("botonVolver").addEventListener("click", function (evento) {
    if (document.referrer.startsWith(window.location.origin) && history.length > 1) {
        evento.preventDefault();
        history.back();
    }
});

prepararFormulario("formRestaurante", "/Restaurantes/RegistrarRestaurante", function () {
    const barrio = document.getElementById("barrio").value;
    const categoria = document.getElementById("categoria").value;
    return {
        nombre: document.getElementById("nombre").value,
        email: document.getElementById("email").value,
        contraseña: document.getElementById("contrasena").value,
        idBarrio: barrio === "" ? null : Number(barrio),
        idCategoria: categoria === "" ? null : Number(categoria),
        plan: document.querySelector('input[name="plan"]:checked').value
    };
});
