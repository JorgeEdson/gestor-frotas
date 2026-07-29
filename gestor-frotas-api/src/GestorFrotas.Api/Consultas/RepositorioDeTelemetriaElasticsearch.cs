using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using GestorFrotas.Api.Configuracao;
using GestorFrotas.Api.Dominio;
using Microsoft.Extensions.Options;

namespace GestorFrotas.Api.Consultas;

public sealed class RepositorioDeTelemetriaElasticsearch : IRepositorioDeTelemetria
{
    private const string CampoIdentificadorDoVeiculo = "identificadorDoVeiculo";
    private const string CampoCategoria = "categoria";
    private const string CampoVelocidade = "velocidadeEmQuilometrosPorHora";
    private const string CampoTemperaturaDoMotor = "temperaturaDoMotorEmGrausCelsius";
    private const string CampoNivelDeCombustivel = "nivelDeCombustivelEmPercentual";
    private const string CampoMomentoDaLeitura = "@timestamp";
    private const double QuantidadeDeMilissegundosPorHora = 3_600_000.0;
    private const int QuantidadeMaximaDeCategorias = 20;

    private readonly ElasticsearchClient clienteElasticsearch;
    private readonly OpcoesElasticsearch opcoesElasticsearch;

    public RepositorioDeTelemetriaElasticsearch(
        ElasticsearchClient clienteElasticsearch,
        IOptions<OpcoesElasticsearch> opcoesElasticsearch)
    {
        this.clienteElasticsearch = clienteElasticsearch;
        this.opcoesElasticsearch = opcoesElasticsearch.Value;
    }

    public async Task<MetricasDaFrota> ObterMetricasDaFrotaAsync(string? categoria, CancellationToken cancellationToken)
    {
        var resposta = await clienteElasticsearch.SearchAsync<LeituraTelemetria>(busca =>
        {
            busca.Indices(opcoesElasticsearch.IndicePadrao);
            busca.Size(0);
            AplicarFiltroDeCategoria(busca, categoria);
            busca.Aggregations(agregacoes => agregacoes
                .Add("quantidade_veiculos", agregacao => agregacao.Cardinality(alvo => alvo.Field(CampoIdentificadorDoVeiculo)))
                .Add("quantidade_leituras", agregacao => agregacao.ValueCount(alvo => alvo.Field(CampoIdentificadorDoVeiculo)))
                .Add("velocidade_media", agregacao => agregacao.Avg(alvo => alvo.Field(CampoVelocidade)))
                .Add("temperatura_maxima", agregacao => agregacao.Max(alvo => alvo.Field(CampoTemperaturaDoMotor)))
                .Add("combustivel_medio", agregacao => agregacao.Avg(alvo => alvo.Field(CampoNivelDeCombustivel))));
        }, cancellationToken);

        GarantirRespostaValida(resposta);

        return new MetricasDaFrota
        {
            QuantidadeDeVeiculos = LerContagem(resposta.Aggregations!.GetCardinality("quantidade_veiculos")?.Value),
            QuantidadeDeLeituras = LerContagem(resposta.Aggregations!.GetValueCount("quantidade_leituras")?.Value),
            VelocidadeMediaEmQuilometrosPorHora = ArredondarOpcional(resposta.Aggregations!.GetAverage("velocidade_media")?.Value),
            TemperaturaMaximaDoMotorEmGrausCelsius = ArredondarOpcional(resposta.Aggregations!.GetMax("temperatura_maxima")?.Value),
            NivelMedioDeCombustivelEmPercentual = ArredondarOpcional(resposta.Aggregations!.GetAverage("combustivel_medio")?.Value)
        };
    }

    public async Task<IReadOnlyList<MetricasPorCategoria>> ObterMetricasPorCategoriaAsync(CancellationToken cancellationToken)
    {
        var resposta = await clienteElasticsearch.SearchAsync<LeituraTelemetria>(busca =>
        {
            busca.Indices(opcoesElasticsearch.IndicePadrao);
            busca.Size(0);
            busca.Aggregations(agregacoes => agregacoes
                .Add("por_categoria", agregacao => agregacao
                    .Terms(termos => termos.Field(CampoCategoria).Size(QuantidadeMaximaDeCategorias))
                    .Aggregations(subagregacoes => subagregacoes
                        .Add("quantidade_veiculos", subagregacao => subagregacao.Cardinality(alvo => alvo.Field(CampoIdentificadorDoVeiculo)))
                        .Add("velocidade_media", subagregacao => subagregacao.Avg(alvo => alvo.Field(CampoVelocidade)))
                        .Add("temperatura_maxima", subagregacao => subagregacao.Max(alvo => alvo.Field(CampoTemperaturaDoMotor))))));
        }, cancellationToken);

        GarantirRespostaValida(resposta);

        var metricasPorCategoria = new List<MetricasPorCategoria>();
        var termosDeCategoria = resposta.Aggregations!.GetStringTerms("por_categoria");

        if (termosDeCategoria is not null)
        {
            foreach (var faixa in termosDeCategoria.Buckets)
            {
                metricasPorCategoria.Add(new MetricasPorCategoria
                {
                    Categoria = faixa.Key.ToString(),
                    QuantidadeDeLeituras = faixa.DocCount,
                    QuantidadeDeVeiculos = LerContagem(faixa.Aggregations.GetCardinality("quantidade_veiculos")?.Value),
                    VelocidadeMediaEmQuilometrosPorHora = ArredondarOpcional(faixa.Aggregations.GetAverage("velocidade_media")?.Value),
                    TemperaturaMaximaDoMotorEmGrausCelsius = ArredondarOpcional(faixa.Aggregations.GetMax("temperatura_maxima")?.Value)
                });
            }
        }

        return metricasPorCategoria;
    }

    public async Task<ResumoDoVeiculo?> ObterResumoDoVeiculoAsync(string identificadorDoVeiculo, CancellationToken cancellationToken)
    {
        var resposta = await clienteElasticsearch.SearchAsync<LeituraTelemetria>(busca =>
        {
            busca.Indices(opcoesElasticsearch.IndicePadrao);
            busca.Size(0);
            busca.Query(consulta => consulta.Term(termo => termo.Field(CampoIdentificadorDoVeiculo).Value(identificadorDoVeiculo)));
            busca.Aggregations(agregacoes => agregacoes
                .Add("quantidade_leituras", agregacao => agregacao.ValueCount(alvo => alvo.Field(CampoIdentificadorDoVeiculo)))
                .Add("velocidade_media", agregacao => agregacao.Avg(alvo => alvo.Field(CampoVelocidade)))
                .Add("velocidade_maxima", agregacao => agregacao.Max(alvo => alvo.Field(CampoVelocidade)))
                .Add("temperatura_maxima", agregacao => agregacao.Max(alvo => alvo.Field(CampoTemperaturaDoMotor)))
                .Add("combustivel_medio", agregacao => agregacao.Avg(alvo => alvo.Field(CampoNivelDeCombustivel)))
                .Add("primeira_leitura", agregacao => agregacao.Min(alvo => alvo.Field(CampoMomentoDaLeitura)))
                .Add("ultima_leitura", agregacao => agregacao.Max(alvo => alvo.Field(CampoMomentoDaLeitura))));
        }, cancellationToken);

        GarantirRespostaValida(resposta);

        var quantidadeDeLeituras = LerContagem(resposta.Aggregations!.GetValueCount("quantidade_leituras")?.Value);

        if (quantidadeDeLeituras == 0)
        {
            return null;
        }

        var velocidadeMedia = resposta.Aggregations!.GetAverage("velocidade_media")?.Value;
        var primeiraLeituraEmEpochMilissegundos = resposta.Aggregations!.GetMin("primeira_leitura")?.Value;
        var ultimaLeituraEmEpochMilissegundos = resposta.Aggregations!.GetMax("ultima_leitura")?.Value;

        return new ResumoDoVeiculo
        {
            Identificador = identificadorDoVeiculo,
            QuantidadeDeLeituras = quantidadeDeLeituras,
            VelocidadeMediaEmQuilometrosPorHora = ArredondarOpcional(velocidadeMedia),
            VelocidadeMaximaEmQuilometrosPorHora = ArredondarOpcional(resposta.Aggregations!.GetMax("velocidade_maxima")?.Value),
            TemperaturaMaximaDoMotorEmGrausCelsius = ArredondarOpcional(resposta.Aggregations!.GetMax("temperatura_maxima")?.Value),
            NivelMedioDeCombustivelEmPercentual = ArredondarOpcional(resposta.Aggregations!.GetAverage("combustivel_medio")?.Value),
            PrimeiraLeitura = ConverterEpochMilissegundosParaDataHora(primeiraLeituraEmEpochMilissegundos),
            UltimaLeitura = ConverterEpochMilissegundosParaDataHora(ultimaLeituraEmEpochMilissegundos),
            DistanciaEstimadaEmQuilometros = EstimarDistanciaPercorrida(velocidadeMedia, primeiraLeituraEmEpochMilissegundos, ultimaLeituraEmEpochMilissegundos)
        };
    }

    private static void AplicarFiltroDeCategoria(SearchRequestDescriptor<LeituraTelemetria> busca, string? categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria))
        {
            return;
        }

        busca.Query(consulta => consulta.Term(termo => termo.Field(CampoCategoria).Value(categoria)));
    }

    private static void GarantirRespostaValida(SearchResponse<LeituraTelemetria> resposta)
    {
        if (!resposta.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"A consulta ao Elasticsearch nao foi bem sucedida. {resposta.DebugInformation}");
        }
    }

    private static long LerContagem(long? valor)
    {
        return valor ?? 0;
    }

    private static long LerContagem(double? valor)
    {
        return valor.HasValue ? (long)valor.Value : 0;
    }

    private static double? ArredondarOpcional(double? valor)
    {
        return valor.HasValue ? Math.Round(valor.Value, 2) : null;
    }

    private static DateTimeOffset? ConverterEpochMilissegundosParaDataHora(double? epochEmMilissegundos)
    {
        return epochEmMilissegundos.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds((long)epochEmMilissegundos.Value)
            : null;
    }

    private static double? EstimarDistanciaPercorrida(
        double? velocidadeMediaEmQuilometrosPorHora,
        double? inicioEmEpochMilissegundos,
        double? fimEmEpochMilissegundos)
    {
        if (!velocidadeMediaEmQuilometrosPorHora.HasValue || !inicioEmEpochMilissegundos.HasValue || !fimEmEpochMilissegundos.HasValue)
        {
            return null;
        }

        var duracaoEmHoras = (fimEmEpochMilissegundos.Value - inicioEmEpochMilissegundos.Value) / QuantidadeDeMilissegundosPorHora;
        return Math.Round(velocidadeMediaEmQuilometrosPorHora.Value * duracaoEmHoras, 2);
    }
}
