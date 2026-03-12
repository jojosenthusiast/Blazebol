using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Services;
using ControlTorneosFutbol.Helpers;
using ControlTorneosFutbol.Data.Entities;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
ConfigurationValidator.ValidateConfiguration(builder.Configuration);

// ========================================
// 1. CONFIGURACIÓN DE SERVICIOS
// ========================================

// Configurar DbContext con PostgreSQL (Supabase Transaction Pooler)
// No Reset On Close=true está en la connection string para compatibilidad con pgBouncer
// Npgsql 8 deshabilita auto-prepare por defecto (MaxAutoPrepare=0), no se necesita Prepare Threshold
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Registrar servicios de la aplicación
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITorneoService, TorneoService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IFixtureService, FixtureService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IConvocatoriaService, ConvocatoriaService>();
builder.Services.AddScoped<IPartidoEnVivoService, PartidoEnVivoService>();

// Configurar sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int>("Authentication:SessionTimeoutMinutes", 60)
    );
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ControlTorneos.Session";
});

builder.Services.AddRazorPages(options =>
{
    /*options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AuthorizeFolder("/Jugador");
    options.Conventions.AuthorizeFolder("/Entrenador");
    options.Conventions.AuthorizeFolder("/Arbitro");*/
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Error");
});

// Configurar antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = ".ControlTorneos.Antiforgery";
});

// Configurar HttpContext Accessor
builder.Services.AddHttpContextAccessor();

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ========================================
// 2. CONFIGURACIÓN DEL PIPELINE HTTP
// ========================================

// Configurar manejo de errores
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Railway maneja HTTPS en su proxy — solo redirigir en desarrollo local
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// Habilitar sesiones
app.UseSession();

// Middleware de autenticación personalizado
app.Use(async (context, next) =>
{
    // Verificar si hay sesión activa
    var path = context.Request.Path.Value?.ToLower() ?? "";
    var userId = context.Session.GetString("UserId");
    var userRole = context.Session.GetString("UserRole");

    // Si la ruta requiere autenticación y no hay sesión
    var requiresAuth = path.StartsWith("/admin") || 
                      path.StartsWith("/jugador") || 
                      path.StartsWith("/entrenador") || 
                      path.StartsWith("/arbitro") ||
                      path.StartsWith("/espectador");

    if (requiresAuth)
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private, max-age=0";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "-1";
        
        // Headers adicionales de seguridad
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    }

    if (requiresAuth && string.IsNullOrEmpty(userId))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }


    // Verificar que el usuario tenga el rol correcto
    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userRole))
    {
        var hasAccess = true;

        if (path.StartsWith("/admin") && userRole != "Administrador")
            hasAccess = false;
        else if (path.StartsWith("/jugador") && userRole != "Jugador")
            hasAccess = false;
        else if (path.StartsWith("/entrenador") && userRole != "Entrenador")
            hasAccess = false;
        else if (path.StartsWith("/arbitro") && userRole != "Arbitro")
            hasAccess = false;

        if (!hasAccess)
        {
            context.Response.Redirect("/Error?statusCode=403");
            return;
        }
    }

    await next();
});
app.UseAuthorization();

app.MapRazorPages();

// ========================================
// 3. INICIALIZACIÓN DE BASE DE DATOS
// ========================================

// Seed data automático al iniciar
// NOTA: Las migraciones se aplican manualmente con el script SQL en Supabase Studio.
//       Este bloque solo verifica la conexión e inserta los roles iniciales.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Verificando conexión a la base de datos...");

        try
        {
            // Abrir conexión directamente para obtener el error exacto si falla
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            logger.LogInformation("Conexión exitosa a la base de datos. Server: {Server}", conn.DataSource);
            await conn.CloseAsync();

            // Seed data inicial (si no existen roles)
            if (!await context.Roles.AnyAsync())
            {
                logger.LogInformation("Insertando datos iniciales (roles)...");
                await SeedInitialDataAsync(context);
                logger.LogInformation("Datos iniciales insertados correctamente");
            }
        }
        catch (Exception connEx)
        {
            logger.LogError("DETALLE DEL ERROR DE CONEXIÓN: {Type} — {Message}", connEx.GetType().Name, connEx.Message);
            if (connEx.InnerException != null)
                logger.LogError("Inner: {Inner}", connEx.InnerException.Message);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error inesperado al inicializar");
    }
}

app.Run();

// ========================================
// 4. MÉTODOS AUXILIARES
// ========================================

static async Task SeedInitialDataAsync(ApplicationDbContext context)
{
    // Verificar si ya existen roles
    if (await context.Roles.AnyAsync())
        return;

    // Insertar roles predefinidos
    var roles = new[]
    {
        new Rol 
        { 
            Nombre = "Administrador", 
            NombreNormalizado = "ADMINISTRADOR", 
            Descripcion = "Administrador con acceso completo" 
        },
        new Rol 
        { 
            Nombre = "Espectador", 
            NombreNormalizado = "ESPECTADOR", 
            Descripcion = "Usuario permisos solo de lectura" 
        },
        new Rol 
        { 
            Nombre = "Jugador", 
            NombreNormalizado = "JUGADOR", 
            Descripcion = "Jugador inscrito" 
        },
        new Rol 
        { 
            Nombre = "Entrenador", 
            NombreNormalizado = "ENTRENADOR", 
            Descripcion = "Entrenador de equipos" 
        },
        new Rol 
        { 
            Nombre = "Arbitro", 
            NombreNormalizado = "ARBITRO", 
            Descripcion = "Árbitro de partidos" 
        }
    };

    context.Roles.AddRange(roles);
    await context.SaveChangesAsync();
}