# Guia de Implantação da Etapa 3 — Sincronização OCS

Este documento orienta a configuração do processo automatizado de sincronização que atualiza os nomes dos computadores no **OCS Server** para o padrão:

```text
HOSTNAME-NUMERO_DO_PATRIMONIO   (Exemplo: PC-FINANCEIRO-01-12345)
```

---

## 1. Como Funciona a Sincronização

1. O script [sync_ocs_patrimonio.php](file:///C:/Users/lol/.gemini/antigravity/worktrees/OCS1/orchestration_ocs_inventory_system/sync/sync_ocs_patrimonio.php) é executado periodicamente no servidor OCS.
2. Ele busca na tabela `computadores_cadastro` todos os computadores com `sincronizado_ocs = 0`.
3. Ele localiza o equipamento na tabela `hardware` do OCS pelo **`hostname`** (ou pela TAG registrada na Etapa 1).
4. Se o equipamento já foi inventariado pelo OCS Agent, o script atualiza o campo `NAME` na tabela `hardware` para `HOSTNAME-PATRIMONIO` e o `USERID` para o usuário responsável.
5. Todas as informações originais do inventário (processador, memória, discos, softwares instalados, placas de rede) são **100% preservadas**.
6. O registro é marcado como `sincronizado_ocs = 1`.
7. Caso o usuário tenha preenchido o cadastro antes do primeiro inventário do OCS Agent chegar ao servidor, o registro permanece como pendente e será sincronizado automaticamente no próximo ciclo.

---

## 2. Teste Manual e Simulação (Dry-Run)

No terminal do servidor OCS:

```bash
# Simulação sem gravar no banco (Dry-run):
php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php --dry-run

# Execução real imediata:
php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php
```

---

## 3. Configuração do Agendamento (Cron no Linux)

Para rodar a sincronização automaticamente a cada 10 minutos:

1. Abra o editor de crontab no servidor OCS:
   ```bash
   crontab -e
   ```
2. Adicione a seguinte linha no final do arquivo:
   ```cron
   */10 * * * * /usr/bin/php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php > /dev/null 2>&1
   ```
3. Salve e saia. Os logs de execução serão gravados automaticamente em `/var/log/ocs_sync_patrimonio.log`.
