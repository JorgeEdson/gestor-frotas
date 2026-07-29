using GestorFrotas.Simulador.Dominio;

namespace GestorFrotas.Simulador.Mensageria;

public interface IPublicadorDeTelemetria
{
    Task EstabelecerConexaoAsync(CancellationToken cancellationToken);

    Task PublicarEventoAsync(EventoTelemetria evento, CancellationToken cancellationToken);
}
