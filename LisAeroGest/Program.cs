using LisAeroGest.Data;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using LisAeroGest.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ── Base de Dados (SQL Server em Dev vs PostgreSQL em Prod) ──────────────
if (builder.Environment.IsDevelopment())
{
    // Registo PRINCIPAL
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("LisAeroGest")
                 .MigrationsHistoryTable("__EFMigrationsHistory")
        )
    );

    // Registo EXTRA — só para migrações
    builder.Services.AddDbContext<DataContextSqlServer>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("LisAeroGest")
                 .MigrationsHistoryTable("__EFMigrationsHistory")
        )
    );
}
else
{
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine(">>> CONNECTION STRING NÃO ENCONTRADA");
    }
    else
    {
        var csb = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        Console.WriteLine(">>> DATABASE CONFIGURATION");
        Console.WriteLine($">>> Host: {csb.Host}");
        Console.WriteLine($">>> Port: {csb.Port}");
        Console.WriteLine($">>> Database: {csb.Database}");
        Console.WriteLine($">>> Username: {csb.Username}");
        Console.WriteLine(">>> Password: [OCULTA]");
    }

    // Registo PRINCIPAL — usado pela aplicação inteira (SeedDb, controllers, repositórios, etc.)
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("LisAeroGest")
                 .MigrationsHistoryTable("__EFMigrationsHistory", "public")
        )
    );

    // Registo EXTRA — usado só pelas ferramentas de migração (Add-Migration/Update-Database),
    // para conseguirem filtrar corretamente as migrações do Postgres.
    builder.Services.AddDbContext<DataContextPostgres>(options =>
        options.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("LisAeroGest")
                 .MigrationsHistoryTable("__EFMigrationsHistory", "public")
        )
    );
}
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

    // Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;


    // Utilizador
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// ── Autenticação JWT + Cookie ─────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
options.DefaultScheme = "MultiScheme";
options.DefaultChallengeScheme = "MultiScheme";
})
// "MultiScheme" decide, pedido a pedido, se usa Cookie (browser) ou Bearer/JWT (app mobile).
.AddPolicyScheme("MultiScheme", "Cookie ou Bearer", options =>
{
options.ForwardDefaultSelector = context =>
{
var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
return JwtBearerDefaults.AuthenticationScheme;

return CookieAuthenticationDefaults.AuthenticationScheme;
};
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAirportRepository, AirportRepository>();
builder.Services.AddScoped<IAirlineRepository, AirlineRepository>();
builder.Services.AddScoped<IGateRepository, GateRepository>();
builder.Services.AddScoped<IAircraftRepository, AircraftRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IBoardingPassRepository, BoardingPassRepository>();
builder.Services.AddScoped<IForumTopicRepository, ForumTopicRepository>();
builder.Services.AddScoped<IForumCommentRepository, ForumCommentRepository>(); 
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddTransient<IMailHelper, MailHelper>();
builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<IFlightExportService, FlightExportService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddHttpClient<PayPalService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddHostedService<ReservationExpirationService>();
// ─── HttpClient (para OpenWeatherMap) ───────────────────────────────────────
builder.Services.AddHttpClient();

// ─── Session (para carrinho de bilhetes) ────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "help",
    pattern: "ajuda",
    defaults: new { controller = "Home", action = "Help" });

app.MapControllerRoute(
    name: "contact",
    pattern: "contactos",
    defaults: new { controller = "Home", action = "Contact" });

app.MapControllerRoute(
    name: "privacy",
    pattern: "privacidade",
    defaults: new { controller = "Home", action = "Privacy" });

app.MapControllerRoute(
    name: "terms",
    pattern: "termos",
    defaults: new { controller = "Home", action = "Terms" });

// ── Seed da Base de Dados ─────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seedDb = new SeedDb(
            services.GetRequiredService<DataContext>(),
            services.GetRequiredService<UserManager<User>>(),
            services.GetRequiredService<RoleManager<IdentityRole>>()
        );
        await seedDb.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao fazer seed da base de dados.");
    }
}

app.Run();