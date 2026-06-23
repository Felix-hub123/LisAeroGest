using LisAeroGest.Data;
using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Base de Dados ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ─────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Configuração de password
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Confirmação de email obrigatória para login
    options.SignIn.RequireConfirmedEmail = true;

    // Utilizador
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// ── Autenticação JWT + Cookie ─────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiScheme";
})
.AddCookie(options =>
{
    // Redireciona para login quando não autenticado
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/NotAuthorized";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Tokens:Issuer"],
        ValidAudience = builder.Configuration["Tokens:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Tokens:Key"]!))
    };
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── Seed da Base de Dados ─────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao aplicar migrations.");
    }
}

app.Run();
