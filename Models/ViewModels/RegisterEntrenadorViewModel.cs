using System;
using System.ComponentModel.DataAnnotations;

namespace ControlTorneosFutbol.Models.ViewModels
{
    public class RegisterEntrenadorViewModel
    {
        // Datos personales del entrenador
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido paterno no puede exceder 100 caracteres")]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
        [Display(Name = "Apellido Materno")]
        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula y un número")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(50, ErrorMessage = "El número de licencia no puede exceder 50 caracteres")]
        [Display(Name = "Número de Licencia (Opcional)")]
        public string? NumeroLicencia { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Expiración de Licencia")]
        public DateTime? FechaExpiracionLicencia { get; set; }

        // Datos del equipo (se crea durante el registro)
        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre del equipo no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del Equipo")]
        public string NombreEquipo { get; set; } = string.Empty;

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Formato de color inválido. Use formato hexadecimal (ej: #FF5733)")]
        [Display(Name = "Color Primario del Equipo")]
        public string? ColorPrimario { get; set; }

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Formato de color inválido. Use formato hexadecimal (ej: #FF5733)")]
        [Display(Name = "Color Secundario del Equipo")]
        public string? ColorSecundario { get; set; }

        [StringLength(200, ErrorMessage = "La sede no puede exceder 200 caracteres")]
        [Display(Name = "Sede/Cancha del Equipo")]
        public string? Sede { get; set; }

        [StringLength(3, MinimumLength = 2, ErrorMessage = "La abreviación debe tener entre 2 y 3 letras")]
        [RegularExpression(@"^[A-Za-zÁáÉéÍíÓóÚúÑñ]+$", ErrorMessage = "Solo se permiten letras")]
        [Display(Name = "Abreviación (2-3 letras)")]
        public string? Abreviacion { get; set; }

        [Display(Name = "Tipo de Equipo")]
        public string TipoEquipo { get; set; } = "Libre";

        // Filled by the page model after processing the uploaded file
        public string? LogoPath { get; set; }
    }
}