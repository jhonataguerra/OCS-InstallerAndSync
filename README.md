# 🖥️ Sistema Integrado de Inventário e Identificação de Máquinas — OCS Inventory NG

Solução completa, leve, segura e compatível com **Windows 7 (32/64 bits), Windows 10 e Windows 11** para automatizar o inventário de equipamentos corporativos, coletar dados patrimoniais e sincronizar a identificação no **OCS Inventory NG Server**.

---

## 📑 Sumário

1. [Visão Geral e Arquitetura](#-visão-geral-e-arquitetura)
2. [Estrutura do Repositório](#-estrutura-do-repositório)
3. [Guia de Implantação Passo a Passo](#-guia-de-implantação-passo-a-passo)
   * [Passo 1: Gerar o Pacote OCS no OcsPackager](#passo-1-gerar-o-pacote-ocs-no-ocspackager)
   * [Passo 2: Distribuir o OCS Agent via GPO (Startup)](#passo-2-distribuir-o-ocs-agent-via-gpo-startup)
   * [Passo 3: Configurar o Banco de Dados e a API de Ingestão](#passo-3-configurar-o-banco-de-dados-e-a-api-de-ingestão)
   * [Passo 4: Distribuir a Aplicação de Cadastro (Logon)](#passo-4-distribuir-a-aplicação-de-cadastro-logon)
   * [Passo 5: Configurar a Sincronização Automática no OCS Server](#passo-5-configurar-a-sincronização-automática-no-ocs-server)
4. [Segurança e Matriz de Criticidade](#-segurança-e-matriz-de-criticidade)
5. [Dúvidas Frequentes e Resolução de Problemas](#-dúvidas-frequentes-e-resolução-de-problemas)

---

## 📐 Visão Geral e Arquitetura

O projeto conecta **3 etapas integradas**:

```text
[ Active Directory / GPO ]
       │
       ├─► (1. GPO Startup) ──► install_ocs_agent.bat (Batch Puro, Sem PowerShell)
       │                           │
       │                           └─► Detecta x86/x64 e instala OCS-Agent-2.11-x86.exe ou OCS-Agent-2.11-x64.exe (/TAG=%COMPUTERNAME%)
       │                                     │
       │                                     ▼
       │                              [ OCS Server ] ◄──────────────────────────────┐
       │                              (Inventário Base: HW/SW + Hostname na TAG)    │
       │                                                                            │
       └─► (2. GPO Logon)   ──► CadastroPatrimonio.exe (.NET 3.5 Nativo)            │
                                   │                                                │
                                   ├─► Verifica se já concluiu (Encerra em <10ms)   │
                                   ├─► Coleta WMI (Serial BIOS, SO, Arch, Hostname) │
                                   ├─► Formulário: Responsável, Nº Patrimônio, Setor│
                                   ├─► Bloqueio: 10s (Tolerância) ou 2min (7+ dias) │
                                   │                                                │
                                   ▼ (POST HTTP + X-API-TOKEN)                      │
                         [ API PHP / cadastrar.php ]                                │
                                   │                                                │
                                   ▼ (UPSERT com Chave Única no Hostname)           │
                         [ MariaDB / MySQL ]                                        │
                         (Tabela: computadores_cadastro)                            │
                                   │                                                │
                                   ▼                                                │
       (3. Cron no Servidor) ──► sync_ocs_patrimonio.php ───────────────────────────┘
                                 Localiza por hardware.NAME = hostname
                                 Atualiza SOMENTE accountinfo.TAG = prefixo + patrimonio
                                 PAC → PACO-{patrimonio} | PLA/DES/FAZ → VIC-{patrimonio} | outros → LOCAL-{patrimonio}
                                 Exemplo: PLA-PC01-123456 → VIC-123456 | hardware.NAME preservado
```

---

## 📁 Estrutura do Repositório

```text
simulate_ocs_seven_days/
│
├── scripts/                                     # Scripts de instalação e automação
│   ├── install_ocs_agent.bat                    # Instalação silenciosa do OCS Agent (Batch Puro)
│   ├── instalar_workgroup.bat                   # Instalação para ambientes sem domínio (Workgroup)
│   ├── instalar_workgroup_tag_manual.bat        # Instalação Workgroup com TAG configurada manualmente
│   ├── generate_security_report_pdf.py          # Gerador do relatório de segurança em PDF
│   ├── generate_test_roadmap_pdf.py             # Gerador do roteiro de testes de homologação em PDF
│   └── run_tests_and_security.py               # Orquestrador: testes + gate de segurança (gate-PDF)
│
├── database/                                    # Banco de Dados
│   └── schema.sql                              # Tabela computadores_cadastro (MySQL / MariaDB)
│
├── api/                                         # API PHP de Ingestão (Hospedada no OCS Server)
│   ├── config.php                              # Credenciais e Token X-API-TOKEN
│   └── cadastrar.php                           # Endpoint protegido e sanitizado
│
├── client_app/                                  # Aplicação Windows Forms (.NET 3.5 AnyCPU)
│   ├── CadastroPatrimonio.exe                  # Executável compilado pronto para distribuição
│   ├── CadastroPatrimonio.exe.config           # Configuração de compatibilidade .NET (supportedRuntime)
│   ├── Program.cs                              # Mutex de instância única e checagem de registro
│   ├── MainForm.cs                             # Interface, filtros PT-BR e temporizador
│   ├── MainForm.Designer.cs                    # Layout moderno da janela
│   ├── SystemInfoCollector.cs                  # Coleta WMI com filtro de BIOS genérica
│   ├── RegistryHelper.cs                       # Controle de prazos e execução única
│   ├── AppConfig.cs                            # Configurações de API e Token
│   ├── app.manifest                            # Manifest de compatibilidade (Win7 a Win11)
│   ├── app.manifest.xml                        # Cópia do manifest em formato XML explícito
│   ├── iniciar_cadastro.bat                    # Atalho batch para abrir o executável manualmente
│   └── build.bat                               # Compilador nativo via csc.exe
│
├── sync/                                        # Processo de Sincronização OCS
│   ├── config_sync.php                         # Conexão com a base nativa do OCS
│   ├── sync_ocs_patrimonio.php                 # Script CLI principal em PHP (com prefixação de TAG)
│   ├── sync_ocs_patrimonio_apenas_patrimonio.php # Variante: atualiza apenas o nº de patrimônio na TAG
│   ├── sync_ocs_patrimonio_old.php             # Versão anterior do script de sincronização (referência)
│   └── sync_ocs_patrimonio.py                  # Script alternativo em Python 3
│
├── tests/                                       # Suite de Testes Automatizados
│   ├── README_testes.md                        # Documentação da suite de testes
│   ├── run_tests.bat                           # Wrapper batch para execução dos testes
│   └── test_install_agent.ps1                  # Suite completa de testes (PowerShell 3.0+, T-01 a T-08)
│
├── utils/                                       # Utilitários e Instaladores
│   ├── Agents/                                 # Instaladores originais do OCS Agent (x86 e x64)
│   ├── Server/                                 # Arquivos do servidor OCS
│   ├── OCS-Agent-2.11-Universal.exe            # Pacote Universal pré-gerado (OcsPackager)
│   └── Parametros Packager.txt                 # Parâmetros de linha de comando para o OcsPackager
│
└── docs/                                        # Manuais e Relatórios
    ├── relatorio_seguranca_matriz_criticidade.pdf  # Relatório ilustrado de auditoria de segurança
    ├── roteiro_testes_homologacao.pdf              # Roteiro de testes de homologação gerado automaticamente
    ├── etapa1_gpo_instrucoes.md                # Manual do OCS Agent via GPO
    ├── etapa2_backend_instrucoes.md            # Manual do Banco MySQL e API
    ├── etapa2_aplicacao_instrucoes.md          # Manual do Executável de Cadastro + Homologação
    └── etapa3_sincronizacao_instrucoes.md      # Manual do Crontab de Sincronização
```

---

## 🚀 Guia de Implantação Passo a Passo

### Passo 1: Gerar os Pacotes OCS no OcsPackager (Dual x86/x64)

> **Atalho disponível:** O arquivo `utils/OCS-Agent-2.11-Universal.exe` já é um pacote Universal pré-gerado com os parâmetros corretos. Se preferir usá-lo diretamente, pule para o Passo 2.

Para gerar **dois pacotes separados** (x86 e x64) — o `install_ocs_agent.bat` detecta a arquitetura e escolhe automaticamente:

**Pacote 32 bits:**
1. Extraia `OCS-Windows-Agent-2.11.0.1_x86.zip` de `utils/Agents/`.
2. Abra o **OcsPackager.exe** (em `utils/Agents/OCS-Windows-Packager-2.8.1.zip`) e configure:
   * **Exe file:** `OCS-Windows-Agent-Setup-x86.exe`
   * **Certificate / Other files:** *(Deixe em branco)*
   * **Command line options:**
     ```text
     /S /NOSPLASH /NO_SYSTRAY /SERVER=http://192.168.2.48/ocsinventory /SSL=0 /DEBUG=2 /TAG=%COMPUTERNAME% /NOW
     ```
   * **Label:** `OCS-Agent-2.11-x86`
3. Gere e renomeie para **`OCS-Agent-2.11-x86.exe`**.

**Pacote 64 bits:**
1. Extraia `OCS-Windows-Agent-2.11.0.1_x64.zip` de `utils/Agents/`.
2. Mesma configuração, com **Exe file:** `OCS-Windows-Agent-Setup-x64.exe` e **Label:** `OCS-Agent-2.11-x64`.
3. Gere e renomeie para **`OCS-Agent-2.11-x64.exe`**.

> Parâmetros completos em `utils/Parametros Packager.txt` (`http://192.168.2.48/ocsinventory`).


---

### Passo 2: Distribuir o OCS Agent via GPO (Startup)
1. Coloque o [install_ocs_agent.bat](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/simulate_ocs_seven_days/scripts/install_ocs_agent.bat), o `OCS-Agent-2.11-x86.exe` e o `OCS-Agent-2.11-x64.exe` (ou `OCS-Agent-2.11-Universal.exe`) na mesma pasta de rede compartilhada (ex: `\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\ocs`).
2. No **GPMC (Group Policy Management Console)**:
   * Edite a GPO de computadores.
   * Vá em: `Configurações do Computador` -> `Políticas` -> `Configurações do Windows` -> `Scripts (Inicialização/Encerramento)` -> **Inicialização (Startup)**.
   * Aponte para: `\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\ocs\install_ocs_agent.bat`.
3. O script roda nativamente como `SYSTEM` antes do logon, não utiliza PowerShell e verifica se o serviço já existe para não reinstalar a cada boot.

---

### Passo 3: Configurar o Banco de Dados e a API de Ingestão
1. **Importar Tabela no MySQL/MariaDB:**
   No servidor Linux onde roda o OCS:
   ```bash
   mysql -u root -p ocsweb < database/schema.sql
   ```
2. **Publicar a API no Apache:**
   ```bash
   mkdir -p /var/www/html/cadastro_api
   cp api/config.php /var/www/html/cadastro_api/
   cp api/cadastrar.php /var/www/html/cadastro_api/
   chown -R www-data:www-data /var/www/html/cadastro_api/
   ```
3. Edite o `/var/www/html/cadastro_api/config.php` informando a senha do banco MySQL e garantindo que o `api_token` corresponda ao configurado no executável.

---

### Passo 4: Distribuir a Aplicação de Cadastro (Logon)
1. Copie **apenas o arquivo [CadastroPatrimonio.exe](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/CadastroPatrimonio.exe)** para a pasta da GPO de usuários (ex: `\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\CadastroPatrimonio.exe`).
2. No **GPMC**:
   * Edite a GPO de usuários.
   * Vá em: `Configurações do Usuário` -> `Políticas` -> `Configurações do Windows` -> `Scripts (Logon/Logoff)` -> **Logon**.
   * Adicione o executável.
3. **Comportamento do usuário:**
   * **Primeiros 7 dias:** Exibe aviso informativo em amarelo com contador de 10s para leitura antes de permitir fechar temporariamente.
   * **Após 7 dias:** Exibe aviso em vermelho informando que o preenchimento agora é obrigatório (bloqueio de fechamento de 2 minutos).
   * **Após gravar:** O servidor valida o token, grava no banco e o executável grava a flag definitiva no Registro (`HKLM`/`HKCU`), encerrando de forma silenciosa em logons subsequentes.

---

### Passo 5: Configurar a Sincronização Automática no OCS Server
1. Copie a pasta `sync/` para o servidor OCS:
   ```bash
   mkdir -p /var/www/html/cadastro_api/sync
   cp sync/config_sync.php /var/www/html/cadastro_api/sync/
   cp sync/sync_ocs_patrimonio.php /var/www/html/cadastro_api/sync/
   ```
2. No terminal do Linux, abra o agendador:
   ```bash
   crontab -e
   ```
3. Adicione a linha para rodar a cada 10 minutos:
   ```cron
   */10 * * * * /usr/bin/php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php > /dev/null 2>&1
   ```
4. O script localiza pelo **Hostname** (`hardware.NAME`) e atualiza **somente `accountinfo.TAG`** para `PREFIXO-PATRIMONIO` (`PAC→PACO-`, `PLA/DES/FAZ→VIC-`, outros `LOCAL-`), preservando `hardware.NAME`, `hardware.USERID` e todo histórico de hardware/software. Ex.: `PLA-PC01 + 123456 → VIC-123456`. Suporta `--dry-run` e `--verbose`.

---

## 🔒 Segurança e Matriz de Criticidade

O projeto passou por auditoria rigorosa de segurança, com relatório ilustrado disponível em [relatorio_seguranca_matriz_criticidade.pdf](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/docs/relatorio_seguranca_matriz_criticidade.pdf).

### Destaques das Proteções Implementadas:
* ✅ **Token Criptográfico (`X-API-TOKEN`):** Impede que requisições avulsas forjem cadastros na rede.
* ✅ **Sanitização contra XSS:** Filtro `strip_tags()` e validação numérica estrita para o patrimônio.
* ✅ **Proteção contra Information Leak:** Mensagens de erro do banco de dados são mascaradas publicamente e gravadas em log protegido.
* ✅ **Filtro de BIOS Genérica:** Fallback inteligente caso a máquina retorne `"To be filled by O.E.M."`.
* ✅ **Permissões de Compartilhamento (Ação de TI):** Manter permissão NTFS do compartilhamento GPO como `Domain Users = Leitura/Execução` e `Domain Admins = Total`.

---

## ❓ Dúvidas Frequentes e Resolução de Problemas

### 1. Como simular 7 dias ou mais sem cadastro (modo obrigatório)?

A aplicação verifica a data de primeira execução em **3 locais, nessa ordem de prioridade**: `HKLM` → `HKCU` → arquivo `first_run.dat`. Para garantir a simulação, todos devem ser definidos (execute como **Administrador**):

```powershell
# 1. HKLM — prioridade máxima (requer Admin)
reg add "HKLM\Software\OCS_Inventario" /v PrimeiraExecucao /t REG_SZ /d "2026-08-10 08:00:00" /f

# 2. HKCU — prioridade secundária
reg add "HKCU\Software\OCS_Inventario" /v PrimeiraExecucao /t REG_SZ /d "2026-08-10 08:00:00" /f

# 3. Arquivo first_run.dat — fallback
$dir = "$env:ProgramData\OCS_Inventario"
if (!(Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
Set-Content -Path "$dir\first_run.dat" -Value "2026-08-10 08:00:00" -Encoding UTF8
```

O app exibirá o **aviso vermelho** com bloqueio de 2 minutos. Veja detalhes completos em [etapa2_aplicacao_instrucoes.md](docs/etapa2_aplicacao_instrucoes.md).

### 2. Como resetar e reabrir o formulário em uma máquina de testes?

**Reset granular** (apaga apenas a data de primeira execução — o app registrará a data atual no próximo logon):

```powershell
# Remove os valores de data (o app vai registrar a data atual como nova primeira execução)
reg delete "HKLM\Software\OCS_Inventario" /v PrimeiraExecucao /f 2>$null
reg delete "HKCU\Software\OCS_Inventario" /v PrimeiraExecucao /f 2>$null
Remove-Item "C:\ProgramData\OCS_Inventario\first_run.dat" -ErrorAction SilentlyContinue
```

**Reset completo** (apaga também a flag de conclusão de cadastro):

```cmd
reg delete "HKCU\Software\OCS_Inventario" /f
reg delete "HKLM\Software\OCS_Inventario" /f
del /f /q "%ProgramData%\OCS_Inventario\*.*" 2>nul
del /f /q "%LocalAppData%\OCS_Inventario\*.*" 2>nul
```

### 3. Onde consultar os logs de diagnóstico?
* **Instalação do OCS Agent no Windows:** `C:\Windows\Temp\ocs_agent_install.log`
* **Log detalhado do OCS Agent:** `C:\ProgramData\OCS Inventory NG\Agent\OCSInventory.log`
* **Log de erros da API de Ingestão:** `/var/log/ocs_cadastro_api_error.log` (no servidor Linux)
* **Log da Sincronização de Nomes:** `/var/log/ocs_sync_patrimonio.log` (no servidor Linux)

### 4. Como recompilar o executável caso eu altere o IP do servidor?
Edite o arquivo [AppConfig.cs](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/AppConfig.cs) e execute no Prompt de Comando:
```cmd
cd client_app
build.bat
```
O script utiliza o compilador C# nativo (`csc.exe`) do Windows e gera o binário otimizado.

---

### 📦 Pacote Completo do Projeto
* Arquivo ZIP pronto para implantação: [ocs_inventory_system.zip](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/ocs_inventory_system.zip)
