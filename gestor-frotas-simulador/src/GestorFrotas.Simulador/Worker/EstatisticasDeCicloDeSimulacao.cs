namespace GestorFrotas.Simulador.Worker;

public sealed class EstatisticasDeCicloDeSimulacao
{
    public int QuantidadeDeEventosPublicados { get; private set; }

    public int QuantidadeDeAlertasDeExcessoDeVelocidade { get; private set; }

    public int QuantidadeDeAlertasDeSuperaquecimento { get; private set; }

    public void RegistrarEventoPublicado()
    {
        QuantidadeDeEventosPublicados++;
    }

    public void RegistrarAlertaDeExcessoDeVelocidade()
    {
        QuantidadeDeAlertasDeExcessoDeVelocidade++;
    }

    public void RegistrarAlertaDeSuperaquecimento()
    {
        QuantidadeDeAlertasDeSuperaquecimento++;
    }
}
