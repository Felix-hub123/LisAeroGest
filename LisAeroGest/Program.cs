using LisAeroGest.Data;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using LisAeroGest.Hub;
using LisAeroGest.Services;
using LisAeroGest.Services.Integrations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ── Base de Dados ──────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    // Contexto principal da aplicação.
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("LisAeroGest")
                 .MigrationsHistoryTable("__EFMigrationsHistory")
        )
    );

    // Contexto utilizado pelas migrações de SQL Server.
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

    // Contexto principal da aplicação.
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("LisAeroGest")
                 .MigrationsHistoryTable("__EFMigrationsHistory", "public")
        )
    );

    // Contexto utilizado pelas migrações de PostgreSQL.
    builder.Services.AddDbContext<DataContextPostgres>(options =>
        options.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("LisAeroGest")
                 .MigrationsHistoryTable("__EFMigrationsHistory", "public")
        )
    );
}

// ── Identity ───────────────────────────────────────────────────────────
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.SignIn.RequireConfirmedEmail = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

// ── Autenticação JWT + Cookie ───────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiScheme";
    options.DefaultChallengeScheme = "MultiScheme";
})
.AddPolicyScheme("MultiScheme", "Cookie ou Bearer", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var authHeader =
            context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader != null &&
            authHeader.StartsWith(
                "Bearer ",
                StringComparison.OrdinalIgnoreCase))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

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

// ── Controllers e serviços ─────────────────────────────────────────────
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
builder.Services.AddScoped<IFlightCommunicationRepository, FlightCommunicationRepository>();

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddTransient<IMailHelper, MailHelper>();
builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IConverterHelper, ConverterHelper>();

builder.Services.AddHttpClient<IWeatherService, WeatherService>();

builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<IFlightExportService, FlightExportService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

builder.Services.AddHttpClient<PayPalService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();

builder.Services.AddHostedService<ReservationExpirationService>();
builder.Services.AddHostedService<FlightStatusUpdaterService>();

builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

builder.Services.AddSignalR();
builder.Services.AddSignalR();

builder.Services.AddHttpClient();

// ── Integração com a companhia aérea ────────────────────────────────────
builder.Services.AddHttpClient<AirlineApiClient>(client =>
{
    // Por enquanto, utilizamos a companhia simulada no computador local.
    // A configuração permitirá substituir este endereço posteriormente.
    var baseUrl =
        builder.Configuration["AirlineIntegration:BaseUrl"]
        ?? "https://localhost:7268/";

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);

    // Tempo máximo de espera por uma resposta.
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ── Session ────────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Cultura ────────────────────────────────────────────────────────────
var supportedCultures = new[]
{
    new System.Globalization.CultureInfo("pt-PT")
};

builder.Services.Configure<
    Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(options =>
    {
        options.DefaultRequestCulture =
            new Microsoft.AspNetCore.Localization.RequestCulture("pt-PT");

        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

var app = builder.Build();

// ── Pipeline HTTP ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ── Rotas ──────────────────────────────────────────────────────────────
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

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

// ── Seed da Base de Dados ───────────────────────────────────────────────
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
;
