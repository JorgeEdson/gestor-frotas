namespace GestorFrotas.Api.Dominio;

public sealed record MetricasPorCategoria
{
    public required string Categoria { get; init; }

    public required long QuantidadeDeVeiculos { get; init; }

    public required long QuantidadeDeLeituras { get; init; }

    public required double? VelocidadeMediaEmQuilometrosPorHora { get; init; }

    public required double? TemperaturaMaximaDoMotorEmGrausCelsius { get; init; }
}
