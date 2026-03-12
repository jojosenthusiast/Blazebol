# FRONTEND_GUIDE.md — ControlTorneosFutbol

> Guía completa de UI/UX: paleta de colores, componentes, patrones Razor, convenciones CSS.

---

## 1. Paleta de Colores (Dark Theme)

### Variables CSS principales (`wwwroot/css/site.css`)

```css
:root {
  --color-bg-primary:      #1a1a2e;   /* Fondo principal de página */
  --color-bg-secondary:    #16213e;   /* Cards, sidebar, paneles */
  --color-bg-tertiary:     #0f3460;   /* Hover, filas de tabla */
  --color-accent:          #a3ff12;   /* Verde neón — acción primaria */
  --color-accent-hover:    #8ce00f;   /* Hover del acento */
  --color-text-primary:    #e0e0e0;   /* Texto principal */
  --color-text-secondary:  #a0a0b0;   /* Texto secundario / labels */
  --color-text-muted:      #6c6c80;   /* Texto terciario / placeholders */
  --color-border:          #2a2a4a;   /* Bordes de cards y tablas */
  --color-danger:          #ff2d55;   /* Rojo — eliminar, rechazar */
  --color-warning:         #ffd60a;   /* Amarillo — advertencia */
  --color-info:            #5ac8fa;   /* Azul — info, editar */
}
```

### Colores por torneo (determinístico por ID)
```javascript
const TorneoColors = ["#a3ff12","#5ac8fa","#ff9500","#bf5af2","#ff2d55","#34c759","#ffd60a"];
// Uso: TorneoColors[Math.Abs(torneoId) % 7]
```

---

## 2. Sistema de Botones

### Clases disponibles

| Clase | Color | Uso |
|-------|-------|-----|
| `btn btn-primary` | Verde (`--color-accent`) | Acción principal (guardar, confirmar) |
| `btn btn-info` | Azul (`#5ac8fa`) | Editar, ver, exportar PDF |
| `btn btn-warning` | Naranja (`#ff9500`) | Acciones de precaución, verificar |
| `btn btn-danger` | Rojo (`#ff2d55`) | Eliminar, rechazar, desactivar |
| `btn btn-secondary` | Gris oscuro | Cancelar, volver |
| `btn btn-outline` | Borde accent | Secundario menos prominente |
| `btn btn-link` | Sin fondo | Limpiar filtros, acciones menores |

### Modificadores

```html
<!-- Botón con ícono (Bootstrap Icons + texto) -->
<button class="btn btn-primary btn-icon">
    <i class="bi bi-plus-lg"></i> Crear
</button>

<!-- Botón pequeño -->
<button class="btn btn-sm btn-info btn-icon">
    <i class="bi bi-pencil"></i> Editar
</button>

<!-- Botón deshabilitado visualmente (CSS, no HTML disabled) -->
<div class="action-card-disabled">...</div>
```

### Botón PDF (siempre azul con ícono)
```html
<a asp-page="/Admin/Torneos/Index" asp-page-handler="ExportPdf"
   class="btn btn-info btn-icon">
    <i class="bi bi-file-earmark-pdf"></i> Exportar PDF
</a>
```

---

## 3. Layout General

### Estructura de página

```
[_Layout.cshtml]
  └─ <body>
       ├─ <nav class="top-bar">         ← Logo + nombre usuario + logout
       ├─ <aside class="sidebar">       ← _Sidebar.cshtml (role-aware)
       └─ <main class="main-content">
            └─ @RenderBody()            ← Contenido de cada página
```

### Dashboard layout típico

```html
<div class="page-header">
    <div>
        <h1 class="page-title">Título de la Página</h1>
        <p class="page-subtitle">Subtítulo descriptivo</p>
    </div>
    <a asp-page="..." class="btn btn-primary btn-icon">
        <i class="bi bi-plus-lg"></i> Crear Nuevo
    </a>
</div>

<!-- Sección de stats -->
<div class="stats-grid">
    <div class="stat-card">
        <i class="bi bi-trophy stat-icon"></i>
        <div class="stat-value">@Model.Total</div>
        <div class="stat-label">Torneos Activos</div>
    </div>
</div>

<!-- Filtros (formulario GET horizontal) -->
<form method="get" class="filter-panel">
    <div class="filter-row">
        <select name="estado" class="form-select">...</select>
        <button type="submit" class="btn btn-primary btn-icon">
            <i class="bi bi-search"></i> Filtrar
        </button>
        <a asp-page="." class="btn btn-link btn-sm btn-icon">
            <i class="bi bi-x-circle"></i> Limpiar
        </a>
    </div>
</form>

<!-- Tabla de datos -->
<div class="table-container">
    <table class="data-table">
        <thead><tr><th>Col1</th><th>Acciones</th></tr></thead>
        <tbody>
            @foreach (var item in Model.Items) {
                <tr>
                    <td>@item.Propiedad</td>
                    <td>
                        <div class="action-buttons">
                            <a asp-page="./Editar" class="btn btn-sm btn-info btn-icon">
                                <i class="bi bi-pencil"></i>
                            </a>
                        </div>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

---

## 4. Sidebar (`Pages/Shared/_Sidebar.cshtml`)

### Detección de página activa
```csharp
@{
    var currentPath = Context.Request.Path.ToString().ToLower();
}
<a class="nav-item @(currentPath.Contains("/admin/torneos") ? "active" : "")">
```

### Estructura de nav-item
```html
<a asp-page="/Admin/Torneos/Index"
   class="nav-item @(currentPath.Contains("/admin/torneos") ? "active" : "")">
    <i class="bi bi-trophy nav-icon"></i>
    <span class="nav-text">Torneos</span>
</a>
```

### Secciones del sidebar
- `nav-section` → título de grupo (e.g., "GESTIÓN", "SISTEMA")
- `nav-divider` → línea separadora entre grupos

---

## 5. Componentes de Alerta / Notificación

### Auto-toast (site.js)
Las alertas con clases `.alert-success`, `.alert-error`, `.alert-warning` se convierten automáticamente en toasts SweetAlert2 al cargar la página.

```html
<!-- En el PageModel: Model.SuccessMessage = "Guardado exitosamente" -->
@if (!string.IsNullOrEmpty(Model.SuccessMessage))
{
    <div class="alert alert-success d-none">@Model.SuccessMessage</div>
}
@if (!string.IsNullOrEmpty(Model.ErrorMessage))
{
    <div class="alert alert-error d-none">@Model.ErrorMessage</div>
}
```

### SweetAlert2 — confirmación antes de submit
```html
<!-- En form -->
<form method="post" data-confirm-form
      data-confirm-title="¿Eliminar torneo?"
      data-confirm-text="Esta acción no se puede deshacer"
      data-confirm-icon="warning"
      data-confirm-color="#ff2d55">
    <button type="submit" class="btn btn-danger btn-icon">
        <i class="bi bi-trash"></i> Eliminar
    </button>
</form>
```

> El JS en `site.js` intercepta `[data-confirm-form]` y muestra el dialog antes de hacer submit.

### SweetAlert2 — toast simple (JS directo)
```javascript
Swal.fire({
    toast: true,
    position: 'top-end',
    icon: 'success',      // 'error', 'warning', 'info'
    title: 'Mensaje',
    showConfirmButton: false,
    timer: 3000
});
```

---

## 6. Badges y Estados

### Badges de estado de partido
```html
<span class="badge badge-programado">Programado</span>   <!-- azul #5ac8fa -->
<span class="badge badge-encurso">En Curso</span>         <!-- naranja #ff9500 -->
<span class="badge badge-finalizado">Finalizado</span>    <!-- verde/rojo según resultado -->
<span class="badge badge-cancelado">Cancelado</span>      <!-- gris #666 -->
```

### Badges genéricos
```html
<span class="badge badge-success">Activo</span>
<span class="badge badge-warning">Pendiente</span>
<span class="badge badge-danger">Rechazado</span>
<span class="badge badge-info">Información</span>
<span class="badge badge-outline">Texto neutro</span>
```

---

## 7. Action Cards (Dashboard)

```html
<div class="actions-grid">
    <!-- Card habilitada -->
    <a asp-page="/Jugador/BuscarEquipos" class="action-card">
        <i class="bi bi-search action-icon"></i>
        <div class="action-title">Buscar Equipos</div>
        <div class="action-desc">Encuentra tu equipo ideal</div>
    </a>

    <!-- Card deshabilitada -->
    <div class="action-card action-card-disabled">
        <i class="bi bi-bar-chart action-icon"></i>
        <div class="action-title">Estadísticas</div>
        <div class="action-desc">Próximamente</div>
    </div>
</div>
```

---

## 8. Formularios

### Patrón estándar
```html
<div class="form-container">
    <form method="post">
        <div asp-validation-summary="ModelOnly" class="alert alert-error"></div>

        <div class="form-group">
            <label asp-for="Input.Nombre" class="form-label">Nombre</label>
            <input asp-for="Input.Nombre" class="form-control" placeholder="..."/>
            <span asp-validation-for="Input.Nombre" class="field-error"></span>
        </div>

        <div class="form-actions">
            <button type="submit" class="btn btn-primary btn-icon">
                <i class="bi bi-floppy"></i> Guardar
            </button>
            <a asp-page="./Index" class="btn btn-secondary btn-icon">
                <i class="bi bi-arrow-left"></i> Cancelar
            </a>
        </div>
    </form>
</div>
```

### Select con datos de BD
```html
<select asp-for="Input.TorneoId" asp-items="Model.TorneoSelectList"
        class="form-select">
    <option value="">-- Seleccionar torneo --</option>
</select>
```

### Upload de imagen (logo equipo)
```html
<input type="file" asp-for="Input.Logo" class="form-control"
       accept="image/png,image/jpeg,image/svg+xml"/>
```

---

## 9. Empty States

```html
<div class="empty-state">
    <i class="bi bi-calendar-x empty-icon"></i>
    <p>No hay partidos programados</p>
    <a asp-page="/Admin/Partidos/Crear" class="btn btn-primary btn-icon">
        <i class="bi bi-plus-lg"></i> Crear Partido
    </a>
</div>
```

```css
/* site.css */
.empty-icon { font-size: 3rem; color: var(--color-text-muted);
              margin-bottom: 16px; display: block; }
.empty-state { text-align: center; padding: 48px;
               color: var(--color-text-secondary); }
```

---

## 10. Widget Próximos Partidos (Dashboard)

```html
<div class="recent-section">
    <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:16px;">
        <h2 class="section-title">Próximos Partidos</h2>
        <a asp-page="/Calendario/Index" class="btn btn-sm btn-info btn-icon">
            <i class="bi bi-calendar3"></i> Ver Calendario
        </a>
    </div>
    <div class="match-mini-list">
        @foreach (var p in Model.ProximosPartidos)
        {
            <div class="match-mini-card">
                <div class="match-mini-dot" style="background:@p.ColorTorneo;"></div>
                <div class="match-mini-body">
                    <div class="match-mini-teams">
                        <strong>@p.AbrevLocal</strong> vs <strong>@p.AbrevVisitante</strong>
                    </div>
                    <div class="match-mini-meta">
                        @p.Fecha.ToString("dd/MM HH:mm") · @p.NombreTorneo
                    </div>
                </div>
                <a asp-page="/Admin/Partidos/Index" class="btn btn-sm btn-info btn-icon">
                    <i class="bi bi-eye"></i>
                </a>
            </div>
        }
    </div>
</div>
```

---

## 11. Bootstrap Icons — Referencia Rápida por Concepto

| Concepto | Ícono |
|---------|-------|
| Crear / agregar | `bi-plus-lg` |
| Editar / guardar | `bi-pencil` / `bi-floppy` |
| Eliminar / borrar | `bi-trash` |
| Ver detalle | `bi-eye` |
| Volver / cancelar | `bi-arrow-left` |
| Buscar / filtrar | `bi-search` |
| Limpiar filtros | `bi-x-circle` |
| PDF / exportar | `bi-file-earmark-pdf` |
| Calendario | `bi-calendar3` |
| Partido sin datos | `bi-calendar-x` |
| Verificar / aprobar | `bi-clipboard-check` |
| Árbitro | `bi-whistle` |
| Jugador | `bi-person-circle` |
| Entrenador | `bi-person-badge-fill` |
| Equipo / fútbol | `bi-dribbble` |
| Torneo | `bi-trophy` / `bi-trophy-fill` |
| Seguridad / lock | `bi-lock-fill` / `bi-unlock` |
| Bloqueo | `bi-person-slash` |
| Escudo | `bi-shield-check` / `bi-shield-x` |
| Sede / ubicación | `bi-geo-alt` |
| Árbitro persona | `bi-person` |
| Estadísticas | `bi-bar-chart` |
| Notificación | `bi-bell` |
| Correo | `bi-envelope` |
| Configuración | `bi-gear` |

---

## 12. Convenciones CSS Globales

- **Nombrado:** kebab-case para clases (`.action-card`, `.stat-card`, `.match-mini-list`)
- **Variables:** `--color-*` para colores, nunca hex inline en reglas CSS
- **Inline styles:** Permitidos SOLO para valores dinámicos (ej: `style="background:@color;"`)
- **Responsive:** Mobile-first con Bootstrap breakpoints (`col-md-*`, `col-lg-*`)
- **Secciones de página:** `.page-header`, `.stats-grid`, `.filter-panel`, `.table-container`, `.recent-section`

---

## 13. Panel de Partido en Vivo *(Sprint 4)*

### Layout del panel
```html
<!-- Fondo oscuro principal -->
<div class="live-match-panel" style="background:#1a1a2e; border-radius:12px; padding:24px;">

    <!-- Header: Estado + Tiempo -->
    <div class="live-header" style="text-align:center; margin-bottom:20px;">
        <span class="badge badge-encurso">EN VIVO</span>
        <span id="minuto" class="live-minute">45'</span>
    </div>

    <!-- Marcador central -->
    <div class="live-scoreboard" style="display:flex; align-items:center; justify-content:center; gap:24px;">
        <div class="live-team">
            <img src="@Model.LogoLocal" class="team-logo-lg"/>
            <div class="live-team-name">@Model.NombreLocal</div>
        </div>
        <div class="live-score">
            <span id="goles-local" class="score-number">@Model.GolesLocal</span>
            <span class="score-separator">-</span>
            <span id="goles-visitante" class="score-number">@Model.GolesVisitante</span>
        </div>
        <div class="live-team">
            <img src="@Model.LogoVisitante" class="team-logo-lg"/>
            <div class="live-team-name">@Model.NombreVisitante</div>
        </div>
    </div>

    <!-- Botones de acción rápida -->
    <div class="live-actions" style="display:flex; gap:12px; justify-content:center; margin-top:24px;">
        <button class="btn btn-success live-action-btn" onclick="abrirModal('Gol')">
            <i class="bi bi-circle-fill"></i> GOL
        </button>
        <button class="btn live-action-btn" style="background:#ffd60a; color:#000" onclick="abrirModal('TarjetaAmarilla')">
            <i class="bi bi-square-fill"></i> AMARILLA
        </button>
        <button class="btn btn-danger live-action-btn" onclick="abrirModal('TarjetaRoja')">
            <i class="bi bi-square-fill"></i> ROJA
        </button>
        <button class="btn btn-info live-action-btn" onclick="abrirModal('Cambio')">
            <i class="bi bi-arrow-left-right"></i> CAMBIO
        </button>
    </div>

    <!-- Lista de incidencias -->
    <div id="incidencias-list" class="incidencias-timeline" style="margin-top:24px;">
        <!-- Generada dinámicamente por JS -->
    </div>

    <!-- Finalizar partido -->
    <div style="text-align:center; margin-top:32px;">
        <form method="post" asp-page-handler="Finalizar" data-confirm-form
              data-confirm-title="¿Finalizar partido?"
              data-confirm-text="Esta acción no se puede deshacer"
              data-confirm-icon="warning" data-confirm-color="#ff2d55">
            <input type="hidden" name="partidoId" value="@Model.PartidoId"/>
            <button type="submit" class="btn btn-danger btn-lg btn-icon">
                <i class="bi bi-stop-fill"></i> Finalizar Partido
            </button>
        </form>
    </div>
</div>
```

### CSS para el panel en vivo
```css
.live-minute { font-size: 2rem; font-weight: 700; color: var(--color-accent); margin-left: 12px; }
.score-number { font-size: 4rem; font-weight: 900; color: var(--color-text-primary); }
.score-separator { font-size: 3rem; color: var(--color-text-muted); margin: 0 8px; }
.team-logo-lg { width: 64px; height: 64px; object-fit: contain; }
.live-team-name { font-size: 0.9rem; color: var(--color-text-secondary); text-align: center; margin-top: 8px; }
.live-action-btn { font-size: 1rem; font-weight: 700; padding: 12px 20px; border-radius: 8px; }
.incidencias-timeline { border-left: 2px solid var(--color-border); padding-left: 16px; }
.incidencia-item { display: flex; align-items: center; gap: 12px; padding: 8px 0;
                   border-bottom: 1px solid var(--color-border); color: var(--color-text-primary); }
.incidencia-minuto { color: var(--color-accent); font-weight: 700; min-width: 40px; }
```

### Modal selector de jugador
```html
<!-- Modal overlay oscuro -->
<div id="modal-jugador" class="modal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content" style="background: var(--color-bg-secondary);">
            <div class="modal-header">
                <h5 class="modal-title" id="modal-titulo">Registrar Gol</h5>
            </div>
            <div class="modal-body">
                <!-- Paso 1: Seleccionar equipo -->
                <div id="paso-equipo">
                    <button class="btn btn-outline w-100 mb-2" onclick="selEquipo(@Model.EquipoLocalId, '@Model.NombreLocal')">
                        @Model.NombreLocal (Local)
                    </button>
                    <button class="btn btn-outline w-100" onclick="selEquipo(@Model.EquipoVisitanteId, '@Model.NombreVisitante')">
                        @Model.NombreVisitante (Visitante)
                    </button>
                </div>
                <!-- Paso 2: Seleccionar jugador (cargado por AJAX) -->
                <div id="paso-jugador" style="display:none;">
                    <input type="number" id="input-minuto" class="form-control mb-2" placeholder="Minuto" min="1" max="120"/>
                    <div id="lista-jugadores" class="player-list"></div>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 13b. Badges Sprint 4

### Badge Reprogramado
```html
<span class="badge badge-reprogramado">Reprogramado</span>
```
```css
.badge-reprogramado { background: rgba(191, 90, 242, 0.15); color: #bf5af2; border: 1px solid #bf5af2; }
```

### Bell de notificaciones (sidebar)
```html
<!-- En _Sidebar.cshtml footer o nav -->
<div class="notif-bell" style="position:relative; cursor:pointer;">
    <i class="bi bi-bell nav-icon"></i>
    <span id="notif-badge" class="notif-count" style="display:none;"></span>
</div>
```
```css
.notif-count {
    position: absolute; top: -4px; right: -4px;
    background: var(--color-danger); color: #fff;
    border-radius: 50%; width: 18px; height: 18px;
    font-size: 10px; font-weight: 700;
    display: inline-flex; align-items: center; justify-content: center;
}
```

---

## 14. Librerías CDN Cargadas en _Layout.cshtml

```html
<!-- Bootstrap 5 CSS -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<!-- Bootstrap Icons -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css" rel="stylesheet">
<!-- SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<!-- jQuery -->
<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
<!-- jQuery Validation -->
<script src="https://cdn.jsdelivr.net/npm/jquery-validation@1.20.0/dist/jquery.validate.min.js"></script>
```
