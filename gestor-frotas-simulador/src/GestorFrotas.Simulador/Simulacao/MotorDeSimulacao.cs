using GestorFrotas.Simulador.Configuracao;
using GestorFrotas.Simulador.Dominio;
using Microsoft.Extensions.Options;

namespace GestorFrotas.Simulador.Simulacao;

public sealed class MotorDeSimulacao
{
    private const double QuilometrosPorGrauDeLatitude = 111.0;
    private const double QuantidadeDeMilissegundosPorHora = 3_600_000.0;
    private const double VelocidadeMinimaEmQuilometrosPorHora = 0;
    private const double VariacaoMaximaDeVelocidadePorCicloEmQuilometrosPorHora = 20;
    private const double VariacaoMaximaDeDirecaoPorCicloEmRadianos = Math.PI / 4;
    private const double TemperaturaBaseDeOperacaoEmGrausCelsius = 85;
    private const double AcrescimoTermicoMaximoPorVelocidadeEmGrausCelsius = 10;
    private const double RuidoTermicoMaximoEmGrausCelsius = 4;
    private const double ConsumoBaseDeCombustivelPorCicloEmPercentual = 0.1;
    private const double ConsumoAdicionalMaximoPorVelocidadeEmPercentual = 0.4;
    private const double NivelDeCombustivelParaReabastecimentoEmPercentual = 5;
    private const double NivelDeReabastecimentoEmPercentual = 100;
    private const double AcrescimoMinimoDeVelocidadeEmAnomaliaEmQuilometrosPorHora = 5;
    private const double AcrescimoMaximoDeVelocidadeEmAnomaliaEmQuilometrosPorHora = 40;
    private const double AcrescimoMinimoDeTemperaturaEmAnomaliaEmGrausCelsius = 2;
    private const double AcrescimoMaximoDeTemperaturaEmAnomaliaEmGrausCelsius = 20;

    private readonly OpcoesSimulacao opcoesDeSimulacao;

    public MotorDeSimulacao(IOptions<OpcoesSimulacao> opcoesDeSimulacao)
    {
        this.opcoesDeSimulacao = opcoesDeSimulacao.Value;
    }

    public EventoTelemetria AvancarSimulacaoDoVeiculo(VeiculoSimulado veiculo)
    {
        AtualizarDirecaoDeDeslocamento(veiculo);
        AtualizarPosicaoGeografica(veiculo);
        AtualizarVelocidade(veiculo);
        AtualizarTemperaturaDoMotor(veiculo);
        AtualizarNivelDeCombustivel(veiculo);

        return ProduzirEventoDeTelemetria(veiculo);
    }

    private void AtualizarDirecaoDeDeslocamento(VeiculoSimulado veiculo)
    {
        var distanciaDoCentroEmGraus = CalcularDistanciaDoCentroDaOperacaoEmGraus(veiculo);

        if (distanciaDoCentroEmGraus > opcoesDeSimulacao.RaioDaAreaDeOperacaoEmGraus)
        {
            veiculo.DirecaoDeDeslocamentoEmRadianos = CalcularDirecaoEmDirecaoAoCentroDaOperacao(veiculo);
            return;
        }

        var variacaoDeDirecao = (Random.Shared.NextDouble() - 0.5) * VariacaoMaximaDeDirecaoPorCicloEmRadianos;
        veiculo.DirecaoDeDeslocamentoEmRadianos += variacaoDeDirecao;
    }

    private void AtualizarPosicaoGeografica(VeiculoSimulado veiculo)
    {
        var intervaloEmHoras = opcoesDeSimulacao.IntervaloEntreEnviosEmMilissegundos / QuantidadeDeMilissegundosPorHora;
        var distanciaPercorridaEmQuilometros = veiculo.VelocidadeEmQuilometrosPorHora * intervaloEmHoras;
        var deslocamentoEmGraus = distanciaPercorridaEmQuilometros / QuilometrosPorGrauDeLatitude;

        veiculo.Latitude += Math.Cos(veiculo.DirecaoDeDeslocamentoEmRadianos) * deslocamentoEmGraus;
        veiculo.Longitude += Math.Sin(veiculo.DirecaoDeDeslocamentoEmRadianos) * deslocamentoEmGraus;
    }

    private void AtualizarVelocidade(VeiculoSimulado veiculo)
    {
        if (OcorreuAnomalia())
        {
            veiculo.VelocidadeEmQuilometrosPorHora =
                opcoesDeSimulacao.VelocidadeMaximaPermitidaEmQuilometrosPorHora
                + AcrescimoMinimoDeVelocidadeEmAnomaliaEmQuilometrosPorHora
                + Random.Shared.NextDouble() * AcrescimoMaximoDeVelocidadeEmAnomaliaEmQuilometrosPorHora;
            return;
        }

        var variacaoDeVelocidade = (Random.Shared.NextDouble() - 0.5) * VariacaoMaximaDeVelocidadePorCicloEmQuilometrosPorHora;
        var novaVelocidade = veiculo.VelocidadeEmQuilometrosPorHora + variacaoDeVelocidade;

        veiculo.VelocidadeEmQuilometrosPorHora = Math.Clamp(
            novaVelocidade,
            VelocidadeMinimaEmQuilometrosPorHora,
            opcoesDeSimulacao.VelocidadeMaximaPermitidaEmQuilometrosPorHora);
    }

    private void AtualizarTemperaturaDoMotor(VeiculoSimulado veiculo)
    {
        if (OcorreuAnomalia())
        {
            veiculo.TemperaturaDoMotorEmGrausCelsius =
                opcoesDeSimulacao.TemperaturaCriticaDoMotorEmGrausCelsius
                + AcrescimoMinimoDeTemperaturaEmAnomaliaEmGrausCelsius
                + Random.Shared.NextDouble() * AcrescimoMaximoDeTemperaturaEmAnomaliaEmGrausCelsius;
            return;
        }

        var fatorDeVelocidade = veiculo.VelocidadeEmQuilometrosPorHora / opcoesDeSimulacao.VelocidadeMaximaPermitidaEmQuilometrosPorHora;
        var temperaturaEsperada = TemperaturaBaseDeOperacaoEmGrausCelsius + fatorDeVelocidade * AcrescimoTermicoMaximoPorVelocidadeEmGrausCelsius;
        var ruidoTermico = (Random.Shared.NextDouble() - 0.5) * RuidoTermicoMaximoEmGrausCelsius;

        veiculo.TemperaturaDoMotorEmGrausCelsius = temperaturaEsperada + ruidoTermico;
    }

    private void AtualizarNivelDeCombustivel(VeiculoSimulado veiculo)
    {
        var fatorDeVelocidade = veiculo.VelocidadeEmQuilometrosPorHora / opcoesDeSimulacao.VelocidadeMaximaPermitidaEmQuilometrosPorHora;
        var consumoNoCiclo = ConsumoBaseDeCombustivelPorCicloEmPercentual + fatorDeVelocidade * ConsumoAdicionalMaximoPorVelocidadeEmPercentual;
        var nivelAtualizado = veiculo.NivelDeCombustivelEmPercentual - consumoNoCiclo;

        veiculo.NivelDeCombustivelEmPercentual = nivelAtualizado <= NivelDeCombustivelParaReabastecimentoEmPercentual
            ? NivelDeReabastecimentoEmPercentual
            : nivelAtualizado;
    }

    private bool OcorreuAnomalia()
    {
        return Random.Shared.NextDouble() < opcoesDeSimulacao.ProbabilidadeDeAnomalia;
    }

    private double CalcularDistanciaDoCentroDaOperacaoEmGraus(VeiculoSimulado veiculo)
    {
        var diferencaDeLatitude = veiculo.Latitude - opcoesDeSimulacao.LatitudeCentralDaOperacao;
        var diferencaDeLongitude = veiculo.Longitude - opcoesDeSimulacao.LongitudeCentralDaOperacao;

        return Math.Sqrt(diferencaDeLatitude * diferencaDeLatitude + diferencaDeLongitude * diferencaDeLongitude);
    }

    private double CalcularDirecaoEmDirecaoAoCentroDaOperacao(VeiculoSimulado veiculo)
    {
        var diferencaDeLatitude = opcoesDeSimulacao.LatitudeCentralDaOperacao - veiculo.Latitude;
        var diferencaDeLongitude = opcoesDeSimulacao.LongitudeCentralDaOperacao - veiculo.Longitude;

        return Math.Atan2(diferencaDeLongitude, diferencaDeLatitude);
    }

    private static EventoTelemetria ProduzirEventoDeTelemetria(VeiculoSimulado veiculo)
    {
        return new EventoTelemetria
        {
            IdentificadorDoVeiculo = veiculo.Identificador,
            Categoria = veiculo.Categoria.ToString(),
            MomentoDaLeitura = DateTimeOffset.UtcNow,
            Latitude = Math.Round(veiculo.Latitude, 6),
            Longitude = Math.Round(veiculo.Longitude, 6),
            VelocidadeEmQuilometrosPorHora = Math.Round(veiculo.VelocidadeEmQuilometrosPorHora, 2),
            TemperaturaDoMotorEmGrausCelsius = Math.Round(veiculo.TemperaturaDoMotorEmGrausCelsius, 2),
            NivelDeCombustivelEmPercentual = Math.Round(veiculo.NivelDeCombustivelEmPercentual, 2)
        };
    }
}
