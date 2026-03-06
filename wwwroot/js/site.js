// ============================================================
// site.js — Funciones globales de UI
// ============================================================

// ------------------------------------------------------------
// ctConfirm — Reemplaza confirm() nativo con SweetAlert2
// uso: ctConfirm('¿Eliminar este torneo?', () => form.submit())
// ------------------------------------------------------------
function ctConfirm(message, onConfirm, options) {
    options = options || {};
    Swal.fire({
        title: options.title || '¿Estás seguro?',
        text: message,
        icon: options.icon || 'warning',
        background: '#1a1a1a',
        color: '#ffffff',
        showCancelButton: true,
        confirmButtonColor: options.confirmColor || '#ff3b30',
        cancelButtonColor: '#444444',
        confirmButtonText: options.confirmText || 'Sí, continuar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then(function (result) {
        if (result.isConfirmed && typeof onConfirm === 'function') {
            onConfirm();
        }
    });
}

// ------------------------------------------------------------
// ctConfirmForm — Variante para formularios: pasa el form en vez de callback
// uso en botón: onclick="ctConfirmForm(event, this, '¿Eliminar?')"
// ------------------------------------------------------------
function ctConfirmForm(event, btn, message, options) {
    event.preventDefault();
    var form = btn.closest('form');
    ctConfirm(message, function () {
        // Mostrar spinner en el botón antes de enviar
        ctLoadingBtn(btn);
        form.submit();
    }, options);
}

// ------------------------------------------------------------
// ctLoadingBtn — Agrega spinner al botón al hacer submit
// uso: btn.addEventListener('click', () => ctLoadingBtn(btn))
// ------------------------------------------------------------
function ctLoadingBtn(btn, loadingText) {
    btn.classList.add('btn-loading');
    var iconHtml = '<span class="spinner-sm"></span> ';
    btn.innerHTML = iconHtml + (loadingText || 'Cargando...');
}

// ------------------------------------------------------------
// Auto-toast desde alertas en la página (se ejecuta al cargar)
// Lee .alert-success y muestra toast; mantiene .alert-error visible
// ------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    if (typeof Swal === 'undefined') return;

    var alertConfigs = [
        { selector: '.alert-success', icon: 'success', color: '#34c759' },
        { selector: '.alert-error',   icon: 'error',   color: '#ff3b30' },
        { selector: '.alert-warning', icon: 'warning', color: '#ff9500' },
        { selector: '.alert-info',    icon: 'info',    color: '#5ac8fa' }
    ];

    alertConfigs.forEach(function (cfg) {
        var el = document.querySelector(cfg.selector);
        if (!el) return;
        var msg = el.textContent.trim();
        if (!msg) return;
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: cfg.icon,
            title: msg,
            showConfirmButton: false,
            timer: 4500,
            timerProgressBar: true,
            background: '#1a1a1a',
            color: cfg.color,
            iconColor: cfg.color
        });
        el.style.display = 'none';
    });
});
