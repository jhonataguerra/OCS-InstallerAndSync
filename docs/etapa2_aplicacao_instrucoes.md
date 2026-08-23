# Guia da Aplicação Executável de Cadastro (Etapa 2)

A aplicação [CadastroPatrimonio.exe](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/client_app/CadastroPatrimonio.exe) é um executável Windows Forms ultra-leve, compilado nativamente contra o **.NET Framework 3.5**, garantindo compatibilidade imediata e sem dependências externas desde o **Windows 7 32 bits (x86)** até o **Windows 11 64 bits**.

---

## 1. Regra de Negócio, Prazos e Forçamento de Preenchimento

A aplicação implementa uma política inteligente de incentivo e obrigatoriedade:

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
"Restam X dias para se tornar         "PRAZO EXPIRADO: O preenchimento
obrigatório."                         agora é OBRIGATÓRIO."
    │                                    │
[ Cronômetro: 10 Segundos ]          [ Cronômetro: 2 Minutos (120s) ]
    │                                    │
    └────────────────┬───────────────────┘
                     ▼
[ Usuário preenche Nome (Só letras) e Patrimônio (Só números) ]
                     │
                     ▼
[ Envia POST HTTP/JSON para API ]
                     │
        ┌────────────┴────────────┐
        ▼ (HTTP 200 OK)           ▼ (Falha de Rede / Erro 500)
[ Grava Flag Definitiva ]   [ Exibe Erro ]
[ Encerra Aplicação    ]   [ NÃO grava Flag ]
                            [ Usuário tenta no próximo logon ]
```

---

## 2. Validação Rigorosa de Campos

* **Nome do Responsável (`txtNome`):**
  * Aceita **estritamente letras** (incluindo caracteres acentuados `á, é, í, ó, ú, ã, õ, ç`), espaços, apóstrofo e hífen.
  * Teclas numéricas e símbolos especiais são rejeitados no teclado e sanitizados automaticamente caso colados via `Ctrl+V`.
* **Número de Patrimônio (`txtPatrimonio`):**
  * Aceita **estritamente números** (dígitos de `0` a `9`).
  * Qualquer letra ou caractere especial é bloqueado no ato da digitação.

---

## 3. Bloqueio Temporário de Fechamento

* **Botão 'X' e Botão Fechar:**
  * Durante a contagem regressiva (10s nos primeiros 7 dias ou 120s após os 7 dias), a tentativa de fechar a janela pelo 'X' ou pelo botão é bloqueada com aviso visual.
  * O botão exibe a contagem regressiva em tempo real: `Fechar (10s)` ... `Fechar (1m 59s)`.
  * Ao zerar o cronômetro, o botão torna-se ativo como **"Fechar Temporariamente"**, permitindo que o usuário feche caso realmente não possa preencher naquele exato momento.
  * No próximo logon do usuário, a tela reaparecerá até que o cadastro seja gravado com sucesso no servidor.

---

## 4. Reset para Testes de Homologação

Para testar o formulário novamente em uma máquina e resetar a data da primeira execução:

```cmd
reg delete "HKCU\Software\OCS_Inventario" /f
reg delete "HKLM\Software\OCS_Inventario" /f
del /f /q "%ProgramData%\OCS_Inventario\*.*" 2>nul
del /f /q "%LocalAppData%\OCS_Inventario\*.*" 2>nul
```
