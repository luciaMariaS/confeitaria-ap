using Microsoft.OpenApi;
using Confeitaria.Api.Dados;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var connectionString = builder.Configuration.GetConnectionString("Padrao");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure a variável de ambiente ConnectionStrings__Padrao.");

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Confeitaria API",
        Version = "v1",
        Description = "Projeto escolar — empresa fictícia"
    });

    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Cole só o token, sem a palavra Bearer."
    });

    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
     await Semeador.SemearAsync(db, app.Configuration);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapHealthChecks("/health");
app.MapGet("/api/versao", () => new
{
    commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "local",
    ambiente = app.Environment.EnvironmentName
});

// Configure the HTTP request pipeline.



app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
