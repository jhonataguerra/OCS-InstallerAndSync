# Guia de Implantação da Etapa 1 — Instalação do OCS Agent via GPO

Este documento orienta a equipe de TI na distribuição automatizada do **OCS Inventory Agent 2.11** utilizando **Active Directory (GPO)** e o script [install_ocs_agent.bat](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/scripts/install_ocs_agent.bat).

---

## 1. Arquivos Necessários na Pasta Compartilhada

Coloque os seguintes arquivos na mesma pasta de rede (ex: `\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\ocs` ou pasta de compartilhamento de software com permissão de leitura para `Domain Computers` / `Computadores do Domínio`):

1. `install_ocs_agent.bat` (Script de orquestração da instalação)
2. `OCS-Windows-Agent-Setup-x86.exe` (Pacote instalador 32 bits)
3. `OCS-Windows-Agent-Setup-x64.exe` (Pacote instalador 64 bits)

> **Nota:** Se os seus executáveis tiverem nomes diferentes, abra o arquivo `install_ocs_agent.bat` e ajuste as variáveis `INSTALLER_32` e `INSTALLER_64`.

---

## 2. Ajuste das Configurações no Script

Abra o arquivo [install_ocs_agent.bat](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/scripts/install_ocs_agent.bat) e ajuste a linha do servidor:

```bat
set "OCS_SERVER_URL=http://IP_OU_HOST_DO_OCS/ocsinventory"
set "OCS_SSL=0"
```

* Se usar HTTPS com certificado válido, configure `OCS_SSL=1`.
* Se usar HTTP padrão, mantenha `OCS_SSL=0`.

---

## 3. Configuração da Diretiva de Grupo (GPO)

Recomenda-se a execução como **Script de Inicialização de Computador (Computer Startup Script)**, pois roda com privilégios de `NT AUTHORITY\SYSTEM` antes do logon:

1. No **Group Policy Management Console (GPMC)**:
   * Crie ou edite uma GPO vinculada à Unidade Organizacional (OU) dos computadores (ex: `GPO_Deploy_OCS_Agent`).
2. Navegue até:
   * `Configurações do Computador` (Computer Configuration)
   * `Políticas` (Policies)
   * `Configurações do Windows` (Windows Settings)
   * `Scripts (Inicialização/Encerramento)` (Scripts Startup/Shutdown)
   * Clique duas vezes em **Inicialização (Startup)**.
3. Clique em **Adicionar (Add)**:
   * **Nome do Script (Script Name):** `\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\ocs\install_ocs_agent.bat`
   * **Parâmetros do Script:** *(Deixe em branco)*.
4. Salve e feche a GPO.

---

## 4. Como Funciona a Idempotência e a TAG

1. **Detecção de Arquitetura:** O script identifica se o sistema operacional é 32 bits ou 64 bits de forma nativa e seleciona o `.exe` correto.
2. **Prevenção de Reinstalação:** O script verifica se o serviço `OCS Inventory Service` já está registrado ou se o binário já existe. Se já estiver instalado, ele finaliza em milissegundos sem reinstalar.
3. **Atribuição da TAG:** O instalador é executado com `/TAG=%COMPUTERNAME%`, garantindo que o Hostname da máquina seja registrado como a TAG no servidor OCS.
4. **Log de Execução:** Todas as execuções gravam log detalhado em `C:\Windows\Temp\ocs_agent_install.log` para fácil diagnóstico.
