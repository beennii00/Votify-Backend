using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class ConfigurarFechasVotacionDto
    {
		public int VotacionId { get; set; }
		public DateTime FechaInicio { get; set; }
		public DateTime FechaFin { get; set; }
	}
}
