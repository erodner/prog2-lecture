using Geometrieeditor.Datenhaltung;
using Geometrieeditor.Fachkonzept;
using Geometrieeditor.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Hier wird entschieden, welche Datenhaltung hinter dem Fachkonzept steckt.
// "Scoped" heißt in Blazor: ein Objekt pro Browser-Verbindung (Circuit).
builder.Services.AddScoped<IFigurSpeicher>(_ => new JsonFigurSpeicher("figuren.json"));
builder.Services.AddScoped<FigurenVerwaltung>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
