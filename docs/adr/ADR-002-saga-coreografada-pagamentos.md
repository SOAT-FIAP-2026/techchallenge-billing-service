# ADR-002: Implementação do Padrão Saga Coreografada com Compensação

- **Data**: 2026-10-18
- **Status**: Aceito
- **Contexto**: Pós-Graduação Software Architecture (SOAT) — FIAP Fase 4

## Contexto
Com a separação dos microsserviços em bancos de dados independentes (PostgreSQL no OS Service e MongoDB no Billing Service), operações de aprovação de orçamento e pagamento não podem mais utilizar transações ACID centralizadas. Em caso de falha de cobrança, rejeição pelo Mercado Pago ou expiração do Pix, a Ordem de Serviço não pode ficar retida ou avançar para execução indevidamente.

## Decisão
Implementar o **Padrão Saga Coreografada** utilizando mensageria assíncrona (RabbitMQ local / AWS SQS-SNS no EKS) e eventos distribuídos:
- Caminho Feliz: `DiagnosticoFinalizadoEvent` ➔ Cobrança Pix ➔ Webhook Aprovado ➔ `PagamentoConfirmadoEvent` ➔ OS transiciona para `ORCAMENTO_APROVADO`.
- Caminho de Compensação: Falha/Timeout no Mercado Pago ➔ Pagamento Falhado ➔ `PagamentoCanceladoCompensacaoEvent` ➔ OS sofre rollback para `CANCELADA` e estorna reservas.

## Consequências
### Positivas:
- Desacoplamento temporal e espacial entre microsserviços.
- Ausência de ponto único de falha de coordenação (sem orchestrador monolítico).
- Resiliência com retries e Dead-Letter Queues (DLQ).

### Negativas / Trade-offs:
- Consistência eventual precisa ser tratada nos estados intermediários da aplicação.
- Complexidade adicional no monitoramento e correlação de eventos.
