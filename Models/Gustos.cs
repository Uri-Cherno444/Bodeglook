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
}
