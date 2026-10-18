# Regras de Segurança e Integração Mercado Pago

Diretrizes obrigatórias para transações financeiras, geração de PIX e recepção de webhooks.

## 1. Validação Criptográfica de Assinaturas
- O endpoint `POST /api/v1/pagamentos/webhook/mercadopago` deve obrigatoriamente validar a assinatura contida no cabeçalho `x-signature`.
- O cálculo da assinatura deve utilizar **HMAC SHA-256** comparando a assinatura enviada contra a chave secreta `MERCADO_PAGO_WEBHOOK_SECRET`.
- Se a assinatura for inválida, a requisição deve ser rejeitada imediatamente com status HTTP `400 Bad Request`.

## 2. Idempotência Rigorosa
- Na geração de transações Pix, enviar cabeçalho `X-Idempotency-Key` gerado com UUID determinístico ou GUID único.
- Ao receber notificações de webhooks, consultar se o pagamento já se encontra no status notificado antes de disparar novos eventos para a mensageria.
- Notificações de pagamentos já aprovados devem retornar `200 OK` de forma imediata (evitar tempestades de eventos duplicados).

## 3. Resiliência de Rede
- Todas as chamadas para a API externa do Mercado Pago devem ser encapsuladas em políticas de retry via `Polly` com backoff exponencial (3 tentativas em status 5xx e 408).
- Respeitar timeout configurado (padrão: 30 segundos).

## 4. Proteção de Dados e LGPD
- CPFs, chaves Pix e dados bancários nunca devem ser expostos em logs em texto claro.
- Sanitizar payloads gravados no histórico do MongoDB se contiverem tokens ou chaves confidenciais.
