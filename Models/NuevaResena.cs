namespace Bodeglook.Models;

// No es una tabla: lo que manda el modal "Dejar una reseña" (04b).
// Los cuatro puntajes van del 1 al 5. En la tabla Reseñas se guarda solo el promedio redondeado
// (no hay columnas para cada uno ni para marcar los canjes).
public class NuevaResena
{
    public int IdRestaurante { get; set; }
    public int Comida { get; set; }
    public int Ambiente { get; set; }
    public int Atencion { get; set; }
    public int Precio { get; set; }
    public string? Comentario { get; set; }
    public bool EsCanje { get; set; }
}
