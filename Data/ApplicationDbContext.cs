using Microsoft.EntityFrameworkCore;
using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets principales
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<UsuarioRol> UsuariosRoles { get; set; } = null!;
        public DbSet<Persona> Personas { get; set; } = null!;
        public DbSet<Jugador> Jugadores { get; set; } = null!;
        public DbSet<Entrenador> Entrenadores { get; set; } = null!;
        public DbSet<Arbitro> Arbitros { get; set; } = null!;
        public DbSet<Liga> Ligas { get; set; } = null!;
        public DbSet<Temporada> Temporadas { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Torneo> Torneos { get; set; } = null!;
        public DbSet<ConfiguracionTorneo> ConfiguracionesTorneo { get; set; } = null!;
        public DbSet<Grupo> Grupos { get; set; } = null!;
        public DbSet<Equipo> Equipos { get; set; } = null!;
        public DbSet<EquipoTorneo> EquiposTorneos { get; set; } = null!;
        public DbSet<EquipoEntrenador> EquiposEntrenadores { get; set; } = null!;
        public DbSet<EquipoJugadorTorneo> EquiposJugadoresTorneos { get; set; } = null!;
        public DbSet<SolicitudJugadorEquipo> SolicitudesJugadorEquipo { get; set; } = null!;
        public DbSet<AuditoriaAccesos> AuditoriaAccesos { get; set; } = null!;
        public DbSet<ReglaPersonalizadaTorneo> ReglasPersonalizadasTorneo { get; set; } = null!;
        public DbSet<PreferenciaEspectador> PreferenciasEspectador { get; set; } = null!;
        public DbSet<Partido> Partidos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Rol>().ToTable("roles");
            modelBuilder.Entity<UsuarioRol>().ToTable("usuariosroles");
            modelBuilder.Entity<Persona>().ToTable("personas");
            modelBuilder.Entity<Jugador>().ToTable("jugadores");
            modelBuilder.Entity<Entrenador>().ToTable("entrenadores");
            modelBuilder.Entity<Arbitro>().ToTable("arbitros");
            modelBuilder.Entity<Liga>().ToTable("ligas");
            modelBuilder.Entity<Temporada>().ToTable("temporadas");
            modelBuilder.Entity<Categoria>().ToTable("categorias");
            modelBuilder.Entity<Torneo>().ToTable("torneos");
            modelBuilder.Entity<ConfiguracionTorneo>().ToTable("configuracionestorneo");
            modelBuilder.Entity<Grupo>().ToTable("grupos");
            modelBuilder.Entity<Equipo>().ToTable("equipos");
            modelBuilder.Entity<EquipoTorneo>().ToTable("equipostorneos");
            modelBuilder.Entity<EquipoEntrenador>().ToTable("equiposentrenadores");
            modelBuilder.Entity<EquipoJugadorTorneo>().ToTable("equiposjugadorestorneos");
            modelBuilder.Entity<SolicitudJugadorEquipo>().ToTable("solicitudesjugadorequipo");
            modelBuilder.Entity<AuditoriaAccesos>().ToTable("auditoriaaccesos");
            modelBuilder.Entity<ReglaPersonalizadaTorneo>().ToTable("reglaspersonalizadastorneo");
            modelBuilder.Entity<PreferenciaEspectador>().ToTable("preferenciasespectador");
            modelBuilder.Entity<Partido>().ToTable("partidos");

            modelBuilder.HasDefaultSchema("public");

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLower());
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName()?.ToLower());
                }

                foreach (var fk in entity.GetForeignKeys())
                {
                    fk.SetConstraintName(fk.GetConstraintName()?.ToLower());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName()?.ToLower());
                }
            }

            ConfigureUsuario(modelBuilder);
            ConfigurePersona(modelBuilder);
            ConfigureTorneo(modelBuilder);
            ConfigureEquipo(modelBuilder);
            ConfigureSolicitudes(modelBuilder);
            ConfigureReglas(modelBuilder);
            ConfigurePreferencias(modelBuilder);
            ConfigurePartido(modelBuilder);
        }

        private void ConfigureUsuario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();

                // Relación 1:1 con Persona
                entity.HasOne(e => e.Persona)
                    .WithOne(p => p.Usuario)
                    .HasForeignKey<Persona>(p => p.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación Many-to-Many con Roles
                entity.HasMany(e => e.UsuariosRoles)
                    .WithOne(ur => ur.Usuario)
                    .HasForeignKey(ur => ur.UsuarioId);
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            modelBuilder.Entity<UsuarioRol>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UsuarioId, e.RolId }).IsUnique();
            });

            modelBuilder.Entity<AuditoriaAccesos>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigurePersona(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApellidoPaterno).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);

                // Relación 1:1 con Jugador
                entity.HasOne(e => e.Jugador)
                    .WithOne(j => j.Persona)
                    .HasForeignKey<Jugador>(j => j.PersonaId);

                // Relación 1:1 con Entrenador
                entity.HasOne(e => e.Entrenador)
                    .WithOne(ent => ent.Persona)
                    .HasForeignKey<Entrenador>(ent => ent.PersonaId);

                // Relación 1:1 con Arbitro
                entity.HasOne(e => e.Arbitro)
                    .WithOne(a => a.Persona)
                    .HasForeignKey<Arbitro>(a => a.PersonaId);
            });

            modelBuilder.Entity<Jugador>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PersonaId).IsUnique();
            });

            modelBuilder.Entity<Entrenador>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PersonaId).IsUnique();
            });

            modelBuilder.Entity<Arbitro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PersonaId).IsUnique();
                entity.Property(e => e.NumeroLicencia).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NumeroLicencia).IsUnique();
            });
        }

        private void ConfigureTorneo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Torneo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Formato).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);

                // Relación con ConfiguracionTorneo
                entity.HasOne(e => e.ConfiguracionTorneo)
                    .WithOne(c => c.Torneo)
                    .HasForeignKey<ConfiguracionTorneo>(c => c.TorneoId);
            });

            modelBuilder.Entity<ConfiguracionTorneo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TorneoId).IsUnique();
            });

            modelBuilder.Entity<Liga>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            modelBuilder.Entity<Temporada>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Nombre, e.Anio }).IsUnique();
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });
        }

        private void ConfigureReglas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReglaPersonalizadaTorneo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Torneo)
                    .WithMany()
                    .HasForeignKey(e => e.TorneoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.TorneoId, e.Orden });
            });
        }

        private void ConfigureEquipo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Equipo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            modelBuilder.Entity<EquipoTorneo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.EquipoId, e.TorneoId }).IsUnique();
            });

            modelBuilder.Entity<EquipoJugadorTorneo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.JugadorId, e.TorneoId }).IsUnique();
                entity.HasIndex(e => new { e.EquipoId, e.TorneoId, e.NumeroCamiseta }).IsUnique();
            });

            modelBuilder.Entity<EquipoEntrenador>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }

        private void ConfigureSolicitudes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SolicitudJugadorEquipo>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Jugador)
                    .WithMany()
                    .HasForeignKey(e => e.JugadorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Equipo)
                    .WithMany()
                    .HasForeignKey(e => e.EquipoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Torneo)
                    .WithMany()
                    .HasForeignKey(e => e.TorneoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.EntrenadorResponsable)
                    .WithMany()
                    .HasForeignKey(e => e.EntrenadorResponsableId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        // ← NUEVO MÉTODO
        private void ConfigurePreferencias(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PreferenciaEspectador>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UsuarioId)
                    .IsRequired();
                
                entity.Property(e => e.NotificacionesEmail)
                    .HasDefaultValue(true);
                
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("NOW()");
                
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.EquipoFavorito)
                    .WithMany()
                    .HasForeignKey(e => e.EquipoFavoritoId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasIndex(e => e.UsuarioId).IsUnique();
            });
        }

        private void ConfigurePartido(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Partido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Sede).HasMaxLength(200);

                entity.HasOne(e => e.Torneo)
                    .WithMany()
                    .HasForeignKey(e => e.TorneoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Grupo)
                    .WithMany()
                    .HasForeignKey(e => e.GrupoId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.EquipoLocal)
                    .WithMany()
                    .HasForeignKey(e => e.EquipoLocalId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EquipoVisitante)
                    .WithMany()
                    .HasForeignKey(e => e.EquipoVisitanteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Arbitro)
                    .WithMany()
                    .HasForeignKey(e => e.ArbitroId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.TorneoId, e.Fecha });
                entity.HasIndex(e => e.ArbitroId);
            });
        }
    }
}