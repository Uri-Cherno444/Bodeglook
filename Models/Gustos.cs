namespace Bodeglook.Models;

public static class Gustos
{
    public static readonly string[] Frecuencias = { "Varias veces por semana", "Una vez por semana", "Cada tanto" };
    public static readonly string[] Companias = { "Amigos", "Pareja", "Familia", "Compañeros de trabajo" };

    public static DatosGustos Limpiar(DatosGustos datos)
    {
        List<int> idsRestricciones = BD.ObtenerIdsRestricciones();
        List<int> idsBarrios = BD.ObtenerIdsBarrios();

        DatosGustos limpios = new DatosGustos();
        limpios.Restricciones = datos.Restricciones.Where(id => idsRestricciones.Contains(id)).Distinct().ToList();
        limpios.Zonas = datos.Zonas.Where(id => idsBarrios.Contains(id)).Distinct().ToList();
        limpios.Frecuencia = datos.Frecuencia != null && Frecuencias.Contains(datos.Frecuencia) ? datos.Frecuencia : null;
        limpios.Companias = datos.Companias.Where(c => Companias.Contains(c)).Distinct().ToList();
        return limpios;
    }

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
