namespace Bodeglook.Models;

public class LugarBuscado
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    private string? _foto;
    public string? Foto
    {
        get { return Fotos.Portada(Id, _foto); }
        set { _foto = value; }
    }
    public string? Categoria { get; set; }
    public string? Barrio { get; set; }
    public bool AptoParaTodos { get; set; }
    public List<string> Restricciones { get; set; } = new List<string>();

    public string BarrioCorto()
    {
        return (Barrio ?? "").Split(',')[0];
    }

    public string TextoCampo()
    {
        return BarrioCorto() == "" ? Nombre ?? "" : Nombre + " · " + BarrioCorto();
    }

    private const int CantidadEnBuscador = 6;

    // excluidos: los lugares ya elegidos en las otras opciones del plan, que no se vuelven a ofrecer
    public static object BuscarParaGrupo(int idGrupo, string? texto, List<int> excluidos)
    {
        texto = (texto ?? "").Trim();
        List<string> restriccionesGrupo = BD.ObtenerRestriccionesDeGrupo(idGrupo);
        List<LugarBuscado> coincidencias = texto == "" ? new List<LugarBuscado>() : BD.BuscarLugaresParaGrupo(idGrupo, texto);
        List<LugarBuscado> encontrados = coincidencias.Where(l => !excluidos.Contains(l.Id)).ToList();

        List<int> idsGrupo = BD.ObtenerIdsRestriccionesPorNombre(restriccionesGrupo);
        ILookup<int, int> deLosLugares = BD.ObtenerRestriccionesPorRestaurante();
        foreach (LugarBuscado lugar in encontrados)
        {
            List<int> delLugar = deLosLugares[lugar.Id].ToList();
            lugar.AptoParaTodos = idsGrupo.All(id => delLugar.Contains(id));
        }

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
            yaElegidos = coincidencias.Count > 0 && encontrados.Count == 0,
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
