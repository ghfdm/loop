using Loop.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache(options => options.SizeLimit = 500);
builder.Services.AddHttpClient("Nominatim", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Nominatim:BaseUrl"]!);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(builder.Configuration["Nominatim:UserAgent"]!);
    client.Timeout = TimeSpan.FromSeconds(15);
});
// Uma única instância compartilha o controle de consultas entre todos os usuários.
builder.Services.AddSingleton<NominatimService>();
builder.Services.AddSingleton<VagasService>();
builder.Services.AddSingleton<ReservasService>();

var app = builder.Build();
app.MapControllers();
app.Run();
