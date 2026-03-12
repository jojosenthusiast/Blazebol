# AUTHENTICATION_FLOW.md — ControlTorneosFutbol

> Flujo completo de autenticación: registro, login, sesiones, middleware, lockout, auditoría.

---

## 1. Resumen del Sistema

- **Sin** ASP.NET Identity
- **Con** sesión HTTP personalizada (cookie de sesión)
- **Con** BCrypt para hashing de contraseñas
- **Con** AES-256-CBC para datos sensibles (DUI)
- **Con** lockout por intentos fallidos
- **Con** auditoría de accesos en BD

---

## 2. Flujo de Registro

### Paso a paso (ejemplo: Jugador)

```
1. Usuario accede a /Account/RegisterJugador
2. Rellena form: email, password, nombre, apellidos, posición, etc.
3. OnPostAsync() llama AuthService.RegisterJugadorAsync(model)
4. AuthService verifica: ¿email ya existe? → retorna (false, mensaje)
5. Crea Usuario: { Email, PasswordHash=BCrypt.Hash(password), Activo=true }
6. SaveChanges() → usuario.Id generado (Guid)
7. Crea Persona: { UsuarioId=usuario.Id, Nombre, Apellidos, DUI=Encrypt(dui) }
8. Crea Jugador: { PersonaId=persona.Id, Posicion, ... }
9. Busca Rol "Jugador" → crea UsuarioRol { UsuarioId, RolId }
10. SaveChanges()
11. Retorna (true, "Cuenta creada", usuario.Id)
12. PageModel hace login automático → SetUserSession + Redirect /Jugador/Dashboard
```

### Diferencias por tipo de registro

| Tipo | Extra |
|------|-------|
| Espectador | Solo crea Persona + rol |
| Jugador | + crea Jugador (posición, camiseta, etc.) |
| Entrenador | + crea Entrenador + Equipo + EquipoEntrenador (auto-crea equipo) |
| Árbitro | + crea Arbitro con EstadoAprobacion = "Pendiente" (workflow de aprobación) |

---

## 3. Flujo de Login

```
1. Usuario accede a /Account/Login
2. Rellena email + contraseña
3. OnPostAsync() → AuthService.LoginAsync(email, password)

AuthService.LoginAsync:
  a. Busca usuario por email (incluye Persona, UsuariosRoles→Rol)
     → si no existe: log auditoría (Fallido) + retorna (false, "Credenciales inválidas")
  b. ¿Usuario activo? → si no: retorna (false, "Cuenta desactivada")
  c. ¿Bloqueado? (FechaBloqueoFin > UtcNow) → retorna (false, "Cuenta bloqueada hasta HH:mm")
  d. ¿Contraseña correcta? → BCrypt.Verify(password, passwordHash)
     → si NO:
        ContadorAccesosFallidos++
        if (contador >= 5) { FechaBloqueoFin = UtcNow + 30min; }
        SaveChanges()
        log auditoría (LoginFallido)
        retorna (false, "Credenciales inválidas" o "Cuenta bloqueada")
  e. Login exitoso:
        ContadorAccesosFallidos = 0
        FechaBloqueoFin = null
        SaveChanges()
        log auditoría (Login, Exitoso)
        retorna (true, "Login exitoso", usuario)

4. PageModel recibe usuario
5. Obtiene rol del usuario: usuario.UsuariosRoles.First().Rol.Nombre
6. SetUserSession(userId, role, name, email)
7. Redirect según rol:
   Administrador → /Admin/Dashboard
   Espectador    → /Espectador/Index
   Jugador       → /Jugador/Dashboard
   Entrenador    → /Entrenador/Dashboard
   Arbitro       → /Arbitro/Dashboard
```

---

## 4. Sesión HTTP

### Claves almacenadas

```csharp
// SessionHelper.cs
private const string UserIdKey    = "UserId";    // Guid
private const string UserRoleKey  = "UserRole";  // "Administrador"|"Jugador"|...
private const string UserNameKey  = "UserName";  // email.Split('@')[0]
private const string UserEmailKey = "UserEmail"; // email completo
```

### Métodos de SessionHelper

```csharp
// Establecer sesión (post-login)
session.SetUserSession(userId, role, name, email);

// Leer desde PageModel
var userId    = HttpContext.Session.GetUserId();    // Guid?
var role      = HttpContext.Session.GetUserRole();  // string?
var name      = HttpContext.Session.GetUserName();  // string?
var email     = HttpContext.Session.GetUserEmail(); // string?

// Verificaciones
bool ok       = HttpContext.Session.IsAuthenticated();  // UserId != null
bool isAdmin  = HttpContext.Session.IsInRole("Administrador");

// Cerrar sesión
session.ClearUserSession();  // session.Clear()
```

### Configuración (Program.cs)

```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);   // Expira por inactividad
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// En pipeline:
app.UseSession();  // ANTES de MapRazorPages
```

---

## 5. Middleware de Autorización por Ruta

```csharp
// Program.cs — Protección por prefijo de ruta
app.Use(async (context, next) =>
{
    var path    = context.Request.Path.ToString().ToLower();
    var session = context.Session;

    var protectedRoutes = new Dictionary<string, string>
    {
        ["/admin"]      = "Administrador",
        ["/jugador"]    = "Jugador",
        ["/entrenador"] = "Entrenador",
        ["/arbitro"]    = "Arbitro",
        ["/espectador"] = "Espectador",
    };

    foreach (var (prefix, requiredRole) in protectedRoutes)
    {
        if (path.StartsWith(prefix))
        {
            if (!session.IsAuthenticated())
            {
                context.Response.Redirect("/Account/Login");
                return;
            }
            if (!session.IsInRole(requiredRole))
            {
                context.Response.Redirect("/Account/Login");
                return;
            }
            break;
        }
    }

    await next();
});
```

---

## 6. Sistema de Lockout

| Parámetro | Valor |
|-----------|-------|
| Intentos máximos | 5 |
| Duración del bloqueo | 30 minutos |
| Reset al éxito | Sí (contador = 0, fechaBloqueoFin = null) |

```csharp
// En AuthService.LoginAsync
private const int MaxFailedAttempts = 5;
private const int LockoutMinutes    = 30;

// Al fallar:
usuario.ContadorAccesosFallidos++;
if (usuario.ContadorAccesosFallidos >= MaxFailedAttempts)
{
    usuario.FechaBloqueoFin = DateTime.UtcNow.AddMinutes(LockoutMinutes);
}
await _context.SaveChangesAsync();

// Al desbloquear (admin):
// Admin/Usuarios/Index → POST handler → usuario.FechaBloqueoFin = null; usuario.ContadorAccesosFallidos = 0;
```

---

## 7. Auditoría de Accesos (`auditoriaaccesos`)

### Cuándo se registra

| Evento | `accion` | `resultado` |
|--------|---------|------------|
| Login exitoso | `Login` | `Exitoso` |
| Contraseña incorrecta | `LoginFallido` | `Fallido` |
| Cuenta bloqueada | `LoginBloqueado` | `Fallido` |
| Logout | `Logout` | `Exitoso` |

### Estructura del registro

```csharp
var log = new AuditoriaAccesos
{
    UsuarioId   = usuario?.Id,     // null si no se encontró el usuario
    Email       = email,           // siempre guardado
    Accion      = "Login",
    Resultado   = "Exitoso",
    IpAddress   = context.Connection.RemoteIpAddress?.ToString(),
    UserAgent   = context.Request.Headers["User-Agent"],
    Detalles    = "Login exitoso",
    FechaAcceso = DateTime.UtcNow
};
_context.AuditoriaAccesos.Add(log);
await _context.SaveChangesAsync();
```

---

## 8. Logout

```csharp
// Pages/Account/Logout.cshtml.cs
public async Task<IActionResult> OnGetAsync()
{
    // Registrar auditoría
    var userId = HttpContext.Session.GetUserId();
    // ... log ...

    // Limpiar sesión
    HttpContext.Session.ClearUserSession();

    return RedirectToPage("/Account/Login");
}
```

---

## 9. Anti-Cache (Back button protection)

```csharp
// Program.cs — middleware que previene ver páginas privadas
// con el botón "Atrás" del navegador después de cerrar sesión
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
    context.Response.Headers["Pragma"]        = "no-cache";
    context.Response.Headers["Expires"]       = "0";
    await next();
});
```

---

## 10. Cómo Agregar un Nuevo Rol

1. **Agregar constante** en `RolesConstantes`:
   ```csharp
   public const string NuevoRol = "NuevoRol";
   ```

2. **Sembrar en BD** (en Program.cs seed o migración):
   ```csharp
   if (!await context.Roles.AnyAsync(r => r.Nombre == "NuevoRol"))
       context.Roles.Add(new Rol { Nombre = "NuevoRol" });
   ```

3. **Crear página de registro** en `Pages/Account/RegisterNuevoRol.cshtml(.cs)`

4. **Agregar método en AuthService**:
   ```csharp
   Task<(bool, string, Guid?)> RegisterNuevoRolAsync(RegisterNuevoRolViewModel model);
   ```

5. **Proteger rutas** en middleware de Program.cs:
   ```csharp
   ["/nuevorol"] = "NuevoRol",
   ```

6. **Crear carpeta de páginas** `Pages/NuevoRol/` con Dashboard

7. **Actualizar Sidebar** `_Sidebar.cshtml` con sección para el nuevo rol

---

## 11. Credenciales de Prueba

| Rol | Email | Contraseña |
|-----|-------|-----------|
| Administrador | `admin@ex.com` | `Admin123!` |
| Espectador | `espectador1@demo.com` | `Usuario123!` |
| Jugador | `jug_e1_1@demo.com` | `Usuario123!` |
| Entrenador | `entrenador1@demo.com` | `Usuario123!` |
| Árbitro | `arbitro1@demo.com` | `Usuario123!` |

> Los usuarios demo se crean con `Scripts/seed_demo.sql`. El script hace TRUNCATE, por lo que el admin se elimina — re-insertar manualmente.
