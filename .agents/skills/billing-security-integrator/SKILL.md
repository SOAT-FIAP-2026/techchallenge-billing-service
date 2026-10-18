---
name: billing-security-integrator
description: >-
  Use this skill when implementing, debugging, or auditing integrations with Mercado Pago API,
  Pix dynamic QR code generation, webhook HMAC SHA-256 signature validation, or idempotency controls.
---

# Billing Security & Payment Integration Skill

Este guia orienta o agente na integração segura com o Mercado Pago, validação de assinaturas criptográficas de webhooks e controle de resiliência.

## 1. Integração com Mercado Pago API v1

- **Base URL**: `https://api.mercadopago.com`
- **Geração de Cobrança PIX**:
  - Endpoint: `POST /v1/payments`
  - Cabeçalhos:
    - `Authorization: Bearer <AccessToken>`
    - `X-Idempotency-Key: <guid-único>`
  - Payload deve conter `payment_method_id: "pix"`, identificação do pagador (`CPF`), `external_reference` da OS e URL de webhook.
- **Consulta de Pagamento**:
  - Endpoint: `GET /v1/payments/{id}`
- **Estorno / Reembolso**:
  - Endpoint: `POST /v1/payments/{id}/refunds`

## 2. Validação Criptográfica de Webhooks (`x-signature`)

1. Extrair os campos `ts` (timestamp) e `v1` (hash HMAC) do cabeçalho `x-signature`.
2. Criar o manifesto de validação:
   - Exemplo: `ts:{ts};body:{requestBody};`
3. Computar o hash **HMAC SHA-256** utilizando `MERCADO_PAGO_WEBHOOK_SECRET`.
4. Comparar os hashes em tempo constante com `CryptographicOperations.FixedTimeEquals` para prevenir timing attacks.

## 3. Checklist de Validação de Segurança

- [ ] Toda chamada ao Mercado Pago envia `X-Idempotency-Key`?
- [ ] O cliente HTTP possui retry exponencial configurado via Polly?
- [ ] A assinatura HMAC do webhook é validada antes de qualquer processamento no banco?
- [ ] Webhooks duplicados com status já processado retornam `200 OK` sem reprocessar?
- [ ] Dados sensíveis (CPF, chaves Pix) estão protegidos contra exposição em logs?
