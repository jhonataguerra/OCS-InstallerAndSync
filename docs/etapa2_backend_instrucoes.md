# Guia de Implantação da Etapa 2 — Banco de Dados e API de Ingestão

Este documento orienta a configuração do banco de dados MySQL/MariaDB e a publicação do endpoint PHP no servidor OCS (ou servidor web dedicado).

---

## 1. Importação do Schema no MySQL / MariaDB

Você pode importar a tabela diretamente no banco de dados do OCS (geralmente chamado `ocsweb`) ou em um banco de dados separado:

```bash
# Executando no terminal do servidor Linux onde roda o MySQL/MariaDB:
mysql -u root -p ocsweb < database/schema.sql
```

A tabela criada será `computadores_cadastro`, com chave única no campo `hostname`.

---

## 2. Publicação dos Arquivos PHP

Copie a pasta `api/` para o diretório raiz do servidor web (Apache) no servidor OCS:

```bash
# Exemplo padrão no Debian/Ubuntu com OCS Server:
mkdir -p /var/www/html/cadastro_api
cp api/config.php /var/www/html/cadastro_api/
cp api/cadastrar.php /var/www/html/cadastro_api/
chown -R www-data:www-data /var/www/html/cadastro_api/
```

---

## 3. Configuração de Credenciais

Edite o arquivo `/var/www/html/cadastro_api/config.php` informando as credenciais de acesso ao MySQL:

```php
return [
    'db_host' => '127.0.0.1',
    'db_port' => 3306,
    'db_name' => 'ocsweb',
    'db_user' => 'ocs',
    'db_pass' => 'sua_senha_aqui',
    'db_charset' => 'utf8mb4',
    'api_token' => '' // Opcional: defina um token secreto se desejar
];
```

---

## 4. Teste de Validação do Endpoint

Você pode validar o funcionamento do endpoint via terminal com um comando `curl`:

```bash
curl -X POST http://SEU_SERVIDOR_OCS/cadastro_api/cadastrar.php \
  -H "Content-Type: application/json" \
  -d '{
    "hostname": "PC-TESTE-01",
    "nome_completo": "Usuario Teste",
    "numero_patrimonio": "99999",
    "setor_local": "TI",
    "usuario_windows": "teste.adm",
    "versao_windows": "Windows 10 Pro",
    "arquitetura": "64 bits",
    "serial_bios": "SN12345678"
  }'
```

**Resposta esperada (HTTP 200):**
```json
{
  "status": "success",
  "message": "Cadastro gravado com sucesso no banco de dados.",
  "data": {
    "hostname": "PC-TESTE-01",
    "numero_patrimonio": "99999"
  }
}
```
