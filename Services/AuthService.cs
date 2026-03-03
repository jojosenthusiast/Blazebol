using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;

namespace ControlTorneosFutbol.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 30;

        public AuthService(ApplicationDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task<(bool Success, string Message, Guid? UserId)> RegisterEspectadorAsync(RegisterEspectadorViewModel model)
        {
            try
            {
                // Validar que el email no exista
                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "El correo electrónico ya está registrado", null);
                }

                // Crear usuario
                var usuario = new Usuario
                {
                    Email = model.Email,
                    PasswordHash = _encryptionService.HashPassword(model.Password),
                    NombreUsuario = model.Email,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Crear persona
                var persona = new Persona
                {
                    UsuarioId = usuario.Id,
                    Nombre = model.Nombre,
                    ApellidoPaterno = model.ApellidoPaterno,
                    ApellidoMaterno = model.ApellidoMaterno,
                    Activo = true
                };

                _context.Personas.Add(persona);

                // Asignar rol de Espectador
                var rolEspectador = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Nombre == RolesConstantes.Espectador);

                if (rolEspectador == null)
                {
                    return (false, "Error: Rol de Espectador no encontrado en el sistema", null);
                }

                var usuarioRol = new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = rolEspectador.Id
                };

                _context.UsuariosRoles.Add(usuarioRol);
                await _context.SaveChangesAsync();

                return (true, "Cuenta de espectador creada exitosamente", usuario.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar espectador: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Guid? UserId)> RegisterJugadorAsync(RegisterJugadorViewModel model)
        {
            try
            {
                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "El correo electrónico ya está registrado", null);
                }

                // Crear usuario
                var usuario = new Usuario
                {
                    Email = model.Email,
                    PasswordHash = _encryptionService.HashPassword(model.Password),
                    NombreUsuario = model.Email,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Crear persona con datos sensibles cifrados
                var persona = new Persona
                {
                    UsuarioId = usuario.Id,
                    Nombre = model.Nombre,
                    ApellidoPaterno = model.ApellidoPaterno,
                    ApellidoMaterno = model.ApellidoMaterno,
                    FechaNacimiento = model.FechaNacimiento,
                    IdentificacionCifrada = !string.IsNullOrEmpty(model.NumeroIdentificacion)
                        ? _encryptionService.Encrypt(model.NumeroIdentificacion)
                        : null,
                    Telefono = model.Telefono,
                    Activo = true
                };

                _context.Personas.Add(persona);

                // Crear jugador
                var jugador = new Jugador
                {
                    PersonaId = persona.Id,
                    PosicionPreferida = model.PosicionPreferida,
                    NumeroCamiseta = model.NumeroCamiseta
                };

                _context.Jugadores.Add(jugador);

                // Asignar rol de Jugador
                var rolJugador = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Nombre == RolesConstantes.Jugador);

                if (rolJugador == null)
                {
                    return (false, "Error: Rol de Jugador no encontrado en el sistema", null);
                }

                var usuarioRol = new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = rolJugador.Id
                };

                _context.UsuariosRoles.Add(usuarioRol);
                await _context.SaveChangesAsync();

                return (true, "Cuenta de jugador creada exitosamente", usuario.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar jugador: {ex.Message}", null);
            }
        }

        
        public async Task<(bool Success, string Message, Guid? UserId, int? EquipoId)> RegisterEntrenadorAsync(RegisterEntrenadorViewModel model)
        {
            // ELIMINAR: using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "El correo electrónico ya está registrado", null, null);
                }

                if (await _context.Equipos.AnyAsync(e => e.Nombre == model.NombreEquipo))
                {
                    return (false, "Ya existe un equipo con ese nombre", null, null);
                }

                // Crear usuario
                var usuario = new Usuario
                {
                    Email = model.Email,
                    PasswordHash = _encryptionService.HashPassword(model.Password),
                    NombreUsuario = model.Email,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Crear persona
                var persona = new Persona
                {
                    UsuarioId = usuario.Id,
                    Nombre = model.Nombre,
                    ApellidoPaterno = model.ApellidoPaterno,
                    ApellidoMaterno = model.ApellidoMaterno,
                    Telefono = model.Telefono,
                    Activo = true
                };

                _context.Personas.Add(persona);

                // Crear entrenador
                var entrenador = new Entrenador
                {
                    PersonaId = persona.Id,
                    NumeroLicencia = model.NumeroLicencia,
                    FechaExpiracionLicencia = model.FechaExpiracionLicencia
                };

                _context.Entrenadores.Add(entrenador);
                await _context.SaveChangesAsync();

                // Crear equipo
                var equipo = new Equipo
                {
                    Nombre = model.NombreEquipo,
                    Abreviacion = model.Abreviacion?.ToUpper(),
                    Logo = model.LogoPath,
                    ColorPrimario = model.ColorPrimario,
                    ColorSecundario = model.ColorSecundario,
                    Sede = model.Sede,
                    Activo = true
                };

                _context.Equipos.Add(equipo);
                await _context.SaveChangesAsync();

                // Asignar entrenador al equipo como propietario
                var equipoEntrenador = new EquipoEntrenador
                {
                    EntrenadorId = entrenador.Id,
                    EquipoId = equipo.Id,
                    Rol = RolesEntrenador.Principal,
                    FechaInicio = DateTime.UtcNow,
                    EsPropietario = true,
                    Activo = true
                };

                _context.EquiposEntrenadores.Add(equipoEntrenador);

                // Asignar rol de Entrenador
                var rolEntrenador = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Nombre == RolesConstantes.Entrenador);

                if (rolEntrenador == null)
                {
                    return (false, "Error: Rol de Entrenador no encontrado en el sistema", null, null);
                }

                var usuarioRol = new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = rolEntrenador.Id
                };

                _context.UsuariosRoles.Add(usuarioRol);
                await _context.SaveChangesAsync();

                // ELIMINAR: await transaction.CommitAsync();

                return (true, "Cuenta de entrenador y equipo creados exitosamente", usuario.Id, equipo.Id);
            }
            catch (Exception ex)
            {
                // ELIMINAR: await transaction.RollbackAsync();
                return (false, $"Error al registrar entrenador: {ex.Message}", null, null);
            }
        }

        public async Task<(bool Success, string Message, Guid? UserId)> RegisterArbitroAsync(RegisterArbitroViewModel model)
        {
            try
            {
                if (await EmailExistsAsync(model.Email))
                {
                    return (false, "El correo electrónico ya está registrado", null);
                }

                // Validar que el número de licencia no exista
                if (await _context.Arbitros.AnyAsync(a => a.NumeroLicencia == model.NumeroLicencia))
                {
                    return (false, "Ya existe un árbitro con ese número de licencia", null);
                }

                // Crear usuario
                var usuario = new Usuario
                {
                    Email = model.Email,
                    PasswordHash = _encryptionService.HashPassword(model.Password),
                    NombreUsuario = model.Email,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Crear persona
                var persona = new Persona
                {
                    UsuarioId = usuario.Id,
                    Nombre = model.Nombre,
                    ApellidoPaterno = model.ApellidoPaterno,
                    ApellidoMaterno = model.ApellidoMaterno,
                    Telefono = model.Telefono,
                    Activo = true
                };

                _context.Personas.Add(persona);

                // Crear árbitro (estado: Pendiente de aprobación)
                var arbitro = new Arbitro
                {
                    PersonaId = persona.Id,
                    NumeroLicencia = model.NumeroLicencia,
                    CategoriasArbitra = model.CategoriasArbitra,
                    EstadoAprobacion = EstadosAprobacionArbitro.Pendiente
                };

                _context.Arbitros.Add(arbitro);

                // Asignar rol de Árbitro
                var rolArbitro = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Nombre == RolesConstantes.Arbitro);

                if (rolArbitro == null)
                {
                    return (false, "Error: Rol de Árbitro no encontrado en el sistema", null);
                }

                var usuarioRol = new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = rolArbitro.Id
                };

                _context.UsuariosRoles.Add(usuarioRol);
                await _context.SaveChangesAsync();

                return (true, "Cuenta de árbitro creada. Pendiente de aprobación por el administrador.", usuario.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar árbitro: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Guid? UserId, string? Rol)> LoginAsync(LoginViewModel model, string ipAddress)
        {
            try
            {
                // 1. BUSCAR USUARIO
                var usuario = await _context.Usuarios
                    .Include(u => u.UsuariosRoles)
                        .ThenInclude(ur => ur.Rol)
                    .Include(u => u.Persona)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (usuario == null)
                {
                    await RegisterFailedLoginAsync(model.Email, ipAddress, "Email no encontrado");
                    return (false, "Credenciales incorrectas", null, null);
                }

                if (usuario.FechaBloqueoFin.HasValue && usuario.FechaBloqueoFin.Value > DateTime.UtcNow)
                {
                    var minutosRestantes = Math.Ceiling((usuario.FechaBloqueoFin.Value - DateTime.UtcNow).TotalMinutes);
                    
                    await RegisterFailedLoginAsync(
                        model.Email, 
                        ipAddress, 
                        $"Intento de acceso - Cuenta bloqueada ({minutosRestantes} min restantes)"
                    );
                    
                    return (false, $"Cuenta bloqueada. Intente nuevamente en {minutosRestantes} minutos.", null, null);
                }

                // 3. VERIFICAR SI ESTÁ ACTIVO
                if (!usuario.Activo)
                {
                    await RegisterFailedLoginAsync(model.Email, ipAddress, "Usuario inactivo");
                    return (false, "La cuenta está desactivada. Contacte al administrador.", null, null);
                }

                // 4. VERIFICAR CONTRASEÑA
                var passwordCorrecta = _encryptionService.VerifyPassword(model.Password, usuario.PasswordHash);

                if (!passwordCorrecta)
                {
                    // INCREMENTAR CONTADOR
                    usuario.ContadorAccesosFallidos++;
                    usuario.FechaActualizacion = DateTime.UtcNow;

                    // VERIFICAR SI ALCANZÓ EL MÁXIMO
                    if (usuario.ContadorAccesosFallidos >= MaxFailedAttempts)
                    {
                        // BLOQUEAR CUENTA
                        usuario.FechaBloqueoFin = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                        
                        // GUARDAR CAMBIOS INMEDIATAMENTE
                        await _context.SaveChangesAsync();
                        
                        await RegisterFailedLoginAsync(
                            model.Email, 
                            ipAddress, 
                            $"Cuenta bloqueada por {LockoutMinutes} minutos tras {MaxFailedAttempts} intentos fallidos"
                        );
                        
                        return (false, $"Cuenta bloqueada por {LockoutMinutes} minutos debido a múltiples intentos fallidos.", null, null);
                    }

                    // GUARDAR INCREMENTO DE CONTADOR
                    await _context.SaveChangesAsync();
                    
                    var intentosRestantes = MaxFailedAttempts - usuario.ContadorAccesosFallidos;
                    
                    await RegisterFailedLoginAsync(
                        model.Email, 
                        ipAddress, 
                        $"Contraseña incorrecta - Intento {usuario.ContadorAccesosFallidos}/{MaxFailedAttempts}"
                    );
                    
                    return (false, $"Credenciales incorrectas. Le quedan {intentosRestantes} intento(s).", null, null);
                }

                // 5. CONTRASEÑA CORRECTA
                
                // IMPORTANTE: Verificar si el bloqueo ya expiró
                if (usuario.FechaBloqueoFin.HasValue)
                {
                    if (usuario.FechaBloqueoFin.Value <= DateTime.UtcNow)
                    {
                        // Bloqueo expiró - DESBLOQUEAR
                        usuario.FechaBloqueoFin = null;
                        usuario.ContadorAccesosFallidos = 0;
                    }
                    else
                    {
                        // Bloqueo AÚN VIGENTE - NO PERMITIR LOGIN
                        var minutosRestantes = Math.Ceiling((usuario.FechaBloqueoFin.Value - DateTime.UtcNow).TotalMinutes);
                        
                        await RegisterFailedLoginAsync(
                            model.Email, 
                            ipAddress, 
                            $"Intento con contraseña correcta - Cuenta aún bloqueada ({minutosRestantes} min)"
                        );
                        
                        return (false, $"Cuenta bloqueada. Intente nuevamente en {minutosRestantes} minutos.", null, null);
                    }
                }
                else
                {
                    // Sin bloqueo - resetear contador
                    usuario.ContadorAccesosFallidos = 0;
                }

                usuario.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // 6. REGISTRAR LOGIN EXITOSO
                await _context.AuditoriaAccesos.AddAsync(new AuditoriaAccesos
                {
                    UsuarioId = usuario.Id,
                    Email = usuario.Email,
                    Exitoso = true,
                    DireccionIP = ipAddress,
                    FechaIntento = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                // 7. OBTENER ROL Y RETORNAR
                var rolPrincipal = usuario.UsuariosRoles.FirstOrDefault()?.Rol.Nombre;

                return (true, "Login exitoso", usuario.Id, rolPrincipal);
            }
            catch (Exception ex)
            {
                await RegisterFailedLoginAsync(model.Email, ipAddress, $"Error del sistema: {ex.Message}");
                return (false, "Error al procesar el login", null, null);
            }
        }


        public async Task<string?> GetUserRoleAsync(Guid userId)
        {
            var usuarioRol = await _context.UsuariosRoles
                .Include(ur => ur.Rol)
                .Where(ur => ur.UsuarioId == userId)
                .FirstOrDefaultAsync();

            return usuarioRol?.Rol.Nombre;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task RegisterFailedLoginAsync(string email, string ipAddress, string reason)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            await _context.AuditoriaAccesos.AddAsync(new AuditoriaAccesos
            {
                UsuarioId = usuario?.Id,
                Email = email,
                Exitoso = false,
                DireccionIP = ipAddress,
                MotivoFallo = reason,
                FechaIntento = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task ResetFailedLoginCountAsync(Guid userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario != null)
            {
                usuario.ContadorAccesosFallidos = 0;
                usuario.FechaBloqueoFin = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
                return false;

            return usuario.FechaBloqueoFin.HasValue && usuario.FechaBloqueoFin.Value > DateTime.UtcNow;
        }
    }
}