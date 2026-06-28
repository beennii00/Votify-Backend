using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class AsignarConcursanteDto
    {
		public int ProyectoId { get; set; }

		public List<int> ConcursantesIds { get; set; } = new List<int>();
	}
}
