namespace GestorFrotas.Simulador.Dominio;

public sealed record EventoTelemetria
{
    public required string IdentificadorDoVeiculo { get; init; }

    public required string Categoria { get; init; }

    public required DateTimeOffset MomentoDaLeitura { get; init; }

    public required double Latitude { get; init; }

    public required double Longitude { get; init; }

    public required double VelocidadeEmQuilometrosPorHora { get; init; }

    public required double TemperaturaDoMotorEmGrausCelsius { get; init; }

    public required double NivelDeCombustivelEmPercentual { get; init; }
}
