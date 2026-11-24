prepararFormulario("formRegistro", "/Cuenta/Registrar", function () {
    return {
        nombreCompleto: document.getElementById("nombre").value,
        nombreUsuario: document.getElementById("usuario").value,
        email: document.getElementById("email").value,
        contraseña: document.getElementById("contrasena").value
    };
});
