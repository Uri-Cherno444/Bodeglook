if (new URLSearchParams(window.location.search).get("restaurante") === "1") {
    mostrarAviso("Registramos tu restaurante.");
}

prepararFormulario("formIniciarSesion", "/Cuenta/Ingresar", function () {
    return {
        nombreUsuario: document.getElementById("usuario").value,
        contraseña: document.getElementById("contrasena").value
    };
});
