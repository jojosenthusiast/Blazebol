using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;

namespace ControlTorneosFutbol.Services
{
    public interface ITorneoService
    {
        /// <summary>
        /// Obtiene todos los torneos con filtros opcionales
        /// </summary>
        Task<List<Torneo>> GetTorneosAsync(string? estado = null, int? anio = null, int? categoriaId = null);

        /// <summary>
        /// Obtiene un torneo por ID
        /// </summary>
        Task<Torneo?> GetTorneoByIdAsync(int id);

        /// <summary>
        /// Crea un nuevo torneo
        /// </summary>
        Task<(bool Success, string Message, int? TorneoId)> CreateTorneoAsync(TorneoViewModel model);

        /// <summary>
        /// Actualiza un torneo existente
        /// </summary>
        Task<(bool Success, string Message)> UpdateTorneoAsync(int id, TorneoViewModel model);

        /// <summary>
        /// Desactiva un torneo (soft delete)
        /// </summary>
        Task<(bool Success, string Message)> DesactivarTorneoAsync(int id);

        /// <summary>
        /// Verifica si un torneo puede ser editado (no está finalizado)
        /// </summary>
        Task<bool> CanEditTorneoAsync(int id);

        /// <summary>
        /// Obtiene ligas activas
        /// </summary>
        Task<List<Liga>> GetLigasActivasAsync();

        /// <summary>
        /// Obtiene temporadas activas
        /// </summary>
        Task<List<Temporada>> GetTemporadasActivasAsync();

        /// <summary>
        /// Obtiene todas las categorías
        /// </summary>
        Task<List<Categoria>> GetCategoriasAsync();
    }
}