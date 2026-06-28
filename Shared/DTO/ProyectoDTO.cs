using Shared.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace Shared.DTO
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "tipoProyecto")]
    [JsonDerivedType(typeof(EstandarProyectoDTO), typeDiscriminator: "estandar")]
    // [JsonDerivedType(typeof(PremiumProyectoDTO), typeDiscriminator: "premium")] // Ejemplos futuros
    public abstract class ProyectoDTO
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del proyecto es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción del proyecto es obligatoria")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El proyecto debe estar asociado a un evento")]
        public int EventoId { get; set; }
    }

    public class EstandarProyectoDTO : ProyectoDTO
    {
       
    }
}
