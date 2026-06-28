using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTO;

namespace Votify.BusinessLogic.Services
{
    public interface IIARoadmapService
    {
        Task<HojaRutaResponseDto> GenerarHojaDeRutaAsync(int concursanteId);
    }
}
