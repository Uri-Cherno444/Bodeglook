// Formularios de Iniciar sesión y Registro.
// Mandan los datos con fetch en JSON y muestran los errores con los estados del UI Kit:
//   - error de un campo: ícono de atención + tooltip negro al costado del campo
//   - aviso general: snackbar amarillo arriba del formulario

function prepararFormulario(idFormulario, url, obtenerDatos) {
    const formulario = document.getElementById(idFormulario);
    const boton = formulario.querySelector("button[type=submit]");

    // Al escribir en un campo con error, se le saca el error
    formulario.querySelectorAll("input").forEach(function (input) {
        input.addEventListener("input", function () {
            limpiarErrorCampo(input.closest(".campo"));
        });
    });

    formulario.querySelector(".aviso-cerrar").addEventListener("click", ocultarAviso);

    formulario.addEventListener("submit", async function (evento) {
        evento.preventDefault();
        limpiarErrores(formulario);
        boton.disabled = true;
        boton.classList.add("cargando");

        try {
            const respuesta = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(obtenerDatos())
            });
            const resultado = await respuesta.json();

            if (resultado.ok) {
                window.location.href = "/Home/Inicio";
                return;
            }

            if (resultado.campo === "aviso") {
                mostrarAviso(resultado.mensaje);
            } else {
                mostrarErrorCampo(formulario.querySelector('[data-campo="' + resultado.campo + '"]'), resultado.mensaje);
            }
        } catch (error) {
            mostrarAviso("No pudimos conectarnos. Probá de nuevo en un rato.");
        }

        boton.disabled = false;
        boton.classList.remove("cargando");
    });
}

function mostrarErrorCampo(campo, mensaje) {
    campo.classList.add("campo-con-error");

    const entrada = campo.querySelector(".campo-entrada");
    const error = document.createElement("span");
    error.className = "campo-error";
    error.innerHTML = '<span class="tooltip"></span><img src="/img/icono-error.svg" width="24" height="24" alt="Error" />';
    error.querySelector(".tooltip").textContent = mensaje;
    entrada.appendChild(error);

    campo.querySelector("input").focus();
}

function limpiarErrorCampo(campo) {
    campo.classList.remove("campo-con-error");
    const error = campo.querySelector(".campo-error");
    if (error) error.remove();
}

function limpiarErrores(formulario) {
    formulario.querySelectorAll(".campo").forEach(limpiarErrorCampo);
    ocultarAviso();
}

function mostrarAviso(mensaje) {
    const aviso = document.getElementById("aviso");
    document.getElementById("avisoTexto").textContent = mensaje;
    aviso.hidden = false;
    // Se vuelve a poner la clase para que la animación arranque de nuevo
    aviso.classList.remove("aviso-visible");
    void aviso.offsetWidth;
    aviso.classList.add("aviso-visible");
}

function ocultarAviso() {
    document.getElementById("aviso").hidden = true;
}
