namespace GestorFrotas.Api.Configuracao;

public sealed class OpcoesElasticsearch
{
    public const string SecaoDeConfiguracao = "Elasticsearch";

    public string Uri { get; init; } = "http://localhost:9200";

    public string IndicePadrao { get; init; } = "telemetria-frota-*";
}
