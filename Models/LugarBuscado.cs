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

    public static object BuscarParaGrupo(int idGrupo, string? texto)
    {
        texto = (texto ?? "").Trim();
        List<string> restriccionesGrupo = BD.ObtenerRestriccionesDeGrupo(idGrupo);
        List<LugarBuscado> encontrados = texto == "" ? new List<LugarBuscado>() : BD.BuscarLugaresParaGrupo(idGrupo, texto);

        List<int> idsGrupo = BD.ObtenerRestricciones().Where(r => restriccionesGrupo.Contains(r.Nombre!)).Select(r => r.Id).ToList();
        List<RestriccionesRestaurantes> deLosLugares = BD.ObtenerRestriccionesDeTodosLosRestaurantes();
        foreach (LugarBuscado lugar in encontrados)
        {
            List<int> delLugar = deLosLugares.Where(r => r.IdRestaurante == lugar.Id).Select(r => r.IdRestriccion ?? 0).ToList();
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
