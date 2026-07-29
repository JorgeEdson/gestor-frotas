namespace GestorFrotas.Api.Dominio;

public sealed record ResumoDoVeiculo
{
    public required string Identificador { get; init; }

    public required long QuantidadeDeLeituras { get; init; }

    public required double? VelocidadeMediaEmQuilometrosPorHora { get; init; }

    public required double? VelocidadeMaximaEmQuilometrosPorHora { get; init; }

    public required double? TemperaturaMaximaDoMotorEmGrausCelsius { get; init; }

    public required double? NivelMedioDeCombustivelEmPercentual { get; init; }

    public required DateTimeOffset? PrimeiraLeitura { get; init; }

    public required DateTimeOffset? UltimaLeitura { get; init; }

    public required double? DistanciaEstimadaEmQuilometros { get; init; }
}
