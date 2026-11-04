using Bodeglook.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

BD.CadenaConexion = builder.Configuration.GetConnectionString("Bodeglook")!;
// La API key de Geoapify está en los user-secrets ("Geoapify:ApiKey"): no va en el código ni en appsettings.json
Geoapify.ApiKey = builder.Configuration["Geoapify:ApiKey"] ?? "";

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// MapStaticAssets solo sirve los archivos que había al compilar; esto sirve las fotos de grupo que se suben
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

// Importación de lugares de Geoapify: solo existe en desarrollo y gasta créditos del plan gratis.
// /Geoapify/Importar?barrios=1,2   (ids de Barrios)   o   ?barrios=todos
if (app.Environment.IsDevelopment())
{
    app.MapGet("/Geoapify/Importar", async (string? barrios) =>
    {
        List<Barrios> existentes = BD.ObtenerBarriosConCoordenadas();
        List<Barrios> elegidos = new List<Barrios>();
        if (barrios == "todos") elegidos = existentes;
        else if (!string.IsNullOrWhiteSpace(barrios))
        {
            List<int> ids = barrios.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(texto => int.TryParse(texto, out int id) ? id : 0).ToList();
            elegidos = existentes.Where(barrio => ids.Contains(barrio.Id)).ToList();
        }
        if (elegidos.Count == 0)
            return Results.Json(new { mensaje = "Indicá los barrios: /Geoapify/Importar?barrios=1,2 o ?barrios=todos", barrios = existentes.Select(b => new { b.Id, b.Nombre }) });

        List<ResultadoImportacion> resultados = await Geoapify.ImportarBarrios(elegidos);
        return Results.Json(new
        {
            total = resultados.Sum(r => r.Nuevos + r.Actualizados),
            porBarrio = resultados
        });
    });
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
