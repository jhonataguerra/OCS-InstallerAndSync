# Guia de Implantação da Etapa 1 — Instalação do OCS Agent via GPO

Este documento orienta a equipe de TI na distribuição automatizada do
**OCS Inventory Agent 2.11** utilizando **Active Directory (GPO)** e o script
[install_ocs_agent.bat](../scripts/install_ocs_agent.bat).

O script detecta automaticamente se o sistema é **32 bits (x86)** ou **64 bits (x64)**
e executa o Packager correto — sem dependência de PowerShell.

---

## 1. Pré-requisitos: Gerar os Dois Pacotes Packager

Antes de distribuir, gere **dois** executáveis separados usando o **OCSPackager.exe**
(disponível em `utils/Agents/OCS-Windows-Packager-2.8.1.zip`).

### Pacote 32 bits

1. Extraia `OCS-Windows-Agent-2.11.0.1_x86.zip` de `utils/Agents/`.
2. Abra o **OcsPackager.exe** e configure:
   * **Exe file:** `OCS-Windows-Agent-Setup-x86.exe`
   * **Certificate / Other files:** *(deixe em branco)*
   * **Command line options:**
     ```
     /S /NOSPLASH /NO_SYSTRAY /SERVER=http://SEU_IP/ocsinventory /SSL=0 /DEBUG=2 /TAG=%COMPUTERNAME% /NOW
     ```
   * **Label:** `OCS-Agent-2.11-x86`
3. Gere o pacote e renomeie o `.exe` para: **`OCS-Agent-2.11-x86.exe`**

### Pacote 64 bits

1. Extraia `OCS-Windows-Agent-2.11.0.1_x64.zip` de `utils/Agents/`.
2. Abra o **OcsPackager.exe** e configure:
   * **Exe file:** `OCS-Windows-Agent-Setup-x64.exe`
   * **Certificate / Other files:** *(deixe em branco)*
   * **Command line options:** *(mesmos parâmetros acima)*
   * **Label:** `OCS-Agent-2.11-x64`
3. Gere o pacote e renomeie o `.exe` para: **`OCS-Agent-2.11-x64.exe`**

---

## 2. Arquivos Necessários na Pasta Compartilhada

Coloque os seguintes arquivos na mesma pasta de rede — por exemplo:
`\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\ocs` — com permissão de
**Leitura/Execução** para `Computadores do Domínio (Domain Computers)`:

| Arquivo | Descrição |
|---|---|
| `install_ocs_agent.bat` | Script de orquestração da instalação |
| `OCS-Agent-2.11-x86.exe` | Packager OCS com agente **32 bits** |
| `OCS-Agent-2.11-x64.exe` | Packager OCS com agente **64 bits** |

> **Nota:** Caso queira usar nomes diferentes para os executáveis, abra
> `install_ocs_agent.bat` e ajuste as variáveis `INSTALLER_32` e `INSTALLER_64`
> na seção `[1] CONFIGURACOES DO AMBIENTE`.

---

## 3. Ajuste das Configurações no Script

Abra [install_ocs_agent.bat](../scripts/install_ocs_agent.bat) e ajuste:

```bat
set "OCS_SERVER_URL=http://IP_OU_HOST_DO_OCS/ocsinventory"
set "OCS_SSL=0"
set "INSTALLER_32=OCS-Agent-2.11-x86.exe"
set "INSTALLER_64=OCS-Agent-2.11-x64.exe"
```

* Use `OCS_SSL=1` para conexões HTTPS com certificado válido.
* Mantenha `OCS_SSL=0` para HTTP padrão em redes internas.

---

## 4. Configuração da Diretiva de Grupo (GPO)

Execute o script como **Script de Inicialização de Computador (Computer Startup Script)**
— roda com privilégios `NT AUTHORITY\SYSTEM` antes do logon:

1. No **Group Policy Management Console (GPMC)**:
   * Crie ou edite uma GPO vinculada à OU dos computadores (ex: `GPO_Deploy_OCS_Agent`).
2. Navegue até:
   `Configurações do Computador` → `Políticas` → `Configurações do Windows`
   → `Scripts (Startup/Shutdown)` → clique em **Startup**.
3. Clique em **Adicionar (Add)**:
   * **Script Name:** `\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\ocs\install_ocs_agent.bat`
   * **Parâmetros:** *(deixe em branco)*
4. Salve e feche a GPO.

---

## 5. Como Funciona: Detecção de Arquitetura e Idempotência

### Detecção de Arquitetura (Seção [4] do script)

O script usa **exclusivamente variáveis de ambiente nativas do Windows** — sem
PowerShell, sem WMI, sem reg query — com cobertura completa de 3 cenários:

| Cenário | `PROCESSOR_ARCHITECTURE` | `PROCESSOR_ARCHITEW6432` | Resultado |
|---|---|---|---|
| SO 64-bit, processo nativo 64-bit | `AMD64` | não existe | → `x64` |
| SO 64-bit, processo 32-bit (WOW64) | `x86` | `AMD64` | → `x64` |
| SO 32-bit puro | `x86` | não existe | → `x86` |

### Idempotência (Seção [3] do script)

Antes de instalar, o script verifica:
1. Se o **serviço** `OCS Inventory Service` já está registrado (`sc query`).
2. Se o **binário** `OCSInventory.exe` já existe em `ProgramFiles` ou `ProgramFiles(x86)`.

Se qualquer condição for verdadeira, o script encerra imediatamente com `exit 0`
sem reinstalar — ideal para boots repetidos via GPO.

### Tag e Log

* **TAG:** O parâmetro `/TAG=%COMPUTERNAME%` registra automaticamente o hostname
  como tag do equipamento no OCS Server.
* **Log:** Cada execução grava em `C:\Windows\Temp\ocs_agent_install.log`,
  incluindo a arquitetura detectada, o instalador selecionado e o código de saída.

---

## 6. Testes e Validação

Antes de distribuir via GPO, valide o script em uma máquina de homologação:

```powershell
# Na máquina de desenvolvimento (PowerShell como administrador):
cd D:\DEV\GitHub\OCS1\tests
powershell -ExecutionPolicy Bypass -File test_install_agent.ps1
```

Ou via orquestrador completo (testes + relatório de segurança):

```cmd
cd D:\DEV\GitHub\OCS1
python scripts\run_tests_and_security.py
```

Consulte [tests/README_testes.md](../tests/README_testes.md) para detalhes
sobre os casos de teste e interpretação dos resultados.

---

## 7. Como Forçar Reinstalação para Testes

Para forçar reinstalação em uma máquina específica, altere temporariamente no script:

```bat
set "FORCE_REINSTALL=1"
```

Volte para `0` antes de redistribuir via GPO.

Para redefinir o formulário de cadastro (Etapa 2) na mesma máquina:

```cmd
reg delete "HKCU\Software\OCS_Inventario" /f
reg delete "HKLM\Software\OCS_Inventario" /f
del /f /q "%ProgramData%\OCS_Inventario\*.*" 2>nul
del /f /q "%LocalAppData%\OCS_Inventario\*.*" 2>nul
```
