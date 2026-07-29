#!/bin/sh

echo "aguardando o kibana ficar disponivel..."
until curl -s http://kibana:5601/api/status | grep -q '"level":"available"'; do
  sleep 5
done

echo "criando data view telemetria-frota..."
curl -sS -X POST http://kibana:5601/api/data_views/data_view \
  -H 'kbn-xsrf: true' \
  -H 'Content-Type: application/json' \
  --data-binary @/config/data-view.json
echo ""

echo "criando regra de alerta de superaquecimento..."
curl -sS -X POST http://kibana:5601/api/alerting/rule/alerta-superaquecimento \
  -H 'kbn-xsrf: true' \
  -H 'Content-Type: application/json' \
  --data-binary @/config/alerta-superaquecimento.json
echo ""

echo "criando regra de alerta de excesso de velocidade..."
curl -sS -X POST http://kibana:5601/api/alerting/rule/alerta-excesso-velocidade \
  -H 'kbn-xsrf: true' \
  -H 'Content-Type: application/json' \
  --data-binary @/config/alerta-excesso-velocidade.json
echo ""

echo "provisionamento do kibana concluido."
