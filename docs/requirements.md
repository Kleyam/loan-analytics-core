# Requisitos — Loan Prepayment Intelligence Platform

> Documento vivo. Atualizado incrementalmente conforme decisões de modelagem e arquitetura são tomadas.

---

## Objetivo

Construir uma plataforma que permita a um banco/fintech prever a probabilidade de um empréstimo ser quitado antecipadamente (prepagamento, parcial ou total) e quantificar o impacto financeiro dessa antecipação sobre a receita futura de juros projetada permitindo decisões melhores de gestão de carteira e fluxo de caixa.

---

## Escopo

**Incluído no MVP:**
- Clientes Pessoa Física (PF) apenas
- Cadastro e gestão de empréstimos individuais
- Cálculo de score de probabilidade de prepagamento (via modelo de ML, fase futura)
- Cálculo de impacto financeiro projetado (receita de juros perdida) por prepagamento
- Dashboard de exposição por faixa de vencimento

**Explicitamente fora do MVP** (Requisitos Futuros):
- Pessoa Jurídica (PJ)
- Integração real ou simulada com sistema de conta corrente/saldo bancário
- Autenticação/autorização (Identity/JWT)
- Multi-tenant
- Simulador "e se a Selic mudar"
- Sistema de alertas configuráveis
- Módulo de recomendação de retenção
- Versionamento de modelo ML
- API pública documentada para terceiros

---

## Requisitos Funcionais

| Código | Descrição | Status |
|---|---|---|
| RF01 | O sistema deve permitir cadastrar um Cliente PF com dados pessoais e financeiros básicos | ✅ Modelado (entidade `Client`) |
| RF02 | O sistema deve permitir atualizar o Score de crédito de um Cliente já existente, com validação de faixa (0–1000) | ✅ Modelado (`Client.UpdateScore`) |
| RF03 | O sistema deve permitir atualizar o Índice Histórico de Inadimplência de um Cliente já existente, com validação de faixa (0–100%) | ✅ Modelado (`Client.ChangeDefaultRate`) |
| RF04 | O sistema deve calcular, sob demanda, o tempo de relacionamento (em anos) entre o Cliente e o banco, sem persistir esse valor | ✅ Modelado (propriedade computada) |
| RF05 | O sistema deve permitir cadastrar um Empréstimo vinculado a um Cliente existente | 🔶 Em andamento (entidade `Loan`) |
| RF06 | O sistema deve inicializar o saldo devedor (`BalanceOwed`) de um Empréstimo recém-criado como igual ao valor total emprestado (`LoanAmount`) | 🔶 Em andamento |
| RF07 | O sistema deve calcular a probabilidade de prepagamento (parcial ou total) de um Empréstimo nos próximos 30/60/90 dias | ⏳ Não iniciado (depende de Fase 3 — ML) |
| RF08 | O sistema deve calcular o impacto financeiro (receita de juros futura perdida) projetado para um Empréstimo, caso seja quitado antecipadamente | ⏳ Não iniciado |
| RF09 | O sistema deve agregar o impacto financeiro projetado por carteira inteira, para um horizonte de 90 dias | ⏳ Não iniciado |
| RF10 | O sistema deve exibir a exposição de risco de prepagamento agrupada por faixa de maturidade (0–6, 6–12, 12+ meses) | ⏳ Não iniciado (Fase 5 — Dashboard) |
| RF11 | O sistema deve recalcular diariamente o score de toda a carteira via job em background | ⏳ Não iniciado (Fase 4 — Hangfire) |

---

## Requisitos Não Funcionais

| Código | Descrição | Status |
|---|---|---|
| RNF01 | O domínio (`Domain`) não deve ter nenhuma dependência de frameworks externos (EF Core, ASP.NET, etc.) | ✅ Aplicado |
| RNF02 | Dependências entre camadas devem seguir a Dependency Rule: `Api` → `Application`/`Infrastructure` → `Domain`; `Domain` não depende de nada | ✅ Aplicado |
| RNF03 | Todo valor monetário ou percentual relevante para cálculo financeiro deve usar o tipo `decimal`, nunca `double`, para evitar erro de arredondamento binário | ✅ Aplicado |
| RNF04 | Toda Entity deve gerar sua própria identidade (`Guid`) no momento da criação, sem depender de banco de dados | ✅ Aplicado |
| RNF05 | Toda Entity deve proteger seus invariantes através de validação no construtor e em métodos de atualização controlados (sem setters públicos livres) | ✅ Aplicado |
| RNF06 | Dados derivados/calculáveis a partir de outras entidades não devem ser persistidos como campo fixo (evitar dessincronização) | ✅ Aplicado como princípio |
| RNF07 | O sistema deve ser capaz de testar regras de negócio do `Domain` sem depender de banco de dados real | ⏳ A validar quando testes forem escritos |

---

## Regras de Negócio

| Código | Descrição |
|---|---|
| RN01 | Um Cliente é uma Entity (possui identidade única e ciclo de vida contínuo), não um Value Object |
| RN02 | Um Empréstimo é uma Entity, pelo mesmo motivo — dois empréstimos criados com valores idênticos podem divergir de estado ao longo do tempo |
| RN03 | O MVP atende exclusivamente Pessoa Física; suporte a Pessoa Jurídica é explicitamente adiado |
| RN04 | `Score` é um valor externo ao Cliente (calculado por processo/modelo de ML), representado como inteiro de 0 a 1000 |
| RN05 | `HistoricalDefaultRate` é uma proporção (não uma contagem bruta de atrasos), representada na escala 0–100 por legibilidade humana; **requer normalização para 0–1 antes de ser consumida por qualquer modelo de ML** |
| RN06 | Valores derivados de outras entidades (ex.: total emprestado histórico, total investido) não são armazenados como campo do Cliente — são calculados sob demanda pela camada de Application a partir das entidades relacionadas |
| RN07 | Tempo de relacionamento do Cliente com o banco não é armazenado — é calculado a partir de `AdmissionDate` sempre que consultado |
| RN08 | Juros de um empréstimo incidem sobre o saldo devedor (`BalanceOwed`), que decresce ao longo do contrato — nunca sobre o valor original (`LoanAmount`) fixo |
| RN09 | Prepagamento parcial com **redução de prazo, mantendo valor de parcela** gera maior perda de receita de juros ao banco do que prepagamento com **redução de parcela, mantendo prazo** — pois reduz o tempo total em que o saldo devedor gera juros |
| RN10 | `Loan.BalanceOwed` nasce sempre igual a `Loan.LoanAmount` — não pode ser definido com outro valor na criação |
| RN11 | `Loan.ClientId` referencia a identidade (`Guid`) do Cliente, não o objeto `Client` inteiro — evitando acoplamento direto entre as entidades |

---

## Casos de Uso

| Código | Caso de Uso | Ator | Status |
|---|---|---|---|
| CU01 | Cadastrar novo Cliente PF | Operador do banco / sistema de originação | Domínio modelado |
| CU02 | Atualizar Score de um Cliente existente | Job de recálculo / modelo de ML | Domínio modelado |
| CU03 | Cadastrar novo Empréstimo para um Cliente existente | Operador do banco / sistema de originação | Domínio modelado |
| CU04 | Consultar tempo de relacionamento de um Cliente | Qualquer consumidor interno (dashboard, relatório) | Domínio modelado |
| CU05 | Consultar score de prepagamento de um Empréstimo específico | Gestor de carteira / API | Domínio modelado |
| CU06 | Consultar exposição agregada da carteira por faixa de vencimento | Gestor de carteira / Dashboard | Domínio modelado |
| CU07 | Recalcular score de toda a carteira (batch diário) | Job agendado (Hangfire) | Domínio modelado |

---

## Restrições

- Não há dados reais de banco disponíveis, dataset será sintético/simulado (Fase 2 do roadmap)
- Não há integração com sistema de conta corrente/saldo descartado do MVP por não haver fonte de dados real ou simulada disponível
- Projeto de portfólio individual, sem time decisões de arquitetura precisam ser simples o suficiente para serem implementadas e defendidas por uma única pessoa
- Sem autenticação/autorização implementada nesta fase assume-se acesso interno/de confiança por ora

---

## Dúvidas (em aberto — não decididas ainda)

| Código | Dúvida |
|---|---|
| DV01 | Qual o limite superior aceitável para `Loan.MonthlyLoanInterest` na validação (trava contra erro de input grosseiro, não uma regra de mercado real)? |
| DV02 | `Loan.LoanAgreement` (número de contrato): formato final ainda não definido (livre, ou com máscara/padrão específico?) |
| DV03 | Como o saldo devedor (`BalanceOwed`) será reduzido ao longo do tempo — método de amortização a ser modelado (parcela fixa tipo Price, SAC, outro?) |
| DV04 | A entidade `Loan` precisa de um método de aplicar prepagamento (parcial/total) ainda não modelado — vai decidir Opção A (reduz parcela) vs Opção B (reduz prazo) como comportamento configurável ou fixo? |
| DV05 | Entidade `Prepayment` ainda não modelada atributos e relação com `Loan` pendentes |
| DV06 | Estratégia de tratamento de concorrência (ex.: dois processos atualizando o mesmo Score simultaneamente) identificada como risco real, solução (lock/concorrência otimista) adiada para a camada de Infrastructure/EF Core |
| DV07 | Decisão de ML.NET vs microsserviço Python separado para servir o modelo preditivo ainda não discutida |

---

## Requisitos Futuros

| Código | Descrição |
|---|---|
| RFU01 | Suporte a clientes Pessoa Jurídica (PJ) |
| RFU02 | Integração real/simulada com sistema de conta corrente para consulta de saldo |
| RFU03 | Simulador "e se a Selic subir/cair X%" sobre o fluxo de caixa projetado |
| RFU04 | Sistema de alertas configuráveis por threshold de exposição por faixa de vencimento |
| RFU05 | Módulo de recomendação de retenção (sugestão de renegociação para clientes de alto risco/alto valor) |
| RFU06 | Autenticação e multi-tenant (simular múltiplos bancos/carteiras na mesma plataforma) |
| RFU07 | Versionamento e comparação de múltiplas versões do modelo preditivo ao longo do tempo |
| RFU08 | API pública documentada (Swagger/OpenAPI) para integração externa |