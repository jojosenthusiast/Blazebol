using System;
using System.Threading.Tasks;
using ControlTorneosFutbol.Models.ViewModels;

namespace ControlTorneosFutbol.Services
{
    /// <summary>
    /// Servicio de autenticación y gestión de usuarios
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registra un nuevo espectador
        /// </summary>
        Task<(bool Success, string Message, Guid? UserId)> RegisterEspectadorAsync(RegisterEspectadorViewModel model);

        /// <summary>
        /// Registra un nuevo jugador
        /// </summary>
        Task<(bool Success, string Message, Guid? UserId)> RegisterJugadorAsync(RegisterJugadorViewModel model);

        /// <summary>
        /// Registra un nuevo entrenador y crea su equipo
        /// </summary>
        Task<(bool Success, string Message, Guid? UserId, int? EquipoId)> RegisterEntrenadorAsync(RegisterEntrenadorViewModel model);

        /// <summary>
        /// Registra un nuevo árbitro
        /// </summary>
        Task<(bool Success, string Message, Guid? UserId)> RegisterArbitroAsync(RegisterArbitroViewModel model);

        /// <summary>
        /// Autentica un usuario
        /// </summary>
        Task<(bool Success, string Message, Guid? UserId, string? Rol)> LoginAsync(LoginViewModel model, string ipAddress);

        /// <summary>
        /// Obtiene el rol principal de un usuario
        /// </summary>
        Task<string?> GetUserRoleAsync(Guid userId);

        /// <summary>
        /// Verifica si un email ya está registrado
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Incrementa el contador de intentos fallidos
        /// </summary>
        Task RegisterFailedLoginAsync(string email, string ipAddress, string reason);

        /// <summary>
        /// Reinicia el contador de intentos fallidos
        /// </summary>
        Task ResetFailedLoginCountAsync(Guid userId);

        /// <summary>
        /// Verifica si una cuenta está bloqueada
        /// </summary>
        Task<bool> IsAccountLockedAsync(string email);
    }
}