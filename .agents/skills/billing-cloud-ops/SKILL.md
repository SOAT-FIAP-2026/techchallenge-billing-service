---
name: billing-cloud-ops
description: >-
  Use this skill when managing Docker containers, Kubernetes manifests, AWS SSM Parameter Store resolution,
  CI/CD pipelines, or OpenTelemetry observability for the Billing & Payment microservice.
---

# Billing Cloud Operations & SRE Skill

Este guia orienta o agente na conteinerização, orquestração Kubernetes no AWS EKS, resolução dinâmica de segredos no AWS SSM e telemetria.

## 1. AWS SSM Parameter Store

O microsserviço resolve credenciais sem intervenção manual no GitHub Secrets através de:

| Parâmetro SSM | Tipo | Descrição |
|---|---|---|
| `/techchallenge/prod/mongodb_connection_string` | `SecureString` | URI de conexão do MongoDB |
| `/techchallenge/prod/mercadopago_access_token` | `SecureString` | Token de produção/sandbox da API do Mercado Pago |
| `/techchallenge/prod/mercadopago_webhook_secret` | `SecureString` | Segredo HMAC para assinatura dos webhooks |
| `/techchallenge/prod/jwt_secret` | `SecureString` | Chave de validação dos tokens emitidos pela Lambda Auth |

### Comandos de Teste do AWS SSM:
```bash
# Obter connection string do MongoDB
aws ssm get-parameter --name "/techchallenge/prod/mongodb_connection_string" --with-decryption --query "Parameter.Value" --output text --region sa-east-1

# Gravar parâmetro no SSM
aws ssm put-parameter --name "/techchallenge/prod/mercadopago_access_token" --value "TOKEN_AQUI" --type "SecureString" --overwrite --region sa-east-1
```

## 2. Padrões Kubernetes (EKS)

- Namespace: `techchallenge`
- Service: Tipo `NodePort` (exposto na porta do pod e mapeado no ALB).
- Probes:
  - Liveness: `http://<pod>:80/health/live`
  - Readiness: `http://<pod>:80/health/ready`
- Roteamento no ALB:
  - `/api/v1/pagamentos*` ➔ Target Group do Billing Service
  - `/api/v1/orcamentos*` ➔ Target Group do Billing Service

## 3. Observabilidade e Métricas

- Protocolo: OpenTelemetry HTTP OTLP
- Endpoint padrão no cluster: `http://opentelemetry-collector.monitoring.svc.cluster.local:4318/v1/traces`
- Scraping do Prometheus: `GET /metrics`
- Cabeçalho de Correlação: `X-Correlation-ID` propagado em todas as chamadas.
