# Suite de Testes — OCS Installer & Sync

Este documento descreve como executar e interpretar os testes automatizados
da **Etapa 1** do projeto OCS InstallerAndSync.

---

## Pré-requisitos

| Requisito | Versão Mínima | Observação |
|---|---|---|
| Windows | 7 / 10 / 11 | Qualquer arquitetura |
| PowerShell | 3.0 | Nativo no Win8.1+; instalar via WMF 3.0 no Win7 |
| Python | 3.8+ | Apenas para o orquestrador com gate de segurança |
| reportlab | qualquer | Apenas para geração do PDF: `pip install reportlab` |

Os testes **não** requerem que o OCS Agent esteja instalado.
Os testes **não** fazem nenhuma alteração permanente no sistema — ambientes
são criados em `%TEMP%` e removidos ao final de cada teste.

---

## Executar os Testes

### Opção A — Wrapper Batch (mais simples)

```cmd
cd D:\DEV\GitHub\OCS1\tests
run_tests.bat
```

### Opção B — PowerShell direto

```powershell
cd D:\DEV\GitHub\OCS1\tests
powershell -ExecutionPolicy Bypass -File test_install_agent.ps1
```

### Opção C — Orquestrador completo (testes + gate de segurança)

```cmd
cd D:\DEV\GitHub\OCS1
python scripts\run_tests_and_security.py
```

---

## Casos de Teste

| ID | Categoria | Cenário Testado |
|---|---|---|
| **T-01** | Detecção de Arquitetura | `PROCESSOR_ARCHITECTURE=AMD64` (processo nativo 64-bit) → instalador `x64` selecionado |
| **T-02** | Detecção de Arquitetura | `PROCESSOR_ARCHITECTURE=x86` sem `ARCHITEW6432` (SO 32-bit puro) → instalador `x86` selecionado |
| **T-03** | Detecção de Arquitetura | `PROCESSOR_ARCHITEW6432=AMD64` (processo 32-bit em SO 64-bit / WOW64) → instalador `x64` selecionado |
| **T-04** | Idempotência | `OCSInventory.exe` presente em `ProgramFiles` → script encerra com `exit 0` sem reinstalar |
| **T-05** | Idempotência | `OCSInventory.exe` presente em `ProgramFiles(x86)` → script encerra com `exit 0` sem reinstalar |
| **T-06** | Resiliência / Erros | Nenhum instalador presente na pasta → `exit 1` e `ERRO CRITICO` no log |
| **T-07** | Log | Log gerado em `C:\Windows\Temp\ocs_agent_install.log` contém o `COMPUTERNAME` |
| **T-08** | Log | Falha crítica gera entrada com prefixo `ERRO CRITICO` no log |

---

## Interpretação dos Resultados

```
[OK]    T-01 PASS  Sistema 64-bit (AMD64 nativo) → instalador x64 selecionado
[OK]    T-02 PASS  Sistema 32-bit nativo → instalador x86 selecionado
[FALHA] T-06 FAIL  Instalador ausente → exit code 1 e "ERRO CRITICO" no log
        Detalhe: ExitCode=0. Esperado 1. Log: ...
```

- **`[OK] PASS`** — comportamento correto confirmado.
- **`[FALHA] FAIL`** — o script não se comportou como esperado. O campo `Detalhe` mostra o exit code real e o trecho do log para diagnóstico.

### Códigos de Saída

| Exit Code | Significado |
|---|---|
| `0` | Todos os testes passaram |
| `1` | Um ou mais testes falharam |
| `99` | Erro fatal — PowerShell não encontrado ou versão insuficiente |

---

## Gate de Segurança (Orquestrador Python)

O script `scripts/run_tests_and_security.py` integra os testes ao processo de
geração do **Relatório de Auditoria e Matriz de Criticidade** (PDF):

```
Testes → TODOS passam → gera docs/relatorio_seguranca_matriz_criticidade.pdf
Testes → ALGUMA falha → exibe instruções de correção e DIFERE a geração do PDF
```

Isso garante que o relatório de segurança reflita sempre um estado validado do
pipeline de instalação.

---

## Adicionando Novos Testes

1. Abra `tests/test_install_agent.ps1`.
2. Crie um bloco seguindo o padrão:
   ```powershell
   Write-Host "-- T-XX: Descricao do caso --" -ForegroundColor DarkCyan
   $testDir = New-TestEnvironment [flags...]
   try {
       $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides @{ ... }
       $passed = <condicao booleana>
       Register-TestResult 'T-XX' 'Descricao legivel' $passed
   }
   finally { Remove-TestEnvironment $testDir }
   ```
3. Execute a suite para verificar.

---

## Arquivos da Suite

```
tests/
├── README_testes.md          ← Este arquivo
├── run_tests.bat             ← Wrapper batch de execução
└── test_install_agent.ps1    ← Suite completa de testes (PowerShell 3.0+)
```
