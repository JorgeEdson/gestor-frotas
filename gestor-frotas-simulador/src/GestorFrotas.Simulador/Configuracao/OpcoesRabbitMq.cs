namespace GestorFrotas.Simulador.Configuracao;

public sealed class OpcoesRabbitMq
{
    public const string SecaoDeConfiguracao = "RabbitMq";

    public string Host { get; init; } = "localhost";

    public int Porta { get; init; } = 5672;

    public string Usuario { get; init; } = "guest";

    public string Senha { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";

    public string NomeDaExchange { get; init; } = "telemetria.frota";

    public string NomeDaFila { get; init; } = "telemetria.ingestao";

    public string ChaveDeRoteamento { get; init; } = "telemetria.veiculo";
}
