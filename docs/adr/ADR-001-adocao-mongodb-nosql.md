# ADR-001: Adoção do MongoDB 7.0 para Persistência de Orçamentos e Pagamentos

- **Data**: 2026-10-18
- **Status**: Aceito
- **Contexto**: Pós-Graduação Software Architecture (SOAT) — FIAP Fase 4

## Contexto
O Tech Challenge Fase 4 determina a segregação obrigatória de banco de dados por microsserviço, exigindo o uso de pelo menos um banco relacional (SQL) e pelo menos um não relacional (NoSQL). O serviço principal (`fase1-tech-challenge`) já utiliza PostgreSQL 16 para entidades relacionais ACID (Clientes, Veículos, Catálogo e OS). 

O microsserviço de Billing e Pagamento demanda:
1. Armazenamento de notificações de Webhook (IPN) do Mercado Pago com esquemas dinâmicos e metadados variáveis de antifraude e bandeiras de cartão/Pix.
2. Snapshots imutáveis de orçamentos aprovados pelo cliente.
3. Isolamento completo de dados sem acoplamento a tabelas relacionais de outros serviços.

## Decisão
Adotar **MongoDB 7.0 (NoSQL Document Store)** como banco de dados exclusivo do `techchallenge-billing-service`, utilizando coleções tipadas `orcamentos` e `pagamentos` com mapeamento explícito via driver oficial C# (`MongoDB.Driver`).

## Consequências
### Positivas:
- Atende à exigência formal do edital de uso de banco NoSQL.
- Esquema flexível para auditoria e histórico de múltiplos payloads de webhooks por transação.
- Snapshots imutáveis de itens de orçamento representados naturalmente como documentos incorporados.
- Alto desempenho de leitura/escrita e suporte a índices específicos (`IdOrdemServico`, `ExternalPaymentId`).

### Negativas / Trade-offs:
- Não possui transações multi-banco com o PostgreSQL do OS Service, exigindo o padrão Saga Coreografada para manter consistência eventual.
