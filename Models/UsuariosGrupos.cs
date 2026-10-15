namespace Bodeglook.Models;

public class UsuariosGrupos
{
    public int Id { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdGrupo { get; set; }
    public DateTime? UltimaLectura { get; set; }
}
