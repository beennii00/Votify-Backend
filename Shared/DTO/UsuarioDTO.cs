using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Shared.Attributes;

namespace Shared.DTO
{

    public class RegisterUsuarioDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }
        
        public string TipoDocumento { get; set; } = "DNI";
        
        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [DocumentoIdentidad("TipoDocumento")]
        public string Dni { get; set; }
        
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Contrasenya { get; set; }
        
        [Required(ErrorMessage = "Debes confirmar la contraseña.")]
        [Compare("Contrasenya", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; }
        
        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Rol { get; set; } 
    }

    public class LoginUsuarioDTO
    {
        public string TipoDocumento { get; set; } = "DNI";

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [DocumentoIdentidad("TipoDocumento")]
        public string Dni { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contrasenya { get; set; }
    }

    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Dni { get; set; }
        public string Rol { get; set; }
        public string? Avatar { get; set; }
    }

    public class UpdateUsuarioDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string? Contrasenya { get; set; }

        [Compare("Contrasenya", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmarContrasenya { get; set; }

        public string? Avatar { get; set; }
    }

    public class CambiarContrasenyaDTO
    {
        public string TipoDocumento { get; set; } = "DNI";

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [DocumentoIdentidad("TipoDocumento")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string NuevaContrasenya { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar la nueva contraseña.")]
        [Compare("NuevaContrasenya", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasenya { get; set; } = string.Empty;
    }
}
