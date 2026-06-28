using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shared.Attributes
{
    public class DocumentoIdentidadAttribute : ValidationAttribute
    {
        private readonly string _tipoProperty;

        public DocumentoIdentidadAttribute(string tipoProperty)
        {
            _tipoProperty = tipoProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(_tipoProperty);
            if (propertyInfo == null) return ValidationResult.Success;

            var tipoDocumento = propertyInfo.GetValue(validationContext.ObjectInstance)?.ToString();
            var documento = value?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(documento))
                return ValidationResult.Success; // [Required] handles empty

            if (tipoDocumento == "DNI" && !Regex.IsMatch(documento, @"^\d{8}[a-zA-Z]$"))
                return new ValidationResult("Formato de DNI inválido (8 números y 1 letra).");
            
            if (tipoDocumento == "NIE" && !Regex.IsMatch(documento, @"^[XYZxyz]\d{7}[a-zA-Z]$"))
                return new ValidationResult("Formato de NIE inválido (ej: X1234567A).");
            
            if (tipoDocumento == "Pasaporte" && !Regex.IsMatch(documento, @"^[a-zA-Z0-9]{6,9}$"))
                return new ValidationResult("Formato de Pasaporte inválido (6-9 caracteres alfanuméricos).");

            return ValidationResult.Success;
        }
    }
}
