using System.Text.RegularExpressions;

namespace Bodeglook.Models;

public class LugarGeoapify
{
    public string? IdGeoapify { get; set; }
    public string? Nombre { get; set; }
    public string? Calle { get; set; }
    public string? Numero { get; set; }
    public string? CodigoPostal { get; set; }
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public List<string> Categorias { get; set; } = new List<string>();

    public int NumeroEntero()
    {
        return PrimerNumero(Numero);
    }

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

    public string? NombreCategoria()
    {
        if (Categorias.Contains("production.winery")) return "Vinoteca";
        if (Categorias.Contains("commercial.food_and_drink.drinks") &&
            Regex.IsMatch(Nombre ?? "", @"vino|vinoteca|vinicola|vinería|wine|bodega|cava|sommelier", RegexOptions.IgnoreCase))
            return "Vinoteca";

        if (Tiene("catering.restaurant.steak_house", "catering.restaurant.barbecue", "catering.restaurant.argentinian")) return "Parrilla";
        if (Tiene("catering.restaurant.pizza", "catering.fast_food.pizza")) return "Pizzería";
        if (Tiene("catering.restaurant.italian")) return "Italiana";
        if (Tiene("catering.restaurant.sushi", "catering.restaurant.japanese")) return "Japonesa";
        if (Tiene("catering.restaurant.mexican", "catering.restaurant.tacos", "catering.restaurant.tex-mex", "catering.fast_food.tacos")) return "Mexicana";
        if (Tiene("catering.restaurant.peruvian")) return "Peruana";
        if (Tiene("catering.restaurant.burger", "catering.fast_food.burger")) return "Hamburguesería";
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
