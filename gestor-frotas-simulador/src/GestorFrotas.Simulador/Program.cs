using GestorFrotas.Simulador.Configuracao;
using GestorFrotas.Simulador.Mensageria;
using GestorFrotas.Simulador.Simulacao;
using GestorFrotas.Simulador.Worker;

var construtorDaAplicacao = Host.CreateApplicationBuilder(args);

construtorDaAplicacao.Services.Configure<OpcoesRabbitMq>(
    construtorDaAplicacao.Configuration.GetSection(OpcoesRabbitMq.SecaoDeConfiguracao));

construtorDaAplicacao.Services.Configure<OpcoesSimulacao>(
    construtorDaAplicacao.Configuration.GetSection(OpcoesSimulacao.SecaoDeConfiguracao));

construtorDaAplicacao.Services.AddSingleton<IPublicadorDeTelemetria, PublicadorDeTelemetriaRabbitMq>();
construtorDaAplicacao.Services.AddSingleton<GeradorDeFrota>();
construtorDaAplicacao.Services.AddSingleton<MotorDeSimulacao>();
construtorDaAplicacao.Services.AddHostedService<ServicoDeSimulacaoDeFrota>();

var aplicacao = construtorDaAplicacao.Build();

await aplicacao.RunAsync();
