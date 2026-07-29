using GestorFrotas.Api.Dominio;

namespace GestorFrotas.Api.Consultas;

public interface IRepositorioDeTelemetria
{
    Task<MetricasDaFrota> ObterMetricasDaFrotaAsync(string? categoria, CancellationToken cancellationToken);

    Task<IReadOnlyList<MetricasPorCategoria>> ObterMetricasPorCategoriaAsync(CancellationToken cancellationToken);

    Task<ResumoDoVeiculo?> ObterResumoDoVeiculoAsync(string identificadorDoVeiculo, CancellationToken cancellationToken);
}
