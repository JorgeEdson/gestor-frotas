namespace GestorFrotas.Simulador.Dominio;

public sealed class VeiculoSimulado
{
    public VeiculoSimulado(
        string identificador,
        CategoriaVeiculo categoria,
        double latitudeInicial,
        double longitudeInicial,
        double velocidadeInicialEmQuilometrosPorHora,
        double temperaturaInicialDoMotorEmGrausCelsius,
        double nivelInicialDeCombustivelEmPercentual,
        double direcaoInicialDeDeslocamentoEmRadianos)
    {
        Identificador = identificador;
        Categoria = categoria;
        Latitude = latitudeInicial;
        Longitude = longitudeInicial;
        VelocidadeEmQuilometrosPorHora = velocidadeInicialEmQuilometrosPorHora;
        TemperaturaDoMotorEmGrausCelsius = temperaturaInicialDoMotorEmGrausCelsius;
        NivelDeCombustivelEmPercentual = nivelInicialDeCombustivelEmPercentual;
        DirecaoDeDeslocamentoEmRadianos = direcaoInicialDeDeslocamentoEmRadianos;
    }

    public string Identificador { get; }

    public CategoriaVeiculo Categoria { get; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double VelocidadeEmQuilometrosPorHora { get; set; }

    public double TemperaturaDoMotorEmGrausCelsius { get; set; }

    public double NivelDeCombustivelEmPercentual { get; set; }

    public double DirecaoDeDeslocamentoEmRadianos { get; set; }
}
