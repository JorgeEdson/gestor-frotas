using Elastic.Clients.Elasticsearch;
using GestorFrotas.Api.Configuracao;
using GestorFrotas.Api.Consultas;
using GestorFrotas.Api.Endpoints;
using Microsoft.Extensions.Options;

var construtorDaAplicacao = WebApplication.CreateBuilder(args);

construtorDaAplicacao.Services.Configure<OpcoesElasticsearch>(
    construtorDaAplicacao.Configuration.GetSection(OpcoesElasticsearch.SecaoDeConfiguracao));

construtorDaAplicacao.Services.AddSingleton<ElasticsearchClient>(provedorDeServicos =>
{
    var opcoesElasticsearch = provedorDeServicos.GetRequiredService<IOptions<OpcoesElasticsearch>>().Value;

    var configuracoesDoCliente = new ElasticsearchClientSettings(new Uri(opcoesElasticsearch.Uri))
        .DefaultIndex(opcoesElasticsearch.IndicePadrao);

    return new ElasticsearchClient(configuracoesDoCliente);
});

construtorDaAplicacao.Services.AddSingleton<IRepositorioDeTelemetria, RepositorioDeTelemetriaElasticsearch>();

construtorDaAplicacao.Services.AddOpenApi();

var aplicacao = construtorDaAplicacao.Build();

aplicacao.MapOpenApi();
aplicacao.MapearEndpointsDeMetricas();

await aplicacao.RunAsync();
