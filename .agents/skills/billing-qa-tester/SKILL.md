---
name: billing-qa-tester
description: >-
  Use this skill when creating, executing, or maintaining unit tests, BDD scenarios with Reqnroll/SpecFlow,
  mutation tests, or checking code coverage for the Billing & Payment microservice.
---

# Billing QA & BDD Testing Skill

Este guia orienta o agente na implementação e execução da suíte de testes automatizados e especificações BDD para o microsserviço `techchallenge-billing-service`.

## 1. Diretrizes de Qualidade e Cobertura

- **Meta de Cobertura**: Mínimo de **80%** de cobertura de código em linhas e branches.
- **Frameworks**:
  - `xUnit`: Runner de testes unitários.
  - `FluentAssertions`: Asserções fluentes e legíveis.
  - `NSubstitute`: Criação de substitutos e mocks para interfaces de repositórios e serviços HTTP.
  - `Reqnroll`: Framework BDD oficial para .NET 8 com suporte a cenários Gherkin em português (`# language: pt`).
  - `Testcontainers`: Execução de containers reais do MongoDB 7.0 em testes de integração.

## 2. Cenários BDD Obrigatórios (`.feature`)

```gherkin
# language: pt
Funcionalidade: Processamento de Pagamento e Liberação de Ordem de Serviço
  Como cliente da oficina mecânica
  Quero receber a chave Pix do Mercado Pago e ter meu pagamento confirmado
  Para que a execução dos reparos do meu veículo seja iniciada automaticamente

  Cenário: Pagamento via Pix aprovado com sucesso via Webhook
    Dado que existe uma Ordem de Serviço "OS-1001" com diagnóstico finalizado no valor de "450.00"
    Quando o serviço de cobrança gera a transação no Mercado Pago
    Então o status do pagamento deve ser "AGUARDANDO_PAGAMENTO"
    E a chave Pix Copia e Cola deve ser gerada com sucesso
    Quando a notificação de Webhook do Mercado Pago for recebida com status "approved"
    Então o pagamento deve ser atualizado para o status "APROVADO"
    E o evento "PagamentoConfirmadoEvent" deve ser publicado no message broker

  Cenário: Compensação da Saga quando o pagamento é recusado ou expira
    Dado que uma cobrança no valor de "300.00" foi gerada para a Ordem de Serviço "OS-2002"
    Quando o Mercado Pago notificar que a transação foi recusada ou atingir timeout
    Então o status do pagamento deve ser alterado para "FALHADO"
    E o evento de compensação "PagamentoCanceladoCompensacaoEvent" deve ser emitido
```

## 3. Comandos de Execução e Verificação

```bash
# Executar todos os testes
dotnet test

# Executar com relatório de cobertura Coverlet
dotnet test --collect:"XPlat Code Coverage"
```
