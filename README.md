# Sistema Integrado de Inventário e Identificação de Máquinas OCS

Solução completa, leve e compatível com **Windows 7 (32/64 bits), Windows 10 e Windows 11** para inventariar equipamentos e sincronizar identificadores no **OCS Inventory NG Server**.

---

## 📁 Estrutura do Projeto

```text
orchestration_ocs_inventory_system/
│
├── scripts/                          # [ETAPA 1] Scripts para GPO
│   └── install_ocs_agent.bat         # Instalação silenciosa do OCS Agent (Batch Puro, Zero PowerShell)
│
├── database/                         # [ETAPA 2] Banco de Dados
│   └── schema.sql                    # Tabela computadores_cadastro no MySQL/MariaDB
│
├── api/                              # [ETAPA 2] Ingestão Web (PHP)
│   ├── config.php                    # Credenciais de acesso ao banco
│   └── cadastrar.php                 # Endpoint de recepção dos dados do cliente
│
├── client_app/                       # [ETAPA 2] Aplicação Cliente Executável
│   ├── CadastroPatrimonio.exe        # Binário compilado (.NET 3.5 Nativo, 100% autônomo)
│   ├── Program.cs                    # Validação de execução única e Mutex
│   ├── MainForm.cs                   # Lógica da interface gráfica e envio HTTP
│   ├── MainForm.Designer.cs          # Layout da janela Windows Forms
│   ├── SystemInfoCollector.cs        # Coleta WMI (Serial BIOS, SO, Arch, Hostname, Usuário)
│   ├── RegistryHelper.cs             # Gravação e checagem de flags no Registro
│   ├── AppConfig.cs                  # Configurações do cliente
│   ├── app.manifest                  # Manifest para permissões e compatibilidade de OS
│   └── build.bat                     # Script de compilação via csc.exe nativo do Windows
│
├── sync/                             # [ETAPA 3] Sincronização no OCS Server
│   ├── config_sync.php               # Configuração do banco do OCS
│   ├── sync_ocs_patrimonio.php       # Script CLI em PHP para rodar no Cron
│   └── sync_ocs_patrimonio.py       # Script alternativo em Python 3
│
└── docs/                             # Documentação e Guias de Implantação
    ├── etapa1_gpo_instrucoes.md      # Guia de implantação do OCS Agent via GPO
    ├── etapa2_backend_instrucoes.md  # Guia do banco MySQL e API PHP
    ├── etapa2_aplicacao_instrucoes.md# Guia da aplicação executável de cadastro
    └── etapa3_sincronizacao_instrucoes.md # Guia de agendamento da sincronização
```

---

## 🚀 Resumo do Fluxo de Funcionamento

1. **Etapa 1:** A GPO de Startup executa [install_ocs_agent.bat](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/scripts/install_ocs_agent.bat) (sem PowerShell), instala o OCS Agent 2.11 e define a TAG com o `%COMPUTERNAME%`.
2. **Etapa 2:** No logon, o [CadastroPatrimonio.exe](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/CadastroPatrimonio.exe) verifica se o computador já foi registrado. Se não, exibe a tela, coleta dados do sistema e solicita Nome, Patrimônio e Setor. Ao enviar com sucesso para a [API](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/api/cadastrar.php), grava a flag no Registro e não incomoda mais o usuário.
3. **Etapa 3:** O script [sync_ocs_patrimonio.php](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/sync/sync_ocs_patrimonio.php) roda via Cron no OCS Server, localiza o computador pelo Hostname e atualiza seu nome para `HOSTNAME-PATRIMONIO` (preservando todo o inventário de hardware e software).
