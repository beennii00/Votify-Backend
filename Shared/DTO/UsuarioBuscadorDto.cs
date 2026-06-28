using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Shared.DTO
{
    public class UsuarioBuscadorDto
    {
		public int Id { get; set; }
		public string Nombre { get; set; }
		public string Dni { get; set; }
	}
}
