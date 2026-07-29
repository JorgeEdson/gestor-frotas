# Gestor de Frotas — Demonstração da Stack ELK

Projeto didático para demonstrar o ecossistema Elastic (Elasticsearch, Logstash, Kibana) em uma arquitetura de microsserviços com comunicação assíncrona via RabbitMQ.

Um simulador .NET gera telemetria de uma frota de veículos e publica em um broker de mensageria. O Logstash consome, trata e enriquece esses eventos, gravando no Elasticsearch. O Kibana visualiza tudo em mapas, séries temporais e alertas. Uma API .NET expõe métricas agregadas consultadas diretamente no Elasticsearch.

## Arquitetura

```
[.NET Simulador] --AMQP--> [RabbitMQ] --AMQP--> [Logstash] --> [Elasticsearch] --> [Kibana]
                                                                      ^
[.NET API] --consultas de agregação (Elastic.Clients)----------------+
```

O ponto central é o desacoplamento: os microsserviços .NET não falam diretamente com o Logstash nem com o Elasticsearch na ingestão. O simulador publica e segue a vida; o Logstash consome no próprio ritmo, com buffer e tolerância a falha. É o padrão de mercado para telemetria.

## Componentes

| Componente | Pasta | Papel |
|---|---|---|
| Simulador | `gestor-frotas-simulador/` | Microsserviço .NET que gera e publica telemetria (RF01/RF02) |
| API de Agregações | `gestor-frotas-api/` | Microsserviço .NET que consulta métricas no Elasticsearch (RF07) |
| Pipeline Logstash | `logstash/` | Consumo, parsing, enriquecimento e gravação (RF03/RF04/RF05) |
| Index template | `elasticsearch/` | Mapeamento explícito dos campos, incluindo `geo_point` e `date` (RF06) |
| Painéis Kibana | `kibana/` | Data view e regras de alerta provisionados; mapa/dashboards guiados (RF08/RF09/RF10) |
| Orquestração | `docker-compose.yml`, `.env` | Sobe toda a demonstração |

Cada microsserviço .NET tem sua própria solution independente (`.slnx`), com target **.NET 10**.

## Pré-requisitos

Docker e Docker Compose. Opcionalmente, .NET 10 SDK e Visual Studio 2026 para depurar os microsserviços fora dos containers.

## Como subir a demonstração

Na raiz do projeto:

```
docker compose up -d --build
```

Isso sobe, na ordem correta de dependências:

1. `rabbitmq` — broker de mensageria (aguarda ficar saudável).
2. `elasticsearch` — armazenamento e indexação.
3. `configurador-elasticsearch` — aplica o index template e encerra.
4. `logstash` — só inicia após o template existir.
5. `kibana` — interface de visualização.
6. `configurador-kibana` — cria o data view e as regras de alerta e encerra.
7. `simulador` e `api` — os microsserviços .NET.

## Endereços

| Serviço | URL | Observação |
|---|---|---|
| RabbitMQ Management | http://localhost:15672 | usuário `guest`, senha `guest` |
| Elasticsearch | http://localhost:9200 | — |
| Kibana | http://localhost:5601 | data view `Telemetria da Frota` |
| API de Agregações | http://localhost:8080 | OpenAPI em `/openapi/v1.json` |
| Logstash | http://localhost:9600 | API de monitoramento |

Exemplos de chamada à API:

```
curl http://localhost:8080/frota/metricas
curl "http://localhost:8080/frota/metricas?categoria=CargaPesada"
curl http://localhost:8080/frota/metricas-por-categoria
curl http://localhost:8080/frota/veiculos/VEICULO-0001/resumo
```

## Requisitos funcionais

Os requisitos completos estão em `anexos/requisitos-funcionais.md` (RF01 a RF10).
