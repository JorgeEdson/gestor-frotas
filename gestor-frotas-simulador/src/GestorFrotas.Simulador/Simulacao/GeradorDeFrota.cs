using GestorFrotas.Simulador.Configuracao;
using GestorFrotas.Simulador.Dominio;
using Microsoft.Extensions.Options;

namespace GestorFrotas.Simulador.Simulacao;

public sealed class GeradorDeFrota
{
    private const double VelocidadeInicialMinimaEmQuilometrosPorHora = 40;
    private const double VelocidadeInicialMaximaEmQuilometrosPorHora = 80;
    private const double TemperaturaInicialMinimaEmGrausCelsius = 85;
    private const double TemperaturaInicialMaximaEmGrausCelsius = 95;
    private const double NivelInicialDeCombustivelEmPercentual = 100;

    private readonly OpcoesSimulacao opcoesDeSimulacao;

    public GeradorDeFrota(IOptions<OpcoesSimulacao> opcoesDeSimulacao)
    {
        this.opcoesDeSimulacao = opcoesDeSimulacao.Value;
    }

    public IReadOnlyList<VeiculoSimulado> GerarFrotaInicial()
    {
        var veiculosDaFrota = new List<VeiculoSimulado>();

        for (var numeroSequencial = 1; numeroSequencial <= opcoesDeSimulacao.QuantidadeDeVeiculos; numeroSequencial++)
        {
            veiculosDaFrota.Add(CriarVeiculoInicial(numeroSequencial));
        }

        return veiculosDaFrota;
    }

    private VeiculoSimulado CriarVeiculoInicial(int numeroSequencial)
    {
        var identificador = $"VEICULO-{numeroSequencial:D4}";
        var categoria = SortearCategoria();

        var latitudeInicial = opcoesDeSimulacao.LatitudeCentralDaOperacao + SortearDeslocamentoDentroDoRaio();
        var longitudeInicial = opcoesDeSimulacao.LongitudeCentralDaOperacao + SortearDeslocamentoDentroDoRaio();

        var velocidadeInicial = SortearValorEntre(
            VelocidadeInicialMinimaEmQuilometrosPorHora,
            VelocidadeInicialMaximaEmQuilometrosPorHora);

        var temperaturaInicial = SortearValorEntre(
            TemperaturaInicialMinimaEmGrausCelsius,
            TemperaturaInicialMaximaEmGrausCelsius);

        var direcaoInicial = Random.Shared.NextDouble() * 2 * Math.PI;

        return new VeiculoSimulado(
            identificador,
            categoria,
            latitudeInicial,
            longitudeInicial,
            velocidadeInicial,
            temperaturaInicial,
            NivelInicialDeCombustivelEmPercentual,
            direcaoInicial);
    }

    private static CategoriaVeiculo SortearCategoria()
    {
        var categoriasDisponiveis = Enum.GetValues<CategoriaVeiculo>();
        var indiceSorteado = Random.Shared.Next(categoriasDisponiveis.Length);
        return categoriasDisponiveis[indiceSorteado];
    }

    private double SortearDeslocamentoDentroDoRaio()
    {
        return (Random.Shared.NextDouble() - 0.5) * 2 * opcoesDeSimulacao.RaioDaAreaDeOperacaoEmGraus;
    }

    private static double SortearValorEntre(double valorMinimo, double valorMaximo)
    {
        return valorMinimo + Random.Shared.NextDouble() * (valorMaximo - valorMinimo);
    }
}
