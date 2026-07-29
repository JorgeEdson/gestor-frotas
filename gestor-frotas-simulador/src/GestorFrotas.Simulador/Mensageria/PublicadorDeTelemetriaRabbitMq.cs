using System.Text.Json;
using GestorFrotas.Simulador.Configuracao;
using GestorFrotas.Simulador.Dominio;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GestorFrotas.Simulador.Mensageria;

public sealed class PublicadorDeTelemetriaRabbitMq : IPublicadorDeTelemetria, IAsyncDisposable
{
    private const int QuantidadeMaximaDeTentativasDeConexao = 10;
    private const int IntervaloEntreTentativasDeConexaoEmSegundos = 5;

    private readonly OpcoesRabbitMq opcoesRabbitMq;
    private readonly ILogger<PublicadorDeTelemetriaRabbitMq> registradorDeEventos;
    private readonly JsonSerializerOptions opcoesDeSerializacaoJson;

    private IConnection? conexao;
    private IChannel? canal;

    public PublicadorDeTelemetriaRabbitMq(
        IOptions<OpcoesRabbitMq> opcoesRabbitMq,
        ILogger<PublicadorDeTelemetriaRabbitMq> registradorDeEventos)
    {
        this.opcoesRabbitMq = opcoesRabbitMq.Value;
        this.registradorDeEventos = registradorDeEventos;
        this.opcoesDeSerializacaoJson = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task EstabelecerConexaoAsync(CancellationToken cancellationToken)
    {
        var fabricaDeConexao = new ConnectionFactory
        {
            HostName = opcoesRabbitMq.Host,
            Port = opcoesRabbitMq.Porta,
            UserName = opcoesRabbitMq.Usuario,
            Password = opcoesRabbitMq.Senha,
            VirtualHost = opcoesRabbitMq.VirtualHost
        };

        await ConectarComTentativasSucessivasAsync(fabricaDeConexao, cancellationToken);
        await DeclararTopologiaDeMensageriaAsync(cancellationToken);
    }

    public async Task PublicarEventoAsync(EventoTelemetria evento, CancellationToken cancellationToken)
    {
        if (canal is null)
        {
            throw new InvalidOperationException(
                "A conexao com o RabbitMQ nao foi estabelecida antes da tentativa de publicacao.");
        }

        var corpoDaMensagem = JsonSerializer.SerializeToUtf8Bytes(evento, opcoesDeSerializacaoJson);

        var propriedadesDaMensagem = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await canal.BasicPublishAsync(
            exchange: opcoesRabbitMq.NomeDaExchange,
            routingKey: opcoesRabbitMq.ChaveDeRoteamento,
            mandatory: false,
            basicProperties: propriedadesDaMensagem,
            body: corpoDaMensagem,
            cancellationToken: cancellationToken);
    }

    private async Task ConectarComTentativasSucessivasAsync(
        ConnectionFactory fabricaDeConexao,
        CancellationToken cancellationToken)
    {
        for (var numeroDaTentativa = 1; numeroDaTentativa <= QuantidadeMaximaDeTentativasDeConexao; numeroDaTentativa++)
        {
            try
            {
                conexao = await fabricaDeConexao.CreateConnectionAsync(cancellationToken);
                canal = await conexao.CreateChannelAsync(cancellationToken: cancellationToken);

                registradorDeEventos.LogInformation(
                    "Conexao com o RabbitMQ estabelecida no endereco {Host}:{Porta}.",
                    opcoesRabbitMq.Host,
                    opcoesRabbitMq.Porta);

                return;
            }
            catch (Exception excecao)
            {
                registradorDeEventos.LogWarning(
                    excecao,
                    "Falha ao conectar ao RabbitMQ na tentativa {NumeroDaTentativa} de {QuantidadeMaximaDeTentativas}.",
                    numeroDaTentativa,
                    QuantidadeMaximaDeTentativasDeConexao);

                await Task.Delay(TimeSpan.FromSeconds(IntervaloEntreTentativasDeConexaoEmSegundos), cancellationToken);
            }
        }

        throw new InvalidOperationException(
            "Nao foi possivel estabelecer conexao com o RabbitMQ apos esgotar todas as tentativas.");
    }

    private async Task DeclararTopologiaDeMensageriaAsync(CancellationToken cancellationToken)
    {
        if (canal is null)
        {
            throw new InvalidOperationException(
                "O canal de comunicacao com o RabbitMQ nao esta disponivel para declaracao da topologia.");
        }

        await canal.ExchangeDeclareAsync(
            exchange: opcoesRabbitMq.NomeDaExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.QueueDeclareAsync(
            queue: opcoesRabbitMq.NomeDaFila,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await canal.QueueBindAsync(
            queue: opcoesRabbitMq.NomeDaFila,
            exchange: opcoesRabbitMq.NomeDaExchange,
            routingKey: opcoesRabbitMq.ChaveDeRoteamento,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (canal is not null)
        {
            await canal.DisposeAsync();
        }

        if (conexao is not null)
        {
            await conexao.DisposeAsync();
        }
    }
}
