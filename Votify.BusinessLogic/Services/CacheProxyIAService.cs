using System.Collections.Generic;
using System.Threading.Tasks;
using Persistance.Repositories;
using Domain.Entitites;

namespace Votify.BusinessLogic.Services
{
    public class CacheProxyIAService : IIACommentSynthesisService
    {
        private readonly IIACommentSynthesisService _servicioIAReal;
        private readonly IDAL _dal;

        public CacheProxyIAService(IIACommentSynthesisService servicioIAReal, IDAL dal)
        {
            _servicioIAReal = servicioIAReal;
            _dal = dal;
        }

        public async Task<string> ResumirComentariosAsync(int proyectoId, List<string> comentarios)
        {
            var proyecto = await _dal.GetByIdAsync<Proyecto>(proyectoId);
            
            // 1. Caché: Evita el acceso costoso a la IA
            if (proyecto != null && !string.IsNullOrEmpty(proyecto.ResumenIA)) 
            {
                return proyecto.ResumenIA;
            }

            // 2. NO HAY CACHÉ -> Delegación al Sujeto Real ->Petición real a la IA
            var resumen = await _servicioIAReal.ResumirComentariosAsync(proyectoId, comentarios);

            // 3. Cache Management: Persistencia de la copia del objeto costoso
            if (proyecto != null && !resumen.StartsWith("No se ha podido") && !resumen.StartsWith("No hay suficientes") && !resumen.StartsWith("Error"))
            {
                proyecto.ResumenIA = resumen;
                await _dal.CommitAsync();
            }

            return resumen;
        }
    }
}
