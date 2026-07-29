using System.Text.Json.Serialization;

namespace GestorFrotas.Api.Dominio;

public sealed record LeituraTelemetria
{
    [JsonPropertyName("identificadorDoVeiculo")]
    public string IdentificadorDoVeiculo { get; init; } = string.Empty;

    [JsonPropertyName("categoria")]
    public string Categoria { get; init; } = string.Empty;

    [JsonPropertyName("descricaoDaCategoria")]
    public string DescricaoDaCategoria { get; init; } = string.Empty;

    [JsonPropertyName("momentoDaLeitura")]
    public DateTimeOffset MomentoDaLeitura { get; init; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("velocidadeEmQuilometrosPorHora")]
    public double VelocidadeEmQuilometrosPorHora { get; init; }

    [JsonPropertyName("temperaturaDoMotorEmGrausCelsius")]
    public double TemperaturaDoMotorEmGrausCelsius { get; init; }

    [JsonPropertyName("nivelDeCombustivelEmPercentual")]
    public double NivelDeCombustivelEmPercentual { get; init; }
}
