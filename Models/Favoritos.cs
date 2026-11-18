namespace Bodeglook.Models;

public class Favoritos
{
    public int Id { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdRestaurante { get; set; }

    // El corazón de cada tarjeta ({ idRestaurante }): si era favorito lo saca, si no lo agrega.
    // Devuelve { ok, esFavorito }, o { ok = false } si el lugar no existe.
    public object Alternar(int idUsuario)
    {
        if (IdRestaurante == null || !BD.ExisteRestaurante(IdRestaurante.Value)) return new { ok = false };

        bool esFavorito = BD.AlternarFavorito(idUsuario, IdRestaurante.Value);
        return new { ok = true, esFavorito };
    }
}
