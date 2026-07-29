namespace GestorFrotas.Simulador.Configuracao;

public sealed class OpcoesSimulacao
{
    public const string SecaoDeConfiguracao = "Simulacao";

    public int QuantidadeDeVeiculos { get; init; } = 10;

    public int IntervaloEntreEnviosEmMinutos { get; init; } = 2;

    public double LatitudeCentralDaOperacao { get; init; } = -23.5613;

    public double LongitudeCentralDaOperacao { get; init; } = -46.6560;

    public double RaioDaAreaDeOperacaoEmGraus { get; init; } = 0.05;

    public double VelocidadeMaximaPermitidaEmQuilometrosPorHora { get; init; } = 90;

    public double TemperaturaCriticaDoMotorEmGrausCelsius { get; init; } = 100;

    public double ProbabilidadeDeAnomalia { get; init; } = 0.05;
}
