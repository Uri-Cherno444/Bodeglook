using Bodeglook.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

BD.CadenaConexion = builder.Configuration.GetConnectionString("Bodeglook")!;

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
