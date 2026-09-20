# Guia de Implantação Fora do Active Directory (Workgroup / Redes Isoladas)

Este documento orienta a equipe técnica sobre a instalação e distribuição do sistema de inventário OCS e do formulário de patrimônio em **máquinas fora do domínio** (computadores em Workgroup, filiais sem controlador de domínio, home office ou redes corporativas segmentadas).

---

## 1. Visão Geral dos Scripts Workgroup

Na pasta `scripts/` existem dois scripts dedicados para instalação sem depender de GPO:

| Script | Finalidade | Comportamento da TAG OCS |
|---|---|---|
| [`instalar_workgroup.bat`](../scripts/instalar_workgroup.bat) | Instalação padrão, rápida e sem perguntas | Usa automaticamente `%COMPUTERNAME%` como TAG |
| [`instalar_workgroup_tag_manual.bat`](../scripts/instalar_workgroup_tag_manual.bat) | Instalação assistida / identificação direta | Abre prompt solicitando a digitação da TAG (ex: `VIC-123456`) |

Ambos os scripts realizam todo o ciclo em uma única execução:
1. Verificam privilégios de **Administrador**.
2. Instalam o **OCS Inventory Agent 2.11** de forma silenciosa (detectando 32 ou 64 bits automaticamente).
3. Copiam o `CadastroPatrimonio.exe` para `C:\Program Files\InventarioPatrimonio\`.
4. Gravam a URL da API externa e a chave `Run` no Registro do Windows (`HKLM`) para persistência nos próximos logons.
5. Abrem o formulário de cadastro imediatamente se houver usuário conectado.

---

## 2. Configurando o IP / Host do Servidor

Abra o script desejado em qualquer editor de texto (Notepad, VS Code) e altere a variável `SERVER_HOST` na linha 17:

```bat
:: Defina o IP externo ou Host FQDN do servidor OCS:
set "SERVER_HOST=200.x.x.x"
```

> **Nota:** Não é necessário recompilar o executável `CadastroPatrimonio.exe`. O instalador grava o endereço do servidor na chave de Registro `HKLM\Software\OCS_Inventario\ApiEndpointUrl`, e o aplicativo lê essa configuração dinamicamente.

---

## 3. Preparando o Pacote de Distribuição

Para levar aos computadores (via pendrive, pasta compartilhada ou ferramenta de suporte remoto), mantenha os seguintes arquivos juntos na mesma pasta:

```text
pacote_workgroup/
│
├── instalar_workgroup.bat            # (ou instalar_workgroup_tag_manual.bat)
├── install_ocs_agent.bat             # Orquestrador silencioso do agente
├── OCS-Agent-2.11-x86.exe            # Instalador 32 bits (do OcsPackager)
├── OCS-Agent-2.11-x64.exe            # Instalador 64 bits (do OcsPackager)
└── CadastroPatrimonio.exe            # Formulário de cadastro de patrimônio
```

---

## 4. Instruções de Instalação Passo a Passo

### Cenário A: Usando a Instalação Automática (`instalar_workgroup.bat`)
1. Copie o pacote para a máquina local (ex: `C:\Temp\OCS`).
2. Clique com o **botão direito** em `instalar_workgroup.bat` e selecione **"Executar como Administrador"**.
3. O script roda de forma 100% automatizada e finaliza exibindo a mensagem de sucesso.

### Cenário B: Usando a Versão com TAG Manual (`instalar_workgroup_tag_manual.bat`)
1. Clique com o **botão direito** em `instalar_workgroup_tag_manual.bat` e selecione **"Executar como Administrador"**.
2. Será solicitado no prompt:
   ```text
   Digite a TAG para este computador (Ex: VIC-123456 ou tecle ENTER para PC-USUARIO):
   ```
3. Digite o código desejado e pressione `ENTER`. O agente será instalado e o inventário inicial será enviado imediatamente ao servidor já com essa TAG.

---

## 5. Como Funciona a Persistência após o Cadastro

* O aplicativo `CadastroPatrimonio.exe` fica registrado em:
  `HKLM\Software\Microsoft\Windows\CurrentVersion\Run`
* Quando o usuário preenche o formulário e envia com sucesso:
  * Os dados são gravados no banco via API.
  * O aplicativo grava a flag de conclusão (`HKLM\Software\OCS_Inventario\CadastroConcluido = 1`).
  * Nos próximos boots/logons, o programa verifica a flag e **encerra em menos de 10 milissegundos**, sem exibir nenhuma janela para o usuário.

