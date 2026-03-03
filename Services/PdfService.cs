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