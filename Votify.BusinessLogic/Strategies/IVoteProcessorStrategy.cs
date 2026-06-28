using Domain.Entitites;
using Shared.DTO;

namespace BusinessLogic.Strategies
{
    public interface IVoteProcessorStrategy
    {
        Voto ProcesarVoto(Votacion votacion, Proyecto proyecto, VotoProyectoDto votoDto, Usuario? usuario);
    }
}
