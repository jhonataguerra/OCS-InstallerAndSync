# Guia da Aplicação Executável de Cadastro (Etapa 2)

A aplicação [CadastroPatrimonio.exe](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/CadastroPatrimonio.exe) é um executável Windows Forms ultra-leve, compilado nativamente contra o **.NET Framework 3.5**, garantindo compatibilidade imediata e sem dependências externas desde o **Windows 7 32 bits (x86)** até o **Windows 11 64 bits**.

---

## 1. Interface Gráfica e Campos do Formulário

A interface foi estruturada seguindo padrões modernos de design e tipografia em **Português (Brasil)**:

* **Nome do Responsável (`txtNome`):**
  * Aceita **exclusivamente letras** (incluindo caracteres acentuados `á, é, í, ó, ú, ã, õ, ç, ê, ô`), espaços, apóstrofo e hífen.
  * Números e caracteres especiais são bloqueados na digitação e sanitizados automaticamente se colados.
* **Nº de Patrimônio (`txtPatrimonio`):**
  * Aceita **estritamente dígitos numéricos (`0` a `9`)**.
* **Setor / Local (`txtSetor`):**
  * Campo para indicação da lotação ou departamento do equipamento.
* **Painel de Informações Técnicas:**
  * Exibe em modo somente-leitura o Hostname, Usuário do Domínio, Serial da BIOS (com filtro de BIOS genérica) e Sistema Operacional.

---

## 2. Política de Prazos e Forçamento de Preenchimento

```text
[ Execução no Logon ]
         │
         ▼
[ Verifica Flag no Registro / Disco ] ──(Concluído == 1)──► [ Encerra em <10ms ]
         │
         ▼ (Não concluído)
[ Calcula dias desde a 1ª Execução ]
         │
    ┌────┴───────────────────────────────┐
    ▼ (Dentro dos Primeiros 7 Dias)      ▼ (Após 7 Dias - Prazo Expirado)
[ Aviso em Amarelo ]                 [ Aviso em Vermelho ]
"Preenchimento pendente ({0} dias).   "Atenção: O prazo expirou e
Após enviado, esta tela não será      tornou-se OBRIGATÓRIO."
mais exibida."                           │
    │                                [ Cronômetro: 2 Minutos (120s) ]
[ Cronômetro: 10 Segundos ]              │
    │                                    │
    └────────────────┬───────────────────┘
                     ▼
[ Envia POST HTTP/JSON com Token de Segurança (X-API-TOKEN) ]
                     │
        ┌────────────┴────────────┐
        ▼ (HTTP 200 OK)           ▼ (Falha de Rede / Erro 500)
[ Grava Flag Definitiva ]   [ Exibe Mensagem Amigável ]
[ Encerra Aplicação    ]   [ NÃO grava Flag ]
                            [ Usuário tenta no próximo logon ]
```

---

## 3. Segurança e Distribuição Corporativa

* **Token de Segurança Integrado:** A aplicação envia no cabeçalho HTTP o token criptográfico `X-API-TOKEN`, impedindo injeção de dados falsos por terceiros na rede.
* **Distribuição via GPO:** Publique apenas o arquivo [CadastroPatrimonio.exe](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/CadastroPatrimonio.exe) no compartilhamento de logon (`\\SEU_DOMINIO\SYSVOL\...`).

---

## 4. Homologação — Simular 7 Dias ou Mais Sem Cadastro

Para acionar o **modo obrigatório** (aviso vermelho + bloqueio de 2 minutos) em uma máquina de teste, é necessário definir a data de primeira execução em **todos os locais** que a aplicação verifica, na seguinte ordem de prioridade:

| Prioridade | Local | Requer Admin? |
|---|---|---|
| 1ª | `HKLM\Software\OCS_Inventario\PrimeiraExecucao` | ✅ Sim |
| 2ª | `HKCU\Software\OCS_Inventario\PrimeiraExecucao` | ❌ Não |
| 3ª | `C:\ProgramData\OCS_Inventario\first_run.dat` | ❌ Não |

> **Atenção:** Se existir um valor em `HKLM`, ele tem prioridade absoluta e os demais são ignorados. Por isso, definir apenas `HKCU` pode não ter efeito caso `HKLM` já contenha um valor diferente.

### Comando Completo (executar como Administrador)

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

Após definir esses valores, abra o `CadastroPatrimonio.exe` — ele deverá exibir o **aviso vermelho** com o cronômetro de 2 minutos.

---

## 5. Reset para Testes de Homologação

Para remover a data de primeira execução e resetar o comportamento da aplicação (o app registrará a data atual como nova primeira execução no próximo logon):

```powershell
# Remove os valores de data (o app vai registrar a data atual como nova primeira execução)
reg delete "HKLM\Software\OCS_Inventario" /v PrimeiraExecucao /f 2>$null
reg delete "HKCU\Software\OCS_Inventario" /v PrimeiraExecucao /f 2>$null
Remove-Item "C:\ProgramData\OCS_Inventario\first_run.dat" -ErrorAction SilentlyContinue
```

Para reset completo (inclusive flag de conclusão):

```cmd
reg delete "HKCU\Software\OCS_Inventario" /f
reg delete "HKLM\Software\OCS_Inventario" /f
del /f /q "%ProgramData%\OCS_Inventario\*.*" 2>nul
del /f /q "%LocalAppData%\OCS_Inventario\*.*" 2>nul
```
