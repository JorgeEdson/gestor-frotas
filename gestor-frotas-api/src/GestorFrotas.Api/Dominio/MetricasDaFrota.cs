namespace GestorFrotas.Api.Dominio;

public sealed record MetricasDaFrota
{
    public required long QuantidadeDeVeiculos { get; init; }

    public required long QuantidadeDeLeituras { get; init; }

    public required double? VelocidadeMediaEmQuilometrosPorHora { get; init; }

    public required double? TemperaturaMaximaDoMotorEmGrausCelsius { get; init; }

    public required double? NivelMedioDeCombustivelEmPercentual { get; init; }
}
