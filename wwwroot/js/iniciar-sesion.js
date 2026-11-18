// 02 · Iniciar sesión. Usa prepararFormulario y mostrarAviso de cuenta.js.

// Si viene de registrar un restaurante se avisa que se guardó
if (new URLSearchParams(window.location.search).get("restaurante") === "1") {
    mostrarAviso("Registramos tu restaurante.");
}

prepararFormulario("formIniciarSesion", "/Cuenta/Ingresar", function () {
    return {
        nombreUsuario: document.getElementById("usuario").value,
        contraseña: document.getElementById("contrasena").value
    };
});
