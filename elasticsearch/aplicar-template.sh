#!/bin/sh

echo "aguardando o elasticsearch ficar disponivel..."
until curl -s http://elasticsearch:9200 >/dev/null; do
  sleep 5
done

echo "aplicando index template telemetria-frota..."
curl -sS -X PUT 'http://elasticsearch:9200/_index_template/telemetria-frota' \
  -H 'Content-Type: application/json' \
  --data-binary @/config/index-template.json
echo ""

echo "index template aplicado."
