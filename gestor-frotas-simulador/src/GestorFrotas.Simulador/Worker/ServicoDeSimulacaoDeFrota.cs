using GestorFrotas.Simulador.Configuracao;
using GestorFrotas.Simulador.Dominio;
using GestorFrotas.Simulador.Mensageria;
using GestorFrotas.Simulador.Simulacao;
using Microsoft.Extensions.Options;

namespace GestorFrotas.Simulador.Worker;

public sealed class ServicoDeSimulacaoDeFrota : BackgroundService
{
    private readonly IPublicadorDeTelemetria publicadorDeTelemetria;
    private readonly GeradorDeFrota geradorDeFrota;
    private readonly MotorDeSimulacao motorDeSimulacao;
    private readonly OpcoesSimulacao opcoesDeSimulacao;
    private readonly ILogger<ServicoDeSimulacaoDeFrota> registradorDeEventos;

    public ServicoDeSimulacaoDeFrota(
        IPublicadorDeTelemetria publicadorDeTelemetria,
        GeradorDeFrota geradorDeFrota,
        MotorDeSimulacao motorDeSimulacao,
        IOptions<OpcoesSimulacao> opcoesDeSimulacao,
        ILogger<ServicoDeSimulacaoDeFrota> registradorDeEventos)
    {
        this.publicadorDeTelemetria = publicadorDeTelemetria;
        this.geradorDeFrota = geradorDeFrota;
        this.motorDeSimulacao = motorDeSimulacao;
        this.opcoesDeSimulacao = opcoesDeSimulacao.Value;
        this.registradorDeEventos = registradorDeEventos;
    }

    protected override async Task ExecuteAsync(CancellationToken tokenDeEncerramento)
    {
        await publicadorDeTelemetria.EstabelecerConexaoAsync(tokenDeEncerramento);

        var frota = geradorDeFrota.GerarFrotaInicial();
        RegistrarInicioDaSimulacao(frota);

        var numeroDoCiclo = 0;

        while (!tokenDeEncerramento.IsCancellationRequested)
        {
            numeroDoCiclo++;

            var estatisticasDoCiclo = await ExecutarCicloDeSimulacaoAsync(frota, tokenDeEncerramento);
            RegistrarResumoDoCiclo(numeroDoCiclo, estatisticasDoCiclo);

            await AguardarProximoCicloAsync(tokenDeEncerramento);
        }

        registradorDeEventos.LogInformation(
            "Simulacao encerrada apos {QuantidadeDeCiclos} ciclos.",
            numeroDoCiclo);
    }

    private async Task<EstatisticasDeCicloDeSimulacao> ExecutarCicloDeSimulacaoAsync(
        IReadOnlyList<VeiculoSimulado> frota,
        CancellationToken tokenDeEncerramento)
    {
        var estatisticasDoCiclo = new EstatisticasDeCicloDeSimulacao();

        foreach (var veiculo in frota)
        {
            if (tokenDeEncerramento.IsCancellationRequested)
            {
                return estatisticasDoCiclo;
            }

            var evento = motorDeSimulacao.AvancarSimulacaoDoVeiculo(veiculo);
            await publicadorDeTelemetria.PublicarEventoAsync(evento, tokenDeEncerramento);

            estatisticasDoCiclo.RegistrarEventoPublicado();
            RegistrarTelemetriaPublicada(evento);
            AvaliarEregistrarAlertas(evento, estatisticasDoCiclo);
        }

        return estatisticasDoCiclo;
    }

    private void RegistrarInicioDaSimulacao(IReadOnlyList<VeiculoSimulado> frota)
    {
        registradorDeEventos.LogInformation(
            "Simulacao iniciada com {QuantidadeDeVeiculos} veiculos, publicando a cada {IntervaloEmMilissegundos} milissegundos.",
            frota.Count,
            opcoesDeSimulacao.IntervaloEntreEnviosEmMilissegundos);
    }

    private void RegistrarTelemetriaPublicada(EventoTelemetria evento)
    {
        registradorDeEventos.LogDebug(
            "Telemetria publicada | Veiculo {Identificador} | Categoria {Categoria} | Velocidade {Velocidade} km/h | Temperatura {Temperatura} C | Combustivel {Combustivel}% | Posicao {Latitude},{Longitude}",
            evento.IdentificadorDoVeiculo,
            evento.Categoria,
            evento.VelocidadeEmQuilometrosPorHora,
            evento.TemperaturaDoMotorEmGrausCelsius,
            evento.NivelDeCombustivelEmPercentual,
            evento.Latitude,
            evento.Longitude);
    }

    private void AvaliarEregistrarAlertas(EventoTelemetria evento, EstatisticasDeCicloDeSimulacao estatisticasDoCiclo)
    {
        if (evento.VelocidadeEmQuilometrosPorHora > opcoesDeSimulacao.VelocidadeMaximaPermitidaEmQuilometrosPorHora)
        {
            estatisticasDoCiclo.RegistrarAlertaDeExcessoDeVelocidade();

            registradorDeEventos.LogWarning(
                "Alerta de excesso de velocidade | Veiculo {Identificador} registrou {Velocidade} km/h (limite {LimiteDeVelocidade} km/h).",
                evento.IdentificadorDoVeiculo,
                evento.VelocidadeEmQuilometrosPorHora,
                opcoesDeSimulacao.VelocidadeMaximaPermitidaEmQuilometrosPorHora);
        }

        if (evento.TemperaturaDoMotorEmGrausCelsius > opcoesDeSimulacao.TemperaturaCriticaDoMotorEmGrausCelsius)
        {
            estatisticasDoCiclo.RegistrarAlertaDeSuperaquecimento();

            registradorDeEventos.LogWarning(
                "Alerta de superaquecimento | Veiculo {Identificador} registrou {Temperatura} C (limite {LimiteDeTemperatura} C).",
                evento.IdentificadorDoVeiculo,
                evento.TemperaturaDoMotorEmGrausCelsius,
                opcoesDeSimulacao.TemperaturaCriticaDoMotorEmGrausCelsius);
        }
    }

    private void RegistrarResumoDoCiclo(int numeroDoCiclo, EstatisticasDeCicloDeSimulacao estatisticasDoCiclo)
    {
        registradorDeEventos.LogInformation(
            "Ciclo {NumeroDoCiclo} concluido | {QuantidadeDeEventos} eventos publicados | {AlertasDeVelocidade} alertas de excesso de velocidade | {AlertasDeTemperatura} alertas de superaquecimento.",
            numeroDoCiclo,
            estatisticasDoCiclo.QuantidadeDeEventosPublicados,
            estatisticasDoCiclo.QuantidadeDeAlertasDeExcessoDeVelocidade,
            estatisticasDoCiclo.QuantidadeDeAlertasDeSuperaquecimento);
    }

    private async Task AguardarProximoCicloAsync(CancellationToken tokenDeEncerramento)
    {
        try
        {
            await Task.Delay(opcoesDeSimulacao.IntervaloEntreEnviosEmMilissegundos, tokenDeEncerramento);
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }
}
