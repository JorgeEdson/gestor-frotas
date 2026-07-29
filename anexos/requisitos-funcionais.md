## Requisitos Funcionais: Gestor de Frotas (Microsserviços + RabbitMQ + Stack ELK)

Para que o projeto cumpra perfeitamente o objetivo pedagógico de demonstrar o ecossistema do Elastic, os requisitos funcionais foram desenhados para exercitar diretamente a **comunicação assíncrona via mensageria (RabbitMQ)**, a **ingestão e tratamento (Logstash)**, a **indexação e modelagem geoespacial/temporal (Elasticsearch)** e a **visualização analítica e geoespacial (Kibana)**.

A arquitetura adota o padrão de mercado para telemetria: os microsserviços .NET **não gravam diretamente no Elasticsearch nem falam com o Logstash por socket/HTTP**. O simulador **publica** eventos numa exchange do RabbitMQ (protocolo AMQP, sobre TCP) e o Logstash **consome** essa fila. Isso desacopla produtor e coletor, oferecendo buffer, replay e tolerância a falha — exatamente o conceito de microsserviços que a disciplina pretende evidenciar.

```
[.NET Simulador] --AMQP--> [RabbitMQ] --AMQP--> [Logstash] --> [Elasticsearch] --> [Kibana]
                                                                      ^
[.NET API] --consultas de agregação (Elastic.Clients)----------------+
```

Cada microsserviço .NET possui sua própria solution independente, e o `docker-compose` central orquestra a demonstração (RabbitMQ + Logstash + Elasticsearch + Kibana).

### Módulo 1: Simulação e Publicação de Telemetria (Foco: .NET + RabbitMQ)

- **RF01 - Simulação de Telemetria Multicanal:** O sistema deve permitir a geração contínua de eventos de telemetria de múltiplos veículos (simulados via microsserviço .NET) contendo ID do veículo, timestamp, coordenadas geográficas, velocidade, temperatura do motor e nível de combustível. A simulação deve incluir anomalias propositais (superaquecimento e excesso de velocidade) para alimentar os painéis e alertas.

- **RF02 - Publicação Assíncrona via Mensageria (AMQP):** O microsserviço simulador deve publicar cada evento de telemetria em uma _exchange_ do RabbitMQ, roteando para uma fila durável dedicada à ingestão. A comunicação deve usar o protocolo AMQP (client `RabbitMQ.Client`), demonstrando desacoplamento entre produtor e consumidor e resiliência a indisponibilidade do coletor.

### Módulo 2: Ingestão, Normalização e Enriquecimento de Dados (Foco: Logstash)

- **RF03 - Consumo da Fila de Mensageria:** O pipeline do Logstash deve consumir os eventos diretamente da fila do RabbitMQ (via `rabbitmq` input plugin), garantindo o processamento no ritmo do coletor, independente da taxa de publicação do simulador.

- **RF04 - Parsing e Conversão de Tipos:** O pipeline do Logstash deve processar o payload bruto recebido, garantindo a conversão correta dos campos numéricos (ex: `speed` e `engine_temp` para ponto flutuante) e estruturação correta do timestamp.

- **RF05 - Enriquecimento de Dados Geográficos e Operacionais:** O pipeline deve traduzir as coordenadas brutas de latitude/longitude em dados de localização legíveis ou injetar metadados fixos de negócio, como a categoria do veículo (ex: _Carga Pesada_, _Utilitário_).

### Módulo 3: Armazenamento, Mapeamento e Indexação (Foco: Elasticsearch)

- **RF06 - Mapeamento de Séries Temporais e Geoespaciais:** O Elasticsearch deve utilizar um _Index Template_ com mapeamentos explícitos, configurando o campo de localização no tipo `geo_point` e o timestamp no tipo `date`, otimizando consultas temporais e espaciais.

- **RF07 - Agregações Analíticas Dinâmicas:** Um microsserviço .NET de API deve expor endpoints que executem consultas de agregação sob demanda no Elasticsearch (via `Elastic.Clients.Elasticsearch`), calculando métricas globais e filtradas da frota, como velocidade média ponderada, quilometragem percorrida e pico máximo de temperatura do motor.

### Módulo 4: Painéis Operacionais, Geoespaciais e Alertas (Foco: Kibana)

- **RF08 - Mapa de Rastreamento em Tempo Real:** O Kibana deve disponibilizar um painel de _Maps_ exibindo a posição atual de todos os veículos da frota em um mapa geográfico, com indicadores visuais dinâmicos (ex: cores diferentes para veículos em velocidade normal versus veículos em excesso de velocidade).

- **RF09 - Monitoramento de Séries Temporais (Dashboards Operacionais):** O Kibana deve conter um dashboard consolidado apresentando:

    - Um gráfico de linha temporal com a variação da temperatura dos motores ao longo do tempo.

    - Um gráfico de barras ou métricas indicando o status geral da frota (veículos ativos vs. alertas de pane mecânica).

- **RF10 - Detecção de Anomalias e Alertas Operacionais:** O Kibana deve permitir a criação de regras de alerta (via _Kibana Alerts_ ou _Watcher_) para disparar notificações visuais na interface sempre que um veículo registrar uma temperatura de motor superior a um limite crítico (ex: > 100°C) ou velocidade acima da permitida.
