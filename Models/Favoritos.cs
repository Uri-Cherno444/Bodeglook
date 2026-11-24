namespace Bodeglook.Models;

public class Favoritos
{
    public int Id { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdRestaurante { get; set; }

    public object Alternar(int idUsuario)
    {
        if (IdRestaurante == null || !BD.ExisteRestaurante(IdRestaurante.Value)) return new { ok = false };

        bool esFavorito = BD.AlternarFavorito(idUsuario, IdRestaurante.Value);
        return new { ok = true, esFavorito };
    }
}
