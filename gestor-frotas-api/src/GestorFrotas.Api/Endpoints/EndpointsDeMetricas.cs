using GestorFrotas.Api.Consultas;

namespace GestorFrotas.Api.Endpoints;

public static class EndpointsDeMetricas
{
    public static void MapearEndpointsDeMetricas(this WebApplication aplicacao)
    {
        var grupoDeMetricas = aplicacao.MapGroup("/frota");

        grupoDeMetricas.MapGet("/metricas", async (
            string? categoria,
            IRepositorioDeTelemetria repositorioDeTelemetria,
            CancellationToken cancellationToken) =>
        {
            var metricasDaFrota = await repositorioDeTelemetria.ObterMetricasDaFrotaAsync(categoria, cancellationToken);
            return Results.Ok(metricasDaFrota);
        });

        grupoDeMetricas.MapGet("/metricas-por-categoria", async (
            IRepositorioDeTelemetria repositorioDeTelemetria,
            CancellationToken cancellationToken) =>
        {
            var metricasPorCategoria = await repositorioDeTelemetria.ObterMetricasPorCategoriaAsync(cancellationToken);
            return Results.Ok(metricasPorCategoria);
        });

        grupoDeMetricas.MapGet("/veiculos/{identificadorDoVeiculo}/resumo", async (
            string identificadorDoVeiculo,
            IRepositorioDeTelemetria repositorioDeTelemetria,
            CancellationToken cancellationToken) =>
        {
            var resumoDoVeiculo = await repositorioDeTelemetria.ObterResumoDoVeiculoAsync(identificadorDoVeiculo, cancellationToken);
            return resumoDoVeiculo is null ? Results.NotFound() : Results.Ok(resumoDoVeiculo);
        });
    }
}
