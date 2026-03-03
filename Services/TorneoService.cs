using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using ControlTorneosFutbol.Models.ViewModels;

namespace ControlTorneosFutbol.Services
{
    public class TorneoService : ITorneoService
    {
        private readonly ApplicationDbContext _context;

        public TorneoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Torneo>> GetTorneosAsync(string? estado = null, int? anio = null, int? categoriaId = null)
        {
            var query = _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(t => t.Estado == estado);
            }

            if (anio.HasValue)
            {
                query = query.Where(t => t.Temporada.Anio == anio.Value);
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(t => t.CategoriaId == categoriaId.Value);
            }

            return await query
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Torneo?> GetTorneoByIdAsync(int id)
        {
            return await _context.Torneos
                .Include(t => t.Liga)
                .Include(t => t.Temporada)
                .Include(t => t.Categoria)
                .Include(t => t.ConfiguracionTorneo)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<(bool Success, string Message, int? TorneoId)> CreateTorneoAsync(TorneoViewModel model)
        {
            try
            {
                // Validar fechas
                if (model.FechaFin <= model.FechaInicio)
                {
                    return (false, "La fecha de fin debe ser posterior a la fecha de inicio", null);
                }

                // Crear torneo
                var torneo = new Torneo
                {
                    LigaId = model.LigaId,
                    TemporadaId = model.TemporadaId,
                    CategoriaId = model.CategoriaId,
                    Nombre = model.Nombre,
                    Formato = model.Formato,
                    Estado = model.Estado,
                    FechaInicio = model.FechaInicio,
                    FechaFin = model.FechaFin,
                    NumeroEquiposMaximo = model.NumeroEquiposMaximo,
                    NumeroEquiposMinimo = model.NumeroEquiposMinimo
                };

                _context.Torneos.Add(torneo);
                await _context.SaveChangesAsync();

                // Crear configuración predeterminada
                var configuracion = new ConfiguracionTorneo
                {
                    TorneoId = torneo.Id,
                    PuntosPorVictoria = 3,
                    PuntosPorEmpate = 1,
                    PuntosPorDerrota = 0,
                    CriterioDesempate1 = "Puntos",
                    CriterioDesempate2 = "DiferenciaGoles",
                    CriterioDesempate3 = "GolesFavor",
                    TarjetasAmarillasParaSuspension = 3,
                    PartidosSuspensionTarjetaRoja = 2,
                    MaximoJugadoresConvocados = 18,
                    MinimoJugadoresTitulares = 11
                };

                _context.ConfiguracionesTorneo.Add(configuracion);
                await _context.SaveChangesAsync();

                return (true, "Torneo creado exitosamente", torneo.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear torneo: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateTorneoAsync(int id, TorneoViewModel model)
        {
            try
            {
                var torneo = await _context.Torneos.FindAsync(id);

                if (torneo == null)
                {
                    return (false, "Torneo no encontrado");
                }

                // Validar que no esté finalizado
                if (torneo.Estado == EstadosTorneo.Finalizado)
                {
                    return (false, "No se puede modificar un torneo finalizado");
                }

                // Validar fechas
                if (model.FechaFin <= model.FechaInicio)
                {
                    return (false, "La fecha de fin debe ser posterior a la fecha de inicio");
                }

                // Actualizar datos
                torneo.LigaId = model.LigaId;
                torneo.TemporadaId = model.TemporadaId;
                torneo.CategoriaId = model.CategoriaId;
                torneo.Nombre = model.Nombre;
                torneo.Formato = model.Formato;
                torneo.Estado = model.Estado;
                torneo.FechaInicio = model.FechaInicio;
                torneo.FechaFin = model.FechaFin;
                torneo.NumeroEquiposMaximo = model.NumeroEquiposMaximo;
                torneo.NumeroEquiposMinimo = model.NumeroEquiposMinimo;
                torneo.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return (true, "Torneo actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar torneo: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DesactivarTorneoAsync(int id)
        {
            try
            {
                var torneo = await _context.Torneos.FindAsync(id);

                if (torneo == null)
                {
                    return (false, "Torneo no encontrado");
                }

                // Cambiar estado a Finalizado en lugar de borrar
                torneo.Estado = EstadosTorneo.Finalizado;
                torneo.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return (true, "Torneo desactivado exitosamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al desactivar torneo: {ex.Message}");
            }
        }

        public async Task<bool> CanEditTorneoAsync(int id)
        {
            var torneo = await _context.Torneos.FindAsync(id);
            return torneo != null && torneo.Estado != EstadosTorneo.Finalizado;
        }

        public async Task<List<Liga>> GetLigasActivasAsync()
        {
            return await _context.Ligas
                .Where(l => l.Activa)
                .OrderBy(l => l.Nombre)
                .ToListAsync();
        }

        public async Task<List<Temporada>> GetTemporadasActivasAsync()
        {
            return await _context.Temporadas
                .Where(t => t.Activa)
                .OrderByDescending(t => t.Anio)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            return await _context.Categorias
                .OrderBy(c => c.Orden)
                .ToListAsync();
        }
    }
}