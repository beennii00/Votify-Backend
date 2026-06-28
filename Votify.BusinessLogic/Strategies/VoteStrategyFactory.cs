using Shared.Enums;

namespace BusinessLogic.Strategies
{
    public static class VoteStrategyFactory
    {
        public static IVoteProcessorStrategy ObtenerEstrategia(TipoVotacion tipo) => tipo switch
        {
            TipoVotacion.Numerica => new NumericaStrategy(),
            TipoVotacion.Binaria => new BinariaStrategy(),
            TipoVotacion.CriteriosPorPeso => new CriteriosPorPesoStrategy(),
            _ => throw new NotSupportedException($"Tipo de votación no soportado: {tipo}")
        };
    }
}
