namespace Bodeglook.Models;

// No es una tabla: las opciones fijas de "Gustos" (02c · Onboarding y 08c · Editar perfil).
// Las restricciones y las zonas salen de las tablas Restricciones y Barrios.
public static class Gustos
{
    public static readonly string[] Frecuencias = { "Varias veces por semana", "Una vez por semana", "Cada tanto" };
    public static readonly string[] Companias = { "Amigos", "Pareja", "Familia", "Compañeros de trabajo" };

    // Deja solo lo válido de lo que mandó el navegador: Id que existen, opciones de la lista y sin repetidos
    public static DatosGustos Limpiar(DatosGustos datos)
    {
        List<int> idsRestricciones = BD.ObtenerRestricciones().Select(r => r.Id).ToList();
        List<int> idsBarrios = BD.ObtenerBarrios().Select(b => b.Id).ToList();

        DatosGustos limpios = new DatosGustos();
        limpios.Restricciones = datos.Restricciones.Where(id => idsRestricciones.Contains(id)).Distinct().ToList();
        limpios.Zonas = datos.Zonas.Where(id => idsBarrios.Contains(id)).Distinct().ToList();
        limpios.Frecuencia = datos.Frecuencia != null && Frecuencias.Contains(datos.Frecuencia) ? datos.Frecuencia : null;
        limpios.Companias = datos.Companias.Where(c => Companias.Contains(c)).Distinct().ToList();
        return limpios;
    }

    // "Seguir" del paso 1 del onboarding. Guarda solo lo válido y devuelve { ok, destino }:
    // si no marcó restricciones se saltea el paso 2 (no hay nada que filtrar).
    public static object Guardar(int idUsuario, DatosGustos datos)
    {
        DatosGustos gustos = Limpiar(datos);
        BD.GuardarGustos(idUsuario, gustos);

        if (gustos.Restricciones.Count == 0)
        {
            BD.GuardarSoloApto(idUsuario, false);
            return new { ok = true, destino = "/Home/Inicio" };
        }
        return new { ok = true, destino = "/Cuenta/FiltroDietario" };
    }
}
