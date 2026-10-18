# 💳 Tech Challenge — Billing & Payment Service (Fase 4)

**Pós-Graduação em Software Architecture (SOAT) — FIAP**  
Microsserviço de gestão de orçamentos, emissão de cobranças PIX dinâmicas via Mercado Pago, recepção de webhooks com validação HMAC SHA-256 e suporte à Saga Pattern.

---

## 🏛️ Arquitetura e Bounded Context

Este serviço foi concebido seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)** como parte do desacoplamento do monolito (`fase1-tech-challenge`):

```
techchallenge-billing-service/
├── src/
│   ├── Fiap.TechChallenge.Billing.Domain/          # Entidades, Enums, Value Objects e Contratos
│   ├── Fiap.TechChallenge.Billing.Application/     # DTOs, AppServices e Casos de Uso
│   ├── Fiap.TechChallenge.Billing.Infrastructure/  # Driver MongoDB (BSON Maps, Índices) e AWS SSM
│   ├── Fiap.TechChallenge.Billing.External/        # Integração HTTP com Mercado Pago (Polly Retry)
│   └── Fiap.TechChallenge.Billing.Api/             # ASP.NET Core 8 Web API, Middlewares, Swagger, OTel
└── Fiap.TechChallenge.Billing.sln
```

### Decisão de Banco de Dados: MongoDB 7.0 (NoSQL)
- **Estruturas Dinâmicas de Webhooks**: Armazenamento completo e flexível dos metadados recebidos das notificações IPN do Mercado Pago.
- **Snapshots Imutáveis**: Histórico e integridade dos itens de orçamento e valores praticados no momento da aprovação do cliente.
- **Atendimento ao Edital**: Cumpre a exigência de segregação de banco próprio não relacional (NoSQL) por microsserviço.

---

## 🔐 Resolução de Segredos & AWS SSM Parameter Store

O serviço suporta resolução híbrida de credenciais:
1. **Em Nuvem (AWS EKS)**: Resolução dinâmica via **AWS Systems Manager Parameter Store** (`/techchallenge/prod/*`).
2. **Local / Containers**: Fallback automático e transparente para variáveis de ambiente e `appsettings.json`.

| Parâmetro SSM | Variável de Ambiente / Config | Descrição |
|---|---|---|
| `/techchallenge/prod/mongodb_connection_string` | `MONGODB_CONNECTION_STRING` | URI de conexão do MongoDB |
| `/techchallenge/prod/mercadopago_access_token` | `MERCADOPAGO_ACCESS_TOKEN` | Token de autenticação da API Mercado Pago |
| `/techchallenge/prod/mercadopago_webhook_secret` | `MERCADOPAGO_WEBHOOK_SECRET` | Chave secreta HMAC para validação de Webhooks |
| `/techchallenge/prod/jwt_secret` | `JWT_SECRET` | Chave de assinatura para validação dos tokens JWT |

---

## 🌐 Endpoints REST da API

### Orçamentos (`/api/v1/orcamentos`)
- `POST /api/v1/orcamentos`: Criação e cálculo de orçamento para uma Ordem de Serviço.
- `GET /api/v1/orcamentos/{id}`: Consulta orçamento por ID com histórico detalhado.
- `GET /api/v1/orcamentos/os/{osId}`: Consulta orçamento vinculado a uma Ordem de Serviço.
- `PATCH /api/v1/orcamentos/{id}/aprovar`: Aprovação do orçamento.
- `PATCH /api/v1/orcamentos/{id}/cancelar`: Cancelamento do orçamento.

### Pagamentos (`/api/v1/pagamentos`)
- `POST /api/v1/pagamentos`: Inicia cobrança e gera QR Code dinâmico PIX no Mercado Pago.
- `GET /api/v1/pagamentos/{id}`: Consulta status e detalhes do pagamento.
- `GET /api/v1/pagamentos/os/{osId}`: Consulta status do pagamento por OS.
- `POST /api/v1/pagamentos/webhook/mercadopago`: Recebimento de notificações de Webhook com validação de assinatura `x-signature` (HMAC SHA-256).
- `POST /api/v1/pagamentos/{id}/estorno`: Ação compensatória da Saga para estorno no gateway.

### Observabilidade & Health
- `GET /health/live`: Liveness probe do Kubernetes.
- `GET /health/ready`: Readiness probe (verifica conexão ativa com MongoDB).
- `GET /metrics`: Métricas de negócio e runtime no formato Prometheus.
- `GET /swagger`: Documentação interativa OpenAPI / Swagger UI.

---

## 🚀 Como Rodar Localmente

### Pré-requisitos
- .NET 8 SDK
- MongoDB (Docker ou local)

### Subindo MongoDB via Docker
```bash
docker run -d --name mongodb-billing -p 27017:27017 -e MONGO_INITDB_DATABASE=techchallenge_billing_db mongo:7.0
```

### Executando a API
```bash
cd src/Fiap.TechChallenge.Billing.Api
dotnet run
```

Acesse o Swagger em: `http://localhost:5000/swagger` ou `https://localhost:5001/swagger`.
