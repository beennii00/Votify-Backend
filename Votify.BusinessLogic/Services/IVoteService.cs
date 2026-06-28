using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface IVoteService
    {
		Task<bool> ConfigurarFechasVotacionAsync(Shared.DTO.ConfigurarFechasVotacionDto dto);
		Task<Shared.DTO.VotacionDto?> ObtenerVotacionAsync(int id);
        Task<bool> EmitirVotoAsync(Shared.DTO.EmitirVotoDto dto);
	}
}

