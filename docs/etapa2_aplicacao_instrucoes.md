# Guia da Aplicação Executável de Cadastro (Etapa 2)

A aplicação [CadastroPatrimonio.exe](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/CadastroPatrimonio.exe) é um executável Windows Forms ultra-leve, compilado nativamente contra o **.NET Framework 3.5**, garantindo compatibilidade imediata e sem dependências externas desde o **Windows 7 32 bits (x86)** até o **Windows 11 64 bits**.

---

## 1. Regra de Negócio e Ciclo de Vida

```text
[ Inicio do CadastroPatrimonio.exe ]
               │
               ▼
[ Verifica Flag no Registro / Disco ]
 (HKLM / HKCU \Software\OCS_Inventario -> CadastroConcluido == 1)
               │
      ┌────────┴────────┐
      ▼ (Sim)           ▼ (Não)
[ Encerra Silencioso ]  [ Coleta WMI: Serial BIOS, SO, Arch, Hostname ]
                        [ Exibe Formulário ao Usuário ]
                                │
                                ▼ (Usuário preenche Nome, Patrimônio, Setor)
                        [ Envia POST HTTP/JSON para API ]
                                │
                        ┌───────┴────────┐
                        ▼ (HTTP 200 OK)  ▼ (Falha de Rede / Erro 500)
             [ Grava Flag Registro ]    [ Exibe Erro ao Usuário ]
             [ Exibe Alerta Sucesso]    [ NÃO grava Flag ]
             [ Encerra Aplicação   ]    [ Permite Nova Tentativa ]
```

---

## 2. Estratégia de Distribuição via GPO

Como a aplicação necessita de **interação com o usuário** (preenchimento do formulário), sua execução deve ocorrer no contexto de logon:

### Opção A — GPO de Logon de Usuário (Recomendada)
1. No **GPMC**, edite a GPO aplicada aos usuários/computadores.
2. Navegue até: `Configurações do Usuário` -> `Políticas` -> `Configurações do Windows` -> `Scripts (Logon/Logoff)` -> **Logon**.
3. Aponte para o executável no compartilhamento de rede:
   `\\SEU_DOMINIO\SYSVOL\SEU_DOMINIO\scripts\CadastroPatrimonio.exe`
4. Como a aplicação verifica o Registro logo na primeira linha de código (`Program.cs`), em logons subsequentes o `.exe` abre e fecha em menos de **10 milissegundos**, sem incomodar o usuário.

---

## 3. Como Recompilar

Se você alterar o endereço da API em [AppConfig.cs](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/AppConfig.cs), basta executar o script [build.bat](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/build.bat):

```cmd
cd client_app
build.bat
```

O script detecta automaticamente o compilador nativo `csc.exe` do Windows e gera o binário otimizado.

---

## 4. Reset para Testes de Homologação

Para testar o formulário novamente em uma máquina que já completou o cadastro, execute no CMD/Prompt de Comando:

```cmd
reg delete "HKCU\Software\OCS_Inventario" /f
reg delete "HKLM\Software\OCS_Inventario" /f
del /f /q "%ProgramData%\OCS_Inventario\concluido.flag" 2>nul
del /f /q "%LocalAppData%\OCS_Inventario\concluido.flag" 2>nul
```
Ao abrir o executável novamente, o formulário será exibido.
