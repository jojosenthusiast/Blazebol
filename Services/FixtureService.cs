using ControlTorneosFutbol.Data;
using ControlTorneosFutbol.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlTorneosFutbol.Services
{
    public class FixtureService : IFixtureService
    {
        private readonly ApplicationDbContext _db;

        public FixtureService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────────────────────────────────
        // TODOS CONTRA TODOS (Round-Robin)
        // ─────────────────────────────────────────────────────────────────────
        public Task<List<Partido>> GenerarTodosContraTodos(int torneoId, ConfiguracionTorneo config, List<int> equipoIds)
        {
            var equipos = equipoIds.ToList();
            var partidos = new List<Partido>();

            // Algoritmo de círculo: fija el primer equipo y rota los demás
            if (equipos.Count % 2 != 0)
                equipos.Add(-1); // bye

            int n = equipos.Count;
            int totalJornadas = n - 1;
            int partidosPorJornada = n / 2;

            for (int jornada = 1; jornada <= totalJornadas; jornada++)
            {
                for (int i = 0; i < partidosPorJornada; i++)
                {
                    int local = equipos[i];
                    int visitante = equipos[n - 1 - i];

                    if (local == -1 || visitante == -1)
                        continue; // bye

                    bool invertir = config.EquilibrioLocalVisitante && jornada % 2 == 0;
                    partidos.Add(CrearPartido(torneoId,
                        invertir ? visitante : local,
                        invertir ? local : visitante,
                        "Jornada " + jornada, jornada));
                }

                // Rotar: fija [0], mueve los demás una posición en sentido horario
                var ultimo = equipos[n - 1];
                for (int k = n - 1; k > 1; k--)
                    equipos[k] = equipos[k - 1];
                equipos[1] = ultimo;
            }

            if (config.IdaYVuelta)
            {
                // Duplicar con local/visitante invertidos, jornadas continuadas
                var vuelta = partidos
                    .Select(p => CrearPartido(torneoId, p.EquipoVisitanteId, p.EquipoLocalId,
                        "Vuelta J" + p.Jornada, totalJornadas + p.Jornada!.Value))
                    .ToList();
                partidos.AddRange(vuelta);
            }

            return Task.FromResult(partidos);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ELIMINACIÓN DIRECTA (Bracket)
        // ─────────────────────────────────────────────────────────────────────
        public Task<List<Partido>> GenerarEliminacionDirecta(int torneoId, List<int> equipoIds)
        {
            var equipos = Shuffle(equipoIds.ToList());
            var partidos = new List<Partido>();
            int n = equipos.Count;

            // Ajustar a siguiente potencia de 2 (byes)
            int potencia = 1;
            while (potencia < n) potencia *= 2;

            // Rellenar con byes (-1)
            while (equipos.Count < potencia)
                equipos.Add(-1);

            int ronda = 1;
            string[] nombresFase = GetNombresFase(potencia);
            int faseIdx = 0;

            var actuales = equipos.ToList();
            while (actuales.Count > 1)
            {
                string fase = faseIdx < nombresFase.Length ? nombresFase[faseIdx] : "Ronda " + ronda;
                var siguientes = new List<int>();

                for (int i = 0; i < actuales.Count; i += 2)
                {
                    int a = actuales[i];
                    int b = actuales[i + 1];

                    if (a == -1 && b == -1)
                    {
                        siguientes.Add(-1);
                    }
                    else if (a == -1)
                    {
                        siguientes.Add(b); // bye automático
                    }
                    else if (b == -1)
                    {
                        siguientes.Add(a); // bye automático
                    }
                    else
                    {
                        partidos.Add(CrearPartido(torneoId, a, b, fase, ronda));
                        siguientes.Add(-1); // ganador TBD — se completará cuando se juegue
                    }
                }

                actuales = siguientes;
                ronda++;
                faseIdx++;
            }

            return Task.FromResult(partidos);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FASE DE GRUPOS (Champions)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<List<Partido>> GenerarGrupos(int torneoId, ConfiguracionTorneo config, List<int> equipoIds)
        {
            int numGrupos = config.NumeroGrupos ?? 4;
            var equipos = Shuffle(equipoIds.ToList());
            var partidos = new List<Partido>();

            // Distribuir equipos en grupos (serpentín para equilibrio)
            var grupos = new List<List<int>>();
            for (int g = 0; g < numGrupos; g++)
                grupos.Add(new List<int>());

            for (int i = 0; i < equipos.Count; i++)
            {
                int grupoIdx = i % numGrupos;
                grupos[grupoIdx].Add(equipos[i]);
            }

            // Guardar grupos en DB y generar fixture round-robin por grupo
            for (int g = 0; g < numGrupos; g++)
            {
                char letra = (char)('A' + g);
                string nombreGrupo = "Grupo " + letra;

                // Crear o recuperar entidad Grupo
                var grupo = await _db.Grupos.FirstOrDefaultAsync(x => x.TorneoId == torneoId && x.Nombre == nombreGrupo);
                if (grupo == null)
                {
                    grupo = new Grupo { TorneoId = torneoId, Nombre = nombreGrupo };
                    _db.Grupos.Add(grupo);
                    await _db.SaveChangesAsync();
                }

                var equiposGrupo = grupos[g];
                if (equiposGrupo.Count % 2 != 0)
                    equiposGrupo.Add(-1);

                int n = equiposGrupo.Count;
                int jornadas = n - 1;

                for (int jornada = 1; jornada <= jornadas; jornada++)
                {
                    for (int i = 0; i < n / 2; i++)
                    {
                        int local = equiposGrupo[i];
                        int visitante = equiposGrupo[n - 1 - i];

                        if (local == -1 || visitante == -1) continue;

                        var partido = CrearPartido(torneoId, local, visitante, nombreGrupo, jornada);
                        partido.GrupoId = grupo.Id;
                        partidos.Add(partido);
                    }

                    // Rotación
                    var ultimo = equiposGrupo[n - 1];
                    for (int k = n - 1; k > 1; k--)
                        equiposGrupo[k] = equiposGrupo[k - 1];
                    equiposGrupo[1] = ultimo;
                }

                if (config.IdaYVuelta)
                {
                    var vuelta = partidos
                        .Where(p => p.Fase == nombreGrupo)
                        .Select(p => {
                            var v = CrearPartido(torneoId, p.EquipoVisitanteId, p.EquipoLocalId,
                                nombreGrupo, jornadas + p.Jornada!.Value);
                            v.GrupoId = grupo.Id;
                            return v;
                        }).ToList();
                    partidos.AddRange(vuelta);
                }
            }

            return partidos;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GUARDAR FIXTURE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<int> GuardarFixture(int torneoId, List<Partido> partidos)
        {
            // Eliminar partidos existentes Programado (no tocar los ya jugados)
            var existentes = await _db.Partidos
                .Where(p => p.TorneoId == torneoId && p.Estado == EstadosPartido.Programado)
                .ToListAsync();
            _db.Partidos.RemoveRange(existentes);

            _db.Partidos.AddRange(partidos);
            await _db.SaveChangesAsync();
            return partidos.Count;
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> TieneFixture(int torneoId)
            => await _db.Partidos.AnyAsync(p => p.TorneoId == torneoId);

        public async Task<(int total, int programados, int jugados)> EstadisticasFixture(int torneoId)
        {
            var partidos = await _db.Partidos.Where(p => p.TorneoId == torneoId).ToListAsync();
            int total = partidos.Count;
            int programados = partidos.Count(p => p.Estado == EstadosPartido.Programado);
            int jugados = partidos.Count(p => p.Estado == EstadosPartido.Finalizado);
            return (total, programados, jugados);
        }

        private static Partido CrearPartido(int torneoId, int localId, int visitanteId, string fase, int jornada)
            => new Partido
            {
                TorneoId = torneoId,
                EquipoLocalId = localId,
                EquipoVisitanteId = visitanteId,
                Fase = fase,
                Jornada = jornada,
                Fecha = DateTime.UtcNow, // se asignará en AsignarDetalles
                Estado = EstadosPartido.Programado,
                FechaCreacion = DateTime.UtcNow
            };

        private static List<int> Shuffle(List<int> lista)
        {
            var rng = new Random();
            for (int i = lista.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (lista[i], lista[j]) = (lista[j], lista[i]);
            }
            return lista;
        }

        private static string[] GetNombresFase(int totalEquipos) => totalEquipos switch
        {
            2  => ["Final"],
            4  => ["Semifinal", "Final"],
            8  => ["Cuartos", "Semifinal", "Final"],
            16 => ["Octavos", "Cuartos", "Semifinal", "Final"],
            32 => ["Dieciseisavos", "Octavos", "Cuartos", "Semifinal", "Final"],
            _  => ["Ronda 1", "Ronda 2", "Ronda 3", "Ronda 4", "Final"]
        };
    }
}
