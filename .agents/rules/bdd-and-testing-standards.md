# Regras de Testes e Padrões BDD

Diretrizes obrigatórias de qualidade de código para o `techchallenge-billing-service`.

## 1. Cobertura Mínima
- A cobertura de testes automatizados do serviço deve ser **>= 80%**, auditada via Coverlet e SonarCloud.
- Todos os serviços da camada `Application` e métodos de negócio da camada `Domain` devem ter 100% dos ramos lógicos (`if`, `switch`, exceções de validação) cobertos.

## 2. Padrões de Testes Unitários
- Framework padrão: `xUnit`
- Asserções: `FluentAssertions`
- Mocks e Test Doubles: `NSubstitute`
- Nomenclatura dos métodos de teste: `Dado_Condicao_Quando_Acao_Entao_ResultadoEsperado` ou `Metodo_Cenario_ComportamentoEsperado`.

## 3. Padrões BDD (Behavior-Driven Development)
- Utilizar `Reqnroll` (sucessor oficial do SpecFlow para .NET 8).
- Cenários escritos em português (`# language: pt`) em arquivos `.feature`.
- Estrutura obrigatória:
  - `Dado que...` (preparação de contexto / state setup)
  - `Quando...` (ação do usuário ou evento do gateway)
  - `Então...` (asserção de estado final e evento publicado)
  - `E...` (asserções complementares)
- Pelo menos um fluxo feliz completo e um fluxo de compensação/erro devem ser implementados via BDD.
