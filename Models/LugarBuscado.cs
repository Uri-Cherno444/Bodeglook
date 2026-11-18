namespace Bodeglook.Models;

// No es una tabla: un resultado del buscador de lugares del modal Crear plan (07c).
public class LugarBuscado
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    // Siempre tiene una ruta: la portada del restaurante o la imagen por defecto (ver Fotos)
    private string? _foto;
    public string? Foto
    {
        get { return Fotos.Portada(Id, _foto); }
        set { _foto = value; }
    }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    // Cumple todas las restricciones de los integrantes del grupo
    public bool AptoParaTodos { get; set; }
    // Las restricciones que cumple el lugar: "Sin TACC", "Vegetariano"
    public List<string> Restricciones { get; set; } = new List<string>();

    public string BarrioCorto()
    {
        return (Barrio ?? "").Split(',')[0];
    }

    // Lo que queda escrito en el campo al elegirlo: "Bodega Los Alamos · Cañuelas"
    public string TextoCampo()
    {
        return BarrioCorto() == "" ? Nombre ?? "" : Nombre + " · " + BarrioCorto();
    }

    // Cuántos lugares muestra el buscador del modal Crear plan
    private const int CantidadEnBuscador = 6;

    // Buscador de cada campo de lugar del modal Crear plan. Devuelve las restricciones del grupo
    // (para "Aptos para todo el grupo") y hasta 6 lugares: primero los aptos para todos (los que cumplen
    // todas las restricciones de los integrantes) y después el resto, cada grupo por nombre.
    public static object BuscarParaGrupo(int idGrupo, string? texto)
    {
        texto = (texto ?? "").Trim();
        List<string> restriccionesGrupo = BD.ObtenerRestriccionesDeGrupo(idGrupo);
        List<LugarBuscado> encontrados = texto == "" ? new List<LugarBuscado>() : BD.BuscarLugaresParaGrupo(idGrupo, texto);

        // Los Id de las restricciones del grupo y qué restricciones cumple cada lugar
        List<int> idsGrupo = BD.ObtenerRestricciones().Where(r => restriccionesGrupo.Contains(r.Nombre!)).Select(r => r.Id).ToList();
        List<RestriccionesRestaurantes> deLosLugares = BD.ObtenerRestriccionesDeTodosLosRestaurantes();
        foreach (LugarBuscado lugar in encontrados)
        {
            List<int> delLugar = deLosLugares.Where(r => r.IdRestaurante == lugar.Id).Select(r => r.IdRestriccion ?? 0).ToList();
            lugar.AptoParaTodos = idsGrupo.All(id => delLugar.Contains(id));
        }

        // Vienen por nombre: primero los aptos (sin cambiar el orden de cada grupo) y se toman los primeros 6
        List<LugarBuscado> lugares = encontrados.Where(l => l.AptoParaTodos)
            .Concat(encontrados.Where(l => !l.AptoParaTodos))
            .Take(CantidadEnBuscador)
            .ToList();
        foreach (LugarBuscado lugar in lugares)
        {
            lugar.Restricciones = BD.ObtenerRestriccionesDeRestaurante(lugar.Id);
        }

        return new
        {
            restriccionesGrupo,
            lugares = lugares.Select(l => new
            {
                id = l.Id,
                nombre = l.Nombre,
                foto = l.Foto,
                detalle = l.Categoria == null ? l.BarrioCorto() : l.Categoria + " · " + l.BarrioCorto(),
                textoCampo = l.TextoCampo(),
                apto = l.AptoParaTodos,
                restricciones = l.Restricciones
            })
        };
    }
}
