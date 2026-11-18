// 06 · Registro Restaurante. Usa prepararFormulario de cuenta.js.

// La tarjeta de plan elegida va con borde y fondo naranja suave
document.querySelectorAll(".plan-opcion input").forEach(function (radio) {
    radio.addEventListener("change", function () {
        document.querySelectorAll(".plan-opcion").forEach(function (opcion) {
            opcion.classList.toggle("plan-opcion-elegida", opcion.querySelector("input").checked);
        });
    });
});

// Volver a la pantalla anterior (si se llegó desde otra página de Bodeglook)
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
