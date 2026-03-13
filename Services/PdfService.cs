using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ControlTorneosFutbol.Data.Entities;

namespace ControlTorneosFutbol.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateUsuariosReport(List<Usuario> usuarios)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Listado de Usuarios del Sistema")
                        .SemiBold().FontSize(20).FontColor(Colors.Black);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Nombre");
                                header.Cell().Element(CellStyle).Text("Email");
                                header.Cell().Element(CellStyle).Text("Rol");
                                header.Cell().Element(CellStyle).Text("Estado");
                                header.Cell().Element(CellStyle).Text("Fecha Registro");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold())
                                        .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var usuario in usuarios)
                            {
                                var persona = usuario.Persona;
                                var nombreCompleto = persona != null 
                                    ? $"{persona.Nombre} {persona.ApellidoPaterno}"
                                    : usuario.Email;

                                var rol = usuario.UsuariosRoles?.FirstOrDefault()?.Rol?.Nombre ?? "Sin rol";

                                table.Cell().Element(CellStyle).Text(nombreCompleto);
                                table.Cell().Element(CellStyle).Text(usuario.Email);
                                table.Cell().Element(CellStyle).Text(rol);
                                table.Cell().Element(CellStyle).Text(usuario.Activo ? "Activo" : "Inactivo");
                                table.Cell().Element(CellStyle).Text(usuario.FechaCreacion.ToString("dd/MM/yyyy"));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                            x.Span(" - Generado el ");
                            x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateTorneosReport(List<Torneo> torneos)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Listado de Torneos")
                        .SemiBold().FontSize(20).FontColor(Colors.Black);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Nombre");
                                header.Cell().Element(CellStyle).Text("Liga");
                                header.Cell().Element(CellStyle).Text("Categoría");
                                header.Cell().Element(CellStyle).Text("Formato");
                                header.Cell().Element(CellStyle).Text("Estado");
                                header.Cell().Element(CellStyle).Text("Fecha Inicio");
                                header.Cell().Element(CellStyle).Text("Fecha Fin");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold())
                                        .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var torneo in torneos)
                            {
                                table.Cell().Element(CellStyle).Text(torneo.Nombre);
                                table.Cell().Element(CellStyle).Text(torneo.Liga?.Nombre ?? "N/A");
                                table.Cell().Element(CellStyle).Text(torneo.Categoria?.Nombre ?? "N/A");
                                table.Cell().Element(CellStyle).Text(torneo.Formato);
                                table.Cell().Element(CellStyle).Text(torneo.Estado);
                                table.Cell().Element(CellStyle).Text(torneo.FechaInicio.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(torneo.FechaFin.ToString("dd/MM/yyyy"));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerarReglamentoPdf(Torneo torneo, ConfiguracionTorneo? config)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text($"REGLAMENTO — {torneo.Nombre.ToUpper()}")
                            .SemiBold().FontSize(18).FontColor(Colors.Black);
                        col.Item().Text($"{torneo.Liga?.Nombre} · {torneo.Categoria?.Nombre} · {torneo.Formato}")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("").FontSize(4);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(12);

                        // Período
                        col.Item().Text("1. PERÍODO DEL TORNEO").SemiBold().FontSize(13);
                        col.Item().Text($"Inicio: {torneo.FechaInicio:dd/MM/yyyy}    Fin: {torneo.FechaFin:dd/MM/yyyy}");

                        // Equipos
                        col.Item().Text("2. EQUIPOS").SemiBold().FontSize(13);
                        col.Item().Text($"Mínimo de equipos: {torneo.NumeroEquiposMinimo}    Máximo de equipos: {torneo.NumeroEquiposMaximo}");

                        // Sistema de puntuación
                        if (config != null)
                        {
                            col.Item().Text("3. SISTEMA DE PUNTUACIÓN").SemiBold().FontSize(13);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });
                                void Row(string label, string val)
                                {
                                    table.Cell().PaddingVertical(3).Text(label);
                                    table.Cell().PaddingVertical(3).AlignRight().Text(val).SemiBold();
                                }
                                Row("Victoria", $"{config.PuntosPorVictoria} pts");
                                Row("Empate",   $"{config.PuntosPorEmpate} pts");
                                Row("Derrota",  $"{config.PuntosPorDerrota} pts");
                            });

                            col.Item().Text("4. CRITERIOS DE DESEMPATE").SemiBold().FontSize(13);
                            var criterios = new[] { config.CriterioDesempate1, config.CriterioDesempate2, config.CriterioDesempate3, config.CriterioDesempate4 }
                                .Where(c => !string.IsNullOrEmpty(c)).Select((c, i) => $"{i + 1}. {c}");
                            col.Item().Text(string.Join("    ", criterios));

                            col.Item().Text("5. DISCIPLINA").SemiBold().FontSize(13);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => { c.RelativeColumn(4); c.RelativeColumn(2); });
                                void Row(string label, string val)
                                {
                                    table.Cell().PaddingVertical(3).Text(label);
                                    table.Cell().PaddingVertical(3).AlignRight().Text(val).SemiBold();
                                }
                                Row("Tarjetas amarillas para suspensión", $"{config.TarjetasAmarillasParaSuspension}");
                                Row("Partidos de suspensión por tarjeta roja", $"{config.PartidosSuspensionTarjetaRoja}");
                            });

                            col.Item().Text("6. PLANTILLA").SemiBold().FontSize(13);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => { c.RelativeColumn(4); c.RelativeColumn(2); });
                                void Row(string label, string val)
                                {
                                    table.Cell().PaddingVertical(3).Text(label);
                                    table.Cell().PaddingVertical(3).AlignRight().Text(val).SemiBold();
                                }
                                if (config.MaximoJugadoresConvocados.HasValue)
                                    Row("Máximo jugadores convocados", $"{config.MaximoJugadoresConvocados}");
                                Row("Mínimo jugadores titulares", $"{config.MinimoJugadoresTitulares}");
                            });
                        }
                        else
                        {
                            col.Item().Text("3. CONFIGURACIÓN").SemiBold().FontSize(13);
                            col.Item().Text("La configuración detallada del torneo aún no ha sido establecida por el organizador.")
                                .FontColor(Colors.Grey.Darken1);
                        }

                        if (!string.IsNullOrEmpty(torneo.Ubicacion))
                        {
                            col.Item().Text("7. SEDE").SemiBold().FontSize(13);
                            col.Item().Text(torneo.Ubicacion);
                            if (!string.IsNullOrEmpty(torneo.DireccionSede))
                                col.Item().Text(torneo.DireccionSede).FontColor(Colors.Grey.Darken1);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generado el ");
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        x.Span("  ·  Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public byte[] GenerateJugadoresReport(List<Jugador> jugadores)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Listado de Jugadores")
                        .SemiBold().FontSize(20).FontColor(Colors.Black);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Nombre");
                                header.Cell().Element(CellStyle).Text("Posición");
                                header.Cell().Element(CellStyle).Text("Nº Camiseta");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold())
                                        .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var jugador in jugadores)
                            {
                                var persona = jugador.Persona;
                                var nombreCompleto = persona != null 
                                    ? $"{persona.Nombre} {persona.ApellidoPaterno}"
                                    : "N/A";

                                table.Cell().Element(CellStyle).Text(nombreCompleto);
                                table.Cell().Element(CellStyle).Text(jugador.PosicionPreferida ?? "N/A");
                                table.Cell().Element(CellStyle).Text(jugador.NumeroCamiseta?.ToString() ?? "N/A");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerarReportePartidoPdf(ReportePartido reporte)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var p = reporte.Partido;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Reporte de Partido Oficial").SemiBold().FontSize(18).FontColor(Colors.Black);
                        col.Item().Text($"{p.Torneo?.Nombre ?? "Torneo"} — {p.Fecha:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(0.8f, Unit.Centimetre).Column(col =>
                    {
                        // Match info
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(60); c.RelativeColumn(); });
                            table.Cell().AlignCenter().Text(p.EquipoLocal?.Nombre ?? "Local").SemiBold().FontSize(14);
                            table.Cell().AlignCenter().Text($"{p.GolesLocal ?? 0} – {p.GolesVisitante ?? 0}").SemiBold().FontSize(18).FontColor(Colors.Black);
                            table.Cell().AlignCenter().Text(p.EquipoVisitante?.Nombre ?? "Visitante").SemiBold().FontSize(14);
                        });

                        col.Item().PaddingVertical(6).Row(row =>
                        {
                            row.RelativeItem().Text($"Sede: {p.Sede ?? "—"}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().AlignCenter().Text($"Árbitro: {reporte.Arbitro?.Persona?.Nombre} {reporte.Arbitro?.Persona?.ApellidoPaterno}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            row.RelativeItem().AlignRight().Text($"Estado: {reporte.Estado}").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(8).Text("Datos del Partido").SemiBold().FontSize(12);
                        col.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text($"Minutos jugados: {reporte.MinutosJugados ?? 90}").FontSize(9);
                            row.RelativeItem().Text($"Prórroga: {(reporte.HuboProrroga ? "Sí" : "No")}").FontSize(9);
                            row.RelativeItem().Text($"Penales: {(reporte.HuboPenales ? $"Sí ({reporte.GolesLocalPenales}–{reporte.GolesVisitantePenales})" : "No")}").FontSize(9);
                        });

                        if (!string.IsNullOrWhiteSpace(reporte.Observaciones))
                        {
                            col.Item().PaddingTop(10).Text("Observaciones del Árbitro").SemiBold().FontSize(12);
                            col.Item().PaddingTop(4).Text(reporte.Observaciones).FontSize(9).FontColor(Colors.Grey.Darken1);
                        }

                        if (reporte.Incidencias?.Any() == true)
                        {
                            col.Item().PaddingTop(10).Text("Incidencias").SemiBold().FontSize(12);
                            col.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(3); });
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Min.").SemiBold().FontSize(8);
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Tipo").SemiBold().FontSize(8);
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Jugador").SemiBold().FontSize(8);
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Equipo").SemiBold().FontSize(8);
                                });
                                foreach (var inc in reporte.Incidencias.OrderBy(i => i.Minuto))
                                {
                                    string jugador = inc.EjtJugador != null
                                        ? $"{inc.EjtJugador.Jugador?.Persona?.Nombre} {inc.EjtJugador.Jugador?.Persona?.ApellidoPaterno}" : "—";
                                    table.Cell().Padding(4).Text(inc.Minuto.HasValue ? $"{inc.Minuto}'" : "—").FontSize(8);
                                    table.Cell().Padding(4).Text(inc.Tipo).FontSize(8);
                                    table.Cell().Padding(4).Text(jugador).FontSize(8);
                                    table.Cell().Padding(4).Text(inc.Equipo?.Nombre ?? "—").FontSize(8);
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        x.Span($" — Generado {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}