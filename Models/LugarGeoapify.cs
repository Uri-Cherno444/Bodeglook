using System.Text.RegularExpressions;

namespace Bodeglook.Models;

// No es una tabla: un lugar tal como lo devuelve Geoapify (datos de OpenStreetMap), listo para guardar en Restaurantes.
public class LugarGeoapify
{
    // place_id de Geoapify: se guarda en Restaurantes.IdGeoapify
    public string? IdGeoapify { get; set; }
    public string? Nombre { get; set; }
    public string? Calle { get; set; }
    public string? Numero { get; set; }
    public string? CodigoPostal { get; set; }
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    // Códigos de categoría de Geoapify: "catering.restaurant.pizza", "catering.bar", etc.
    public List<string> Categorias { get; set; } = new List<string>();

    // "Honduras 5000" -> 5000. Si no hay número (o es "S/N") queda 0.
    public int NumeroEntero()
    {
        return PrimerNumero(Numero);
    }

    // "C1414DDH" -> 1414. La columna es un entero, así que se guardan solo los dígitos.
    public int CodigoPostalEntero()
    {
        return PrimerNumero(CodigoPostal);
    }

    private static int PrimerNumero(string? texto)
    {
        Match digitos = Regex.Match(texto ?? "", @"\d+");
        if (digitos.Success && int.TryParse(digitos.Value, out int numero)) return numero;
        return 0;
    }

    // Pasa los códigos de Geoapify a una de NUESTRAS categorías (Categorias.Nombre).
    // Si el lugar trae varios códigos gana el más específico, en este orden.
    // Devuelve null si no corresponde a ninguna (ese lugar no se guarda).
    public string? NombreCategoria()
    {
        // Bodegas y vinotecas: "drinks" mezcla comercios de todo tipo, así que ahí se pide además que el nombre hable de vino
        if (Categorias.Contains("production.winery")) return "Bodega";
        if (Categorias.Contains("commercial.food_and_drink.drinks") &&
            Regex.IsMatch(Nombre ?? "", @"vino|vinoteca|vinicola|vinería|wine|bodega|cava|sommelier", RegexOptions.IgnoreCase))
            return "Bodega";

        if (Tiene("catering.restaurant.steak_house", "catering.restaurant.barbecue", "catering.restaurant.argentinian")) return "Parrilla";
        if (Tiene("catering.restaurant.pizza", "catering.fast_food.pizza")) return "Pizzería";
        if (Tiene("catering.restaurant.italian")) return "Pastas";
        if (Tiene("catering.restaurant.sushi", "catering.restaurant.japanese")) return "Sushi";
        if (Tiene("catering.restaurant.mexican", "catering.restaurant.tacos", "catering.restaurant.tex-mex", "catering.fast_food.tacos")) return "Mexicana";
        if (Tiene("catering.restaurant.peruvian")) return "Peruana";
        if (Tiene("catering.restaurant.burger", "catering.fast_food.burger")) return "Hamburguesería";
        // Antes que Cafetería, porque catering.cafe.ice_cream también es un café
        if (Tiene("catering.ice_cream", "catering.cafe.ice_cream")) return "Heladería";
        if (Tiene("catering.cafe")) return "Cafetería";
        if (Tiene("catering.restaurant")) return "Restaurante";
        if (Tiene("catering.bar", "catering.pub")) return "Bar";
        return null;
    }

    private bool Tiene(params string[] codigos)
    {
        return codigos.Any(codigo => Categorias.Contains(codigo));
    }
}
