# Regras de Clean Architecture & Design de Domínio

Este arquivo estabelece os limites arquiteturais obrigatórios para o `techchallenge-billing-service`.

## 1. Regra das Dependências
- A camada `Domain` NUNCA deve referenciar projetos externos, drivers de banco de dados ou bibliotecas de mensageria.
- O fluxo de dependência é estritamente centrípeto: `Api` ➔ `Application` ➔ `Domain`, com `Infrastructure` e `External` implementando contratos e interfaces definidos no `Domain`.

## 2. Modelagem MongoDB e BSON
- O banco de dados do serviço é exclusivamente MongoDB 7.0 (NoSQL).
- Nenhuma foreign key relacional direta com PostgreSQL ou outros microsserviços é permitida; correlações são sempre feitas via identificadores canônicos (`IdOrdemServico`, `IdOrcamento`).
- As coleções devem ser mapeadas explicitamente em `BsonClassMappings.cs` utilizando convenções padrão: `CamelCaseElementNameConvention`, `IgnoreExtraElementsConvention(true)` e `GuidRepresentation.Standard`.
- Todo orçamento aprovado armazena um snapshot imutável dos itens praticados naquele momento.

## 3. Padrão Saga Coreografada
- Transições de status de pagamento devem emitir eventos para a mensageria (`techchallenge-events`).
- Em caso de falha de cobrança ou expiração de tempo limite, acionar a compensação emitindo `PagamentoCanceladoCompensacaoEvent` para que o serviço de OS reverta o estado e estorne reservas de peças.
