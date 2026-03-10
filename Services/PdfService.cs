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
    }
}